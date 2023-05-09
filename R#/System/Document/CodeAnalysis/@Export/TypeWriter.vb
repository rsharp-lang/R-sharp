Namespace Development.CodeAnalysis

    Public Class TypeWriter

        ReadOnly indent As Integer
        ReadOnly symbol As SymbolTypeDefine

        Sub New(indent As Integer, symbol As SymbolTypeDefine)
            Me.indent = indent
            Me.symbol = symbol
        End Sub

        Public Overrides Function ToString() As String
            Return symbol.ToString
        End Function

    End Class
End Namespace