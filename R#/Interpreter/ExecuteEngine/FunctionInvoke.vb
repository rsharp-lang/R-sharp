Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class FunctionInvoke : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim funcName As String
        Dim parameters As Expression()

        Sub New(tokens As Token())
            Dim params As New List(Of Expression)

            funcName = tokens(Scan0).text

        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace