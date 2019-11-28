Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class ContinuteFor : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return envir("$").value
        End Function

        Public Overrides Function ToString() As String
            Return "next"
        End Function
    End Class
End Namespace