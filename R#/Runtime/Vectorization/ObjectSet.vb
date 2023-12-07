Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Vectorization

    Public NotInheritable Class ObjectSet

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 将任意类型的序列输入转换为统一的对象枚举序列
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' for a dataframe object, the row observation data will be treated as an object and populate from this function.
        ''' </remarks>
        Public Shared Function GetObjectSet(x As Object, env As Environment, Optional ByRef elementType As RType = Nothing) As IEnumerable(Of Object)
            If x Is Nothing Then
                Return {}
            Else
                Return GetObjectSet(x, env, x.GetType, elementType)
            End If
        End Function

        Private Shared Function extractVector(x As vector, ByRef elementType As RType) As IEnumerable(Of Object)
            elementType = x.elementType
            Return x.data.AsObjectEnumerator
        End Function

        Private Shared Function extractList(x As list, ByRef elementType As RType) As IEnumerable(Of Object)
            ' list value as sequence data
            Dim raw As Object() = x.slots.Values.ToArray
            elementType = MeasureRealElementType(raw).DoCall(AddressOf RType.GetRSharpType)
            Return raw.AsEnumerable
        End Function

        Private Shared Function extractPipeline(x As pipeline, env As Environment, ByRef elementType As RType) As IEnumerable(Of Object)
            elementType = x.elementType
            Return x.populates(Of Object)(env)
        End Function

        Private Shared Function extractClrDictionary1(x As IDictionary(Of String, Object), ByRef elementType As RType) As IEnumerable(Of Object)
            Dim raw As Object() = x.Values.AsEnumerable.ToArray
            elementType = MeasureRealElementType(raw).DoCall(AddressOf RType.GetRSharpType)
            Return raw.AsEnumerable
        End Function

        Private Shared Function extractClrDictionary2(x As IDictionary, ByRef elementType As RType) As IEnumerable(Of Object)
            Dim raw As Object() = x.Values.ToVector
            elementType = MeasureRealElementType(raw).DoCall(AddressOf RType.GetRSharpType)
            Return raw.AsEnumerable
        End Function

        Private Shared Function extractArray(x As Array, ByRef elementType As RType) As IEnumerable(Of Object)
            elementType = x.GetType.GetElementType.DoCall(AddressOf RType.GetRSharpType)
            Return x.AsObjectEnumerator
        End Function

        ''' <summary>
        ''' populate the row observation
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Private Shared Iterator Function extractDataframe(x As dataframe) As IEnumerable(Of Object)
            Dim featureNames As String() = x.colnames
            Dim obj As Dictionary(Of String, Object)

            For Each row As NamedCollection(Of Object) In x.forEachRow
                obj = featureNames _
                    .Select(Function(name, i) (name, row(i))) _
                    .ToDictionary(Function(a) a.name,
                                  Function(a)
                                      Return a.Item2
                                  End Function)

                obj(".row.name") = row.name

                Yield New list With {
                    .slots = obj
                }
            Next
        End Function

        Private Shared Function GetObjectSet(x As Object, env As Environment, type As Type, ByRef elementType As RType) As IEnumerable(Of Object)
            Select Case type
                Case GetType(vector) : Return extractVector(DirectCast(x, vector), elementType)
                Case GetType(list) : Return extractList(DirectCast(x, list), elementType)
                Case GetType(pipeline) : Return extractPipeline(DirectCast(x, pipeline), env, elementType)
                Case GetType(dataframe)
                    elementType = RType.list
                    Return extractDataframe(DirectCast(x, dataframe))
                Case GetType(Group)
                    Return GetObjectSet(DirectCast(x, Group).group, env, elementType)
            End Select

            If type.ImplementInterface(GetType(IDictionary(Of String, Object))) Then
                Return extractClrDictionary1(DirectCast(x, IDictionary(Of String, Object)), elementType)
            ElseIf type.ImplementInterface(Of IDictionary) Then
                Return extractClrDictionary2(DirectCast(x, IDictionary), elementType)
            ElseIf type.IsArray Then
                Return extractArray(DirectCast(x, Array), elementType)
            Else
                elementType = RType.GetRSharpType(x.GetType)
                Return {x}
            End If
        End Function

    End Class
End Namespace