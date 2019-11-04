Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class IIfExpression : Inherits Expression

        Friend ifTest As Expression
        Friend trueResult As Expression
        Friend falseResult As Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim test As Boolean = Runtime.getFirst(ifTest.Evaluate(envir))

            If test = True Then
                Return trueResult.Evaluate(envir)
            Else
                Return falseResult.Evaluate(envir)
            End If
        End Function
    End Class
End Namespace