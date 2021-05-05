Namespace Interpreter.ExecuteEngine.LINQ

    Public Class QuerySource

        Protected Friend ReadOnly sequence As Expression
        Protected Friend ReadOnly symbol As SymbolDeclare

        Sub New(symbol As SymbolDeclare, sequence As Expression)
            Me.symbol = symbol
            Me.sequence = sequence
        End Sub
    End Class
End Namespace