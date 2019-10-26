Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports Microsoft.VisualBasic.Language

Namespace Interpreter.ExecuteEngine

    Public Class VectorLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly values As Expression()

        Sub New(tokens As Token())
            Dim values As New List(Of Expression)
            Dim buffer As New List(Of Token)

            For Each t As Token In tokens
                If t.name <> TokenType.comma Then
                    buffer += t
                Else
                    values += Expression.CreateExpression(buffer.PopAll)
                End If
            Next

            Me.values = values
            Me.type = values _
                .GroupBy(Function(exp) exp.type) _
                .OrderByDescending(Function(g) g.Count) _
                .FirstOrDefault _
               ?.Key
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return values _
                .Select(Function(exp) exp.Evaluate(envir)) _
                .ToArray
        End Function
    End Class
End Namespace