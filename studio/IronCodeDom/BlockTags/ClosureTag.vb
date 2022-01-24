Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Public Class ClosureTag : Inherits PythonCodeDOM

    Public Property assignTarget As Expression()

    Public Overrides Function ToExpression() As Expression
        Dim value As New ClosureExpression(script.ToArray)

        If assignTarget.IsNullOrEmpty Then
            Return value
        Else
            Return New ValueAssignExpression(assignTarget, value)
        End If
    End Function

End Class
