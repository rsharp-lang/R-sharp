Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class SymbolReference : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim symbol As String

        Sub New(symbol As Token)
            Me.symbol = symbol.text
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return envir(symbol)
        End Function
    End Class
End Namespace