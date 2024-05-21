#Region "Microsoft.VisualBasic::151be00673411cff77c547736b2df0ee, R#\Runtime\Internal\objects\RConversion\castList.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 315
    '    Code Lines: 241 (76.51%)
    ' Comment Lines: 24 (7.62%)
    '    - Xml Docs: 70.83%
    ' 
    '   Blank Lines: 50 (15.87%)
    '     File Size: 12.74 KB


    '     Module castList
    ' 
    '         Function: CTypeList, dataframe_castList, dictionaryToRList, isEnumType, listElementNames
    '                   listInternal, objCastList, vector_castList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.CType
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.Converts

    ''' <summary>
    ''' cast ``R#`` <see cref="list"/> to <see cref="Dictionary(Of String, TValue)"/>
    ''' </summary>
    Public Module castList

        <Extension>
        Public Function CTypeList(list As list, type As Type, env As Environment) As Object
            Dim table As IDictionary = Activator.CreateInstance(type)
            Dim keyType As Type = type.GenericTypeArguments(Scan0)
            Dim valType As Type = type.GenericTypeArguments(1)
            Dim key As Object
            Dim val As Object

            For Each item In list.slots
                key = any.CTypeDynamic(item.Key, keyType)
                val = RCType.CTypeDynamic(item.Value, valType, env)
                table.Add(key, val)
            Next

            Return table
        End Function

        <Extension>
        Private Function listElementNames(vec As Array, args As list, env As Environment) As String()
            Dim names As String() = CLRVector.asCharacter(args.getByName("names"))

            If names.IsNullOrEmpty Then
                Return vec.Length _
                    .Sequence _
                    .Select(Function(i) $"X_{i}") _
                    .ToArray
            Else
                Return names.uniqueNames
            End If
        End Function

        Public Function vector_castList(vec As Array, args As list, env As Environment) As Object
            If vec.Length = 1 Then
                ' just apply for the primitive data
                If GetVectorElement.IsScalar(vec(Scan0)) Then
                    Dim scalarNames As String() = vec.listElementNames(args, env)
                    Dim list As New list With {.slots = New Dictionary(Of String, Object)}
                    Call list.add(scalarNames(Scan0), vec(Scan0))
                    Return list
                Else
                    Return listInternal(vec.GetValue(Scan0), args, env)
                End If
            End If

            Dim names As String() = vec.listElementNames(args, env)

            If names.Length <> vec.Length Then
                Return Internal.debug.stop({
                    $"size of names is mis-matched with the dimension of your vector!",
                    $"sizeof_names: {names.Length}",
                    $"dimensionof_vector: {vec.Length}"
                }, env)
            End If

            Dim newList As New list With {
                .slots = New Dictionary(Of String, Object)
            }

            For i As Integer = 0 To vec.Length - 1
                Call newList.add(names(i), vec(i))
            Next

            Return newList
        End Function

        ''' <summary>
        ''' cast any clr object to R# tuple list object
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="args"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Friend Function listInternal(obj As Object, args As list, env As Environment) As Object
            Dim type As Type

            If obj Is Nothing Then
                Return Nothing
            Else
                type = obj.GetType
            End If

            If TypeOf obj Is vector Then
                Return vector_castList(DirectCast(obj, vector).data, args, env)
            ElseIf type.IsArray Then
                Return vector_castList(DirectCast(obj, Array), args, env)
            ElseIf GetVectorElement.IsScalar(obj) Then
                Return vector_castList(New Object() {obj}, args, env)
            ElseIf type.ImplementInterface(Of IList) Then
                Dim list As IList = obj
                Dim array As Array = Array.CreateInstance(GetType(Object), length:=list.Count)

                For i As Integer = 0 To list.Count - 1
                    If list.Item(i) Is Nothing Then
                        Call array.SetValue(Nothing, i)
                    Else
                        Dim value As Object = list.Item(i)

                        If Not (DataFramework.IsPrimitive(value.GetType) OrElse value.GetType.IsEnum) Then
                            value = listInternal(value, args, env)
                        End If

                        Call array.SetValue(value, i)
                    End If
                Next

                Return REnv.TryCastGenericArray(array, env)
            End If

            If type.ImplementInterface(Of ICTypeList) Then
                Return DirectCast(obj, ICTypeList).toList
            End If

            Select Case type
                Case GetType(Dictionary(Of String, Object))
                    Return DirectCast(obj, IDictionary).dictionaryToRList(args, env)
                Case GetType(list)
                    Return obj
                Case GetType(vbObject)
                    ' object property as list data
                    Return DirectCast(obj, vbObject).objCastList(args, env)
                Case GetType(dataframe)
                    Return dataframe_castList(obj, args, env)
                Case Else
                    Dim callFunc As GenericFunction = Nothing

                    ' 将字典对象转换为列表对象
                    If type.ImplementInterface(GetType(IDictionary)) Then
                        Return DirectCast(obj, IDictionary).dictionaryToRList(args, env)
                    ElseIf generic.exists(obj, "as.list", obj.GetType, env, callFunc) Then
                        ' implements call the generic function of as.list
                        Return callFunc(obj, args, env)
                    Else
                        Return New vbObject(obj).objCastList(args, env)
                    End If
            End Select
        End Function

        <Extension>
        Friend Function dictionaryToRList(dict As IDictionary, args As list, env As Environment) As list
            Dim objList As New Dictionary(Of String, Object)
            Dim eleType As RType = RType.any
            Dim type As Type = dict.GetType

            If Not type.GetGenericArguments.ElementAtOrDefault(1) Is Nothing Then
                eleType = RType.GetRSharpType(type.GetGenericArguments()(1))
            End If

            ' 20231011 the dictionary is generic, and the value type is already 
            ' the R# runtime type, no needs for convert to list!
            Dim isPrimitive As Boolean = eleType.mode = TypeCodes.boolean OrElse
                eleType.mode = TypeCodes.double OrElse
                eleType.mode = TypeCodes.integer OrElse
                eleType.mode = TypeCodes.string OrElse
                eleType.mode = TypeCodes.list OrElse
                eleType.mode = TypeCodes.formula OrElse
                eleType.mode = TypeCodes.environment OrElse
                eleType.mode = TypeCodes.dataframe

            With dict
                For Each key As Object In .Keys
                    Dim value As Object = .Item(key)

                    If value IsNot Nothing Then
                        If Not isPrimitive Then
                            ' dictionary valye is any object
                            If Not DataFramework.IsPrimitive(value.GetType) Then
                                value = listInternal(value, args, env)
                            End If
                        End If
                    End If

                    Call objList.Add(any.ToString(key), value)
                Next
            End With

            Return New list With {
                .slots = objList,
                .elementType = eleType
            }
        End Function

        <Extension>
        Private Function dataframe_castList(obj As Object, args As list, env As Environment) As Object
            Dim byRow As Boolean = CLRVector.asLogical(args!byrow)(Scan0)
            Dim names As String = any.ToString(REnv.getFirst(args!names), null:=Nothing)
            Dim df As dataframe = DirectCast(obj, dataframe)

            If byRow Then
                Return df.listByRows(names, env)
            Else
                Return df.listByColumns
            End If
        End Function

        Private Function isEnumType(p As PropertyInfo, value As Object) As Boolean
            If p.PropertyType.IsEnum Then
                Return True
            End If

            If Not p.PropertyType Is GetType(Object) Then
                Return False
            End If

            Return value IsNot Nothing AndAlso value.GetType.IsEnum
        End Function

        ''' <summary>
        ''' cast .NET CLR object to R# list object
        ''' </summary>
        ''' <param name="vbobj"></param>
        ''' <param name="args"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <Extension>
        Private Function objCastList(vbobj As vbObject, args As list, env As Environment) As Object
            Dim list As New Dictionary(Of String, Object)
            Dim metadata As PropertyInfo = Nothing

            If vbobj.type.haveDynamicsProperty Then
                metadata = DynamicMetadataAttribute.GetMetadata(vbobj.type.raw)
            End If

            For Each p As KeyValuePair(Of String, PropertyInfo) In vbobj.properties
                Dim value As Object

                Try
                    value = p.Value.GetValue(vbobj.target)
                Catch ex As Exception
                    Return Internal.debug.stop(New Exception($"data reader: {p.Key} [{p.Value.Name} As {p.Value.PropertyType.FullName}]", ex), env)
                End Try

                Dim name As String
                Dim ref As Field = p.Value.GetCustomAttribute(Of Field)
                Dim isEnums As Boolean = isEnumType(p.Value, value)

                If Not metadata Is Nothing Then
                    If metadata Is p.Value Then
                        Continue For
                    End If
                End If

                If Not ref Is Nothing Then
                    name = ref.Name
                Else
                    name = p.Key
                End If

                If value Is Nothing Then
                    list.Add(name, Nothing)
                ElseIf isEnums Then
                    list.Add(name, value.ToString)
                ElseIf TypeOf value Is Array OrElse TypeOf value Is vector Then
                    Dim v As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(value), env)
                    Dim type As Type = v.GetType.GetElementType

                    If Not DataFramework.IsPrimitive(type) Then
                        v = v _
                            .AsObjectEnumerator _
                            .Select(Function(o)
                                        Return listInternal(o, args, env)
                                    End Function) _
                            .ToArray
                    End If

                    list.Add(name, v)
                ElseIf DataFramework.IsPrimitive(value.GetType) Then
                    list.Add(name, value)
                ElseIf Not value.GetType Is GetType(System.Xml.XmlComment) Then
                    value = listInternal(value, args, env)

                    If TypeOf value Is Message Then
                        Return value
                    Else
                        list.Add(name, value)
                    End If
                End If
            Next

            If vbobj.type.haveDynamicsProperty Then
                Dim dynamic As IDynamicsObject = vbobj.target

                For Each name As String In dynamic _
                    .GetNames _
                    .Where(Function(nameKey)
                               Return Not list.ContainsKey(nameKey)
                           End Function)

                    Call list.Add(name, dynamic.GetItemValue(name))
                Next
            End If

            Return New list With {.slots = list}
        End Function
    End Module
End Namespace
