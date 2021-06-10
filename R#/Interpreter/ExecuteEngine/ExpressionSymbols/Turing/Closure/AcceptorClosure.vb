Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    Public Class AcceptorClosure : Inherits ClosureExpression

        Public Sub New(code() As Expression)
            MyBase.New(code)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            If TypeOf program.Last Is FunctionInvoke Then
                Dim arguments = envir.acceptorArguments
                Dim newProgram As New Program(program.Take(program.lines - 1))
            Else
                Return MyBase.Evaluate(envir)
            End If
        End Function
    End Class
End Namespace