Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    Public Class ElseIfBranch : Inherits IfBranch

        Public Sub New(ifTest As Expression, trueClosure As ClosureExpression, stackframe As StackFrame)
            MyBase.New(ifTest, trueClosure, stackframe)

            stackframe.Method.Method = "elseif_closure"
        End Sub
    End Class
End Namespace