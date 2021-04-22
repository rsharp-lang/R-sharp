Namespace Interpreter.ExecuteEngine.LINQ

    Public MustInherit Class QueryExpression : Inherits Expression

        ReadOnly sequence As Expression

        Friend ReadOnly symbol As SymbolDeclare
        Friend ReadOnly executeQueue As Expression()

        Sub New(symbol As SymbolDeclare, sequence As Expression, execQueue As IEnumerable(Of Expression))
            Me.symbol = symbol
            Me.sequence = sequence
            Me.executeQueue = execQueue.ToArray
        End Sub
    End Class
End Namespace