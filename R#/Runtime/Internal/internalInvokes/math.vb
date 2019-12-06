Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Interop
Imports stdNum = System.Math

Namespace Runtime.Internal.Invokes

    Module math

        <ExportAPI("round")>
        Public Function round(x As Double(), Optional decimals% = 0) As Object
            If x.IsNullOrEmpty Then
                Return Nothing
            Else
                Return (From element As Double In x Select stdNum.Round(element, decimals)).ToArray
            End If
        End Function

        <ExportAPI("log")>
        Public Function log(x As Double(), Optional newBase As Double = stdNum.E) As Object
            Return Runtime.asVector(Of Double)(x) _
                .AsObjectEnumerator(Of Double) _
                .Select(Function(d) stdNum.Log(d, newBase)) _
                .ToArray
        End Function

        <ExportAPI("sum")>
        Public Function sum(<RRawVectorArgument> x As Object) As Double

        End Function
    End Module
End Namespace