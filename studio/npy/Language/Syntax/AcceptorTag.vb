Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements

Public Class AcceptorTag : Inherits PythonCodeDOM

    Public Property target As FunctionInvoke

    Public Overrides Function ToExpression() As Expression
        Return target.CreateInvoke(script, Nothing).expression
    End Function

End Class
