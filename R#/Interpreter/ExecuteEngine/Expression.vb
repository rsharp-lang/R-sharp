Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public MustInherit Class Expression

        Public MustOverride Function Evaluate(envir As Environment) As Object

    End Class
End Namespace