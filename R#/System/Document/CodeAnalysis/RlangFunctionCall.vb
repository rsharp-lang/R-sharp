Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime

Namespace Development.CodeAnalysis

    Public Class RlangFunctionCall

        ReadOnly func As DeclareNewFunction

        Sub New(f As DeclareNewFunction)
            func = f
        End Sub

        Public Function GetScript(args As Dictionary(Of String, Object), env As Environment) As String

        End Function
    End Class
End Namespace