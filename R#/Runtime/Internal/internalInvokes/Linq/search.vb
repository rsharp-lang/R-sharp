Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Internal.Invokes.LinqPipeline

    <Package("search")>
    Module search

        <ExportAPI("binarySearch")>
        Public Function binarySearch(v As BinarySearchFunction(Of Double, SeqValue(Of Double)), x As Double) As Integer
            Dim index As Integer = v.BinarySearch(x)

            If index = -1 Then
                Return -1
            Else
                Dim search As SeqValue(Of Double) = v(index)
                Dim i As Integer = search.i

                Return i
            End If
        End Function

        <ExportAPI("binaryIndex")>
        Public Function binaryIndex(v As Double()) As BinarySearchFunction(Of Double, SeqValue(Of Double))
            Dim i As SeqValue(Of Double)() = v _
                .SeqIterator(offset:=1) _
                .OrderBy(Function(a) a.value) _
                .ToArray
            Dim index As New BinarySearchFunction(Of Double, SeqValue(Of Double))(
                source:=i,
                key:=Function(a) a.value,
                compares:=Function(a, b) a.CompareTo(b)
            )

            Return index
        End Function
    End Module
End Namespace