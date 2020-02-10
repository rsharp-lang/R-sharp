Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' Set Operations
    ''' </summary>
    Module [set]

        ''' <summary>
        ''' 将任意类型的序列输入转换为统一的对象枚举序列
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function getObjectSet(x As Object) As IEnumerable(Of Object)
            If x Is Nothing Then
                Return {}
            End If

            Dim type As Type = x.GetType

            If type Is GetType(vector) Then
                Return DirectCast(x, vector).data.AsObjectEnumerator
            ElseIf type Is GetType(list) Then
                ' list value as sequence data
                Return DirectCast(x, list).slots.Values.AsEnumerable
            ElseIf type.ImplementInterface(GetType(IDictionary(Of String, Object))) Then
                Return DirectCast(x, IDictionary(Of String, Object)).Values.AsEnumerable
            ElseIf type.IsArray Then
                Return DirectCast(x, Array).AsObjectEnumerator
            Else
                Return {x}
            End If
        End Function

        <ExportAPI("intersect")>
        Public Function intersect(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object) As Object
            Dim index_a As New Index(Of Object)(getObjectSet(x))
            Dim inter As Object() = index_a _
                .Intersect(collection:=getObjectSet(y)) _
                .Distinct _
                .ToArray

            Return inter
        End Function

        <ExportAPI("union")>
        Public Function union(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object) As Object
            Dim join As Object() = getObjectSet(x) _
                .JoinIterates(getObjectSet(y)) _
                .Distinct _
                .ToArray
            Return join
        End Function
    End Module
End Namespace