Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Interpreter.SyntaxParser

    Friend MustInherit Class GenericSymbolOperatorProcessor

        Protected MustOverride ReadOnly Property operatorSymbol As String

        Protected MustOverride Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String)) As SyntaxResult

        Public Function JoinBinaryExpression(queue As SyntaxQueue, oplist As List(Of String)) As SyntaxQueue
            If queue.buf = 1 Then
                Return queue
            End If

            Dim nop As Integer = oplist _
                .AsEnumerable _
                .Count(Function(op) op = operatorSymbol)
            Dim buf = queue.buf

            ' 从左往右计算
            For i As Integer = 0 To nop - 1
                For j As Integer = 0 To buf.Count - 1
                    If buf(j) Like GetType(String) AndAlso operatorSymbol = buf(j).VB Then
                        ' j-1 and j+1
                        Dim a = buf(j - 1) ' parameter
                        Dim b = buf(j + 1) ' function invoke
                        Dim exp As SyntaxResult = expression(a, b)

                        Call buf.RemoveRange(j - 1, 3)
                        Call buf.Insert(j - 1, exp)

                        Exit For
                    End If
                Next
            Next

            Return queue
        End Function

    End Class

    Friend Class SyntaxQueue

        Public buf As List(Of [Variant](Of SyntaxResult, String))

    End Class
End Namespace