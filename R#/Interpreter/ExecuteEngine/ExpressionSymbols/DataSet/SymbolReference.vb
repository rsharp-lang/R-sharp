Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class SymbolReference : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public ReadOnly Property symbol As String

        Sub New(symbol As Token)
            Me.symbol = symbol.text
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return envir(symbol).value
        End Function

        Public Overrides Function ToString() As String
            Return $"&{symbol}"
        End Function
    End Class
End Namespace