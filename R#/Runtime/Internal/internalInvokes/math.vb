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
            If x Is Nothing Then
                Return 0
            End If

            Dim array = Runtime.asVector(Of Object)(x)
            Dim elementType As Type = Runtime.MeasureArrayElementType(array)

            Select Case elementType
                Case GetType(Boolean)
                    Return Runtime.asLogical(array).Select(Function(b) If(b, 1, 0)).Sum
                Case GetType(Integer), GetType(Long), GetType(Short), GetType(Byte)
                    Return Runtime.asVector(Of Long)(x).AsObjectEnumerator(Of Long).Sum
                Case Else
                    Return Runtime.asVector(Of Double)(x).AsObjectEnumerator(Of Double).Sum
            End Select
        End Function
    End Module
End Namespace