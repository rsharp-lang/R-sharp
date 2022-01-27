Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Public Class FunctionTag : Inherits PythonCodeDOM

    Public Property funcName As String
    Public Property arguments As Expression()
    Public Property stackframe As StackFrame

    Public Overrides Function ToExpression() As Expression
        Return New DeclareNewFunction(funcName, arguments, MyBase.ToExpression(), stackframe)
    End Function

    Public Overrides Function ToString() As String
        Return $"[{level}] {keyword} {funcName}(): {script.JoinBy("; ")}"
    End Function

End Class