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
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace