Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Runtime.Internal.Invokes

    Module math

        <ExportAPI("round")>
        Public Function round(x As Object, Optional decimals% = 0) As Object
            If x.GetType.IsInheritsFrom(GetType(Array)) Then
                Return (From element As Object In DirectCast(x, Array).AsQueryable Select math.round(CDbl(element), decimals)).ToArray
            Else
                Return math.round(CDbl(x), decimals)
            End If
        End Function

        <ExportAPI("log")>
        Public Function log(x As Double(), Optional newBase As Double = stdNum.E) As Object
            Return Runtime.asVector(Of Double)(x) _
                .AsObjectEnumerator(Of Double) _
                .Select(Function(d) stdNum.Log(d, newBase)) _
                .ToArray
        End Function
    End Module
End Namespace