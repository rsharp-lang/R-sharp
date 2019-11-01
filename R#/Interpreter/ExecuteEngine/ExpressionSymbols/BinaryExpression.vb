Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class BinaryExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim left, right As Expression
        Dim [operator] As String

        Sub New(left As Expression, right As Expression, op$)
            Me.left = left
            Me.right = right
            Me.operator = op
        End Sub

        Shared ReadOnly integers As Index(Of Type) = {
            GetType(Integer), GetType(Integer()),
            GetType(Long), GetType(Long())
        }

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim a As Object = left.Evaluate(envir)
            Dim b As Object = right.Evaluate(envir)
            Dim ta = a.GetType
            Dim tb = b.GetType

            If Program.isException(a) Then
                Return a
            ElseIf Program.isException(b) Then
                Return b
            End If

            If ta Like integers Then
                If tb Like integers Then
                    Select Case [operator]
                        Case "+" : Return Runtime.Core.Add(Of Long, Long, Long)(a, b).ToArray
                        Case "-" : Return Runtime.Core.Minus(Of Long, Long, Long)(a, b).ToArray
                        Case "*" : Return Runtime.Core.Multiply(Of Long, Long, Long)(a, b).ToArray
                        Case "/" : Return Runtime.Core.Divide(Of Long, Long, Double)(a, b).ToArray
                        Case "^" : Return Runtime.Core.Power(Of Long, Long, Double)(a, b).ToArray
                        Case ">" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x > y).ToArray
                        Case "<" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x < y).ToArray
                        Case "!=" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x <> y).ToArray
                        Case "==" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x = y).ToArray
                        Case ">=" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x >= y).ToArray
                        Case "<=" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x <= y).ToArray
                    End Select
                ElseIf tb Is GetType(Double) OrElse tb Is GetType(Double()) Then
                    Select Case [operator]
                        Case "+" : Return Runtime.Core.Add(Of Long, Double, Double)(a, b).ToArray
                        Case "-" : Return Runtime.Core.Minus(Of Long, Double, Double)(a, b).ToArray
                        Case "*" : Return Runtime.Core.Multiply(Of Long, Double, Double)(a, b).ToArray
                        Case "/" : Return Runtime.Core.Divide(Of Long, Double, Double)(a, b).ToArray
                        Case "^" : Return Runtime.Core.Power(Of Long, Double, Double)(a, b).ToArray
                    End Select
                End If
            ElseIf ta Is GetType(String) OrElse tb Is GetType(String) Then
                If [operator] = "&" Then
                    Return DoStringJoin(a, b)
                Else
                    Throw New InvalidExpressionException
                End If
            Else

                If [operator] = "||" OrElse [operator] = "&&" Then
                    Dim op As Func(Of Object, Object, Object)

                    If [operator] = "||" Then
                        op = Function(x, y) x Or y
                    Else
                        op = Function(x, y) x And y
                    End If

                    Return Runtime.Core _
                        .BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                            x:=Core.asLogical(a),
                            y:=Core.asLogical(b),
                            [do]:=op
                        ).ToArray
                End If

            End If

            Throw New NotImplementedException
        End Function

        Public Shared Function DoStringJoin(a As Object, b As Object) As String()
            Dim va = (From element In Runtime.asVector(Of Object)(a).AsQueryable Select Scripting.ToString(element, "NULL")).ToArray
            Dim vb = (From element In Runtime.asVector(Of Object)(b).AsQueryable Select Scripting.ToString(element, "NULL")).ToArray

            Return Runtime.Core.BinaryCoreInternal(Of String, String, String)(va, vb, Function(x, y) x & y).ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"{left} {[operator]} {right}"
        End Function
    End Class
End Namespace