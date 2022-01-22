Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Public Class UsingTag : Inherits PythonCodeDOM

    Public Property symbol As String
    Public Property auto As Expression
    Public Property sourceMap As StackFrame

    Public Overrides Function ToExpression() As Expression
        Dim body As New ClosureExpression(script.ToArray)
        Dim auto As New DeclareNewSymbol(symbol, sourceMap, Me.auto)

        Return New UsingClosure(auto, body, sourceMap)
    End Function

End Class
