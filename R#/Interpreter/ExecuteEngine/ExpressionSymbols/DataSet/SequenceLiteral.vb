Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    ' from:to:steps

    Public Class SequenceLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim from As Expression
        Dim [to] As Expression
        Dim steps As Expression

        Sub New(from As Token, [to] As Token, steps As Token)
            Me.from = Expression.CreateExpression({from})
            Me.to = Expression.CreateExpression({[to]})

            If steps Is Nothing Then
                Me.steps = New Literal(1)
            ElseIf steps.name = TokenType.numberLiteral OrElse steps.name = TokenType.integerLiteral Then
                Me.steps = New Literal(steps)
            Else
                Me.steps = Expression.CreateExpression({steps})
            End If
        End Sub

        Sub New(from As Token(), [to] As Token(), steps As Token())
            Me.from = Expression.CreateExpression(from)
            Me.to = Expression.CreateExpression([to])

            If steps.IsNullOrEmpty Then
                Me.steps = New Literal(1)
            ElseIf steps.isLiteral Then
                Me.steps = New Literal(steps(Scan0))
            Else
                Me.steps = Expression.CreateExpression(steps)
            End If
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim init = from.Evaluate(envir)
            Dim stops = [to].Evaluate(envir)
            Dim offset = steps.Evaluate(envir)

            If {init, stops, offset}.Any(Function(num)
                                             Dim ntype As Type = num.GetType

                                             If ntype Is GetType(Double) OrElse ntype Is GetType(Double()) Then
                                                 Return True
                                             Else
                                                 Return False
                                             End If
                                         End Function) Then
                Dim start As Double = Runtime.getFirst(init)
                Dim steps As Double = Runtime.getFirst(offset)
                Dim ends As Double = Runtime.getFirst(stops)
                Dim seq As New List(Of Double)

                Do While start <= ends
                    seq += start
                    start += steps
                Loop

                Return seq.ToArray
            Else
                Dim start As Long = Runtime.getFirst(init)
                Dim steps As Long = Runtime.getFirst(offset)
                Dim ends As Integer = Runtime.getFirst(stops)
                Dim seq As New List(Of Long)

                Do While start <= ends
                    seq += start
                    start += steps
                Loop

                Return seq.ToArray
            End If
        End Function
    End Class
End Namespace