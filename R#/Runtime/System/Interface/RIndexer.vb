Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Components.Interface

    Public Interface RIndexer

        Function EvaluateIndexer(expr As Expression, env As Environment) As Object

    End Interface
End Namespace