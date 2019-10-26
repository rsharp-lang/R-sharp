Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class SequenceLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim from As Expression
        Dim [to] As Expression
        Dim steps As Expression

        Sub New(from As Token(), [to] As Token(), steps As Token())
            Me.from = Expression.CreateExpression(from)
            Me.to = Expression.CreateExpression([to])

            If steps.IsNullOrEmpty Then
                Me.steps = New Literal(1)
            Else
                Me.steps = Expression.CreateExpression(steps)
            End If
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim init = from.Evaluate(envir)
            Dim stops = [to].Evaluate(envir)
            Dim offset = steps.Evaluate(envir)
            Dim seq As New List(Of Object)

            If {init, stops, offset}.Any(Function(num)
                                             Dim ntype As Type = num.GetType

                                             If ntype Is GetType(Double) OrElse ntype Is GetType(Double()) Then
                                                 Return True
                                             Else
                                                 Return False
                                             End If
                                         End Function) Then
                Dim start As Double = init
                Dim steps As Double = offset
                Dim ends As Double = stops

                Do While start <= ends
                    seq += start
                    start += steps
                Loop
            Else
                Dim start As Long = init
                Dim steps As Long = offset
                Dim ends As Integer = stops

                Do While start <= ends
                    seq += start
                    start += steps
                Loop
            End If

            Return seq.ToArray
        End Function
    End Class
End Namespace