
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class ReturnValue : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim value As Expression

        Sub New(value As IEnumerable(Of Token))
            Me.value = Expression.CreateExpression(value)
        End Sub

        Sub New(value As Expression)
            Me.value = value
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Me.value.Evaluate(envir)
        End Function

        Public Overrides Function ToString() As String
            Return $"return {value.ToString}"
        End Function
    End Class
End Namespace