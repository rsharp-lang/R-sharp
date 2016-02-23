Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports RDotNET.SymbolicExpressionExtension

''' <summary>
''' Convert the R object into a .NET object from the specific type schema information.
''' (将R之中的对象内存数据转换为.NET之中指定的对象实体)
''' </summary>
''' <remarks></remarks>
''' 
<PackageNamespace("R.Serialization",
                  Description:="Convert the R object into a .NET object from the specific type schema information.",
                  Category:=APICategories.UtilityTools,
                  Publisher:="xieguigang@gcmodeller.org")>
Public Module Serialization

    ''' <summary>
    ''' Deserialize the R object into a specific .NET object. <see cref="RDotNET.SymbolicExpression"></see>  =====> "T"
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="RData"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 反序列化的规则：
    ''' 1. S4对象里面的Slot为对象类型之中的属性
    ''' 2. 任何对象属性都会被表示为数组
    ''' </remarks>
    Public Function LoadFromStream(Of T As Class)(RData As RDotNET.SymbolicExpression) As T
        Dim value As Object = __loadFromStream(RData, GetType(T), 1)
        Return DirectCast(value, T)
    End Function

    ''' <summary>
    ''' Needs your manual type casting in your program. 
    ''' </summary>
    ''' <param name="RData"></param>
    ''' <param name="Type"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("RStream.Load")>
    Public Function LoadRStream(<Parameter("R.S4Object")> RData As RDotNET.SymbolicExpression, Type As Type) As Object
        Dim value As Object = __loadFromStream(RData, Type, 1)
        Return value
    End Function

    ''' <summary>
    ''' The recursive operation of the S4Object in R starts from here. this recursive operation will stop when the property value is not a S4Object.
    ''' (这个可能是一个递归的过程，一直解析到各个属性的R类型不再是S4对象类型为止)
    ''' </summary>
    ''' <param name="RData"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function InternalLoadS4Object(RData As RDotNET.SymbolicExpression, TypeInfo As System.Type, DebugLevel As Integer) As Object
        Dim Mappings = Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFrameColumnAttribute.LoadMapping(TypeInfo)
        Dim obj As Object = Activator.CreateInstance(TypeInfo)

        Call Console.WriteLine("[DEBUG] {0}  ---> R.S4Object (""{1}"")", TypeInfo.FullName, String.Join("; ", RData.GetAttributeNames))

        For Each Slot In Mappings
            Dim RSlot As RDotNET.SymbolicExpression = RData.GetAttribute(Slot.Key.Name)
            Dim value As Object = __loadFromStream(RSlot, Slot.Value.PropertyType, DebugLevel)

            Call __valueMapping(value, Slot.Value, obj:=obj)
        Next

        Return obj
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="pInfo"></param>
    ''' <param name="obj">对象实例</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function __valueMapping(value As Object, pInfo As System.Reflection.PropertyInfo, ByRef obj As Object) As Boolean
        Dim pTypeInfo As System.Type = pInfo.PropertyType

        If pTypeInfo.HasElementType Then
            Call InternalMappingCollectionType(value, pInfo, obj, pTypeInfo)
        Else
            Call InternalRVectorToNETProperty(pTypeInfo:=value.GetType, value:=value, obj:=obj, pInfo:=pInfo)
        End If

        Return True
    End Function

    ''' <summary>
    ''' All of the object in R is a vector, so that we needs this function to convert the R vector to a property value.
    ''' </summary>
    ''' <param name="pTypeInfo"></param>
    ''' <param name="value"></param>
    ''' <param name="pInfo"></param>
    ''' <param name="obj"></param>
    ''' <remarks></remarks>
    Private Sub InternalRVectorToNETProperty(pTypeInfo As System.Type, value As Object, pInfo As System.Reflection.PropertyInfo, ByRef obj As Object)
        If Not pTypeInfo.HasElementType Then '目标映射的属性不是数组，但是R之中的几乎所有的对象都是使用数组表示的，故而在这里可能需要取第一个元素
            Call pInfo.SetValue(obj, value)
            Return
        End If

        Dim SourceList As Object() = (From val As Object In DirectCast(value, System.Collections.IEnumerable) Select val).ToArray

        If SourceList.IsNullOrEmpty Then
            Call pInfo.SetValue(obj, Nothing)
        Else
            value = SourceList.First
            Call pInfo.SetValue(obj, value)
        End If
    End Sub

    ''' <summary>
    ''' Object() to T()()
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="pInfo"></param>
    ''' <param name="obj"></param>
    ''' <param name="pTypeInfo"></param>
    ''' <remarks></remarks>
    Private Sub InternalMappingCollectionType(value As Object, pInfo As System.Reflection.PropertyInfo, ByRef obj As Object, pTypeInfo As System.Type)
        Dim EleTypeInfo As Type = pTypeInfo.GetElementType
        Dim SourceList = (From val As Object In DirectCast(value, System.Collections.IEnumerable) Select val).ToArray
        Dim List = Array.CreateInstance(EleTypeInfo, SourceList.Count)

        For i As Integer = 0 To SourceList.Count - 1
            Call List.SetValue(SourceList(i), i)
        Next

        Call pInfo.SetValue(obj, List)
    End Sub

    ''' <summary>
    ''' Load the R symbolic expression data recursivly start from here.
    ''' </summary>
    ''' <param name="RData"></param>
    ''' <param name="TypeInfo"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function __loadFromStream(RData As RDotNET.SymbolicExpression, TypeInfo As System.Type, DebugLevel As Integer) As Object

        Call Console.WriteLine(New String("."c, DebugLevel) & ">   " & RData.Type.ToString)

        Select Case RData.Type

            Case Internals.SymbolicExpressionType.S4

                'Load the R symbolic expression data recursivly start from here.
                Return InternalLoadS4Object(RData, TypeInfo, DebugLevel + 1)

            Case Internals.SymbolicExpressionType.LogicalVector
                Return RData.AsLogical.ToArray
            Case Internals.SymbolicExpressionType.CharacterVector
                Return RData.AsCharacter.ToArray
            Case Internals.SymbolicExpressionType.IntegerVector
                Return RData.AsInteger.ToArray
            Case Internals.SymbolicExpressionType.NumericVector
                Return RData.AsNumeric.ToArray
            Case Internals.SymbolicExpressionType.List
                Return __createMatrix(RData, TypeInfo, DebugLevel + 1)


            Case Else
                'Throw New NotImplementedException(RData.Type.ToString)
                Return Nothing
        End Select
    End Function

    Private Function __createMatrix(RData As RDotNET.SymbolicExpression, TypeInfo As System.Type, DebugLevel As Integer) As Object
        Dim List = RData.AsList.ToArray
        Dim Matrix = (From vec In List Select __loadFromStream(vec, TypeInfo, DebugLevel)).ToArray
        Return Matrix
    End Function
End Module
