Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    Public Class AcceptorClosure : Inherits ClosureExpression

        Public Sub New(code() As Expression)
            MyBase.New(code)
        End Sub
    End Class
End Namespace