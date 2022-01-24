Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Public Class IfTag : Inherits PythonCodeDOM

    Public Property test As Expression
    Public Property stackframe As StackFrame

    Public Overrides Function ToExpression() As Expression
        Return New IfBranch(test, DirectCast(MyBase.ToExpression(), ClosureExpression), stackframe)
    End Function

End Class

Public Class ElseTag : Inherits PythonCodeDOM

    Public Property stackframe As StackFrame

    Public Overrides Function ToExpression() As Expression
        Return New ElseBranch(MyBase.ToExpression(), stackframe)
    End Function

End Class