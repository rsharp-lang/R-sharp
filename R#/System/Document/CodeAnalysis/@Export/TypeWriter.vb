Imports System.IO

Namespace Development.CodeAnalysis

    Public Class TypeWriter

        ReadOnly indent As Integer
        ReadOnly symbol As SymbolTypeDefine
        ReadOnly ts As TextWriter

        Sub New(indent As Integer, symbol As SymbolTypeDefine, ts As TextWriter)
            Me.indent = indent
            Me.symbol = symbol
            Me.ts = ts
        End Sub

        Public Sub Flush()

        End Sub

        Public Overrides Function ToString() As String
            Return symbol.ToString
        End Function

    End Class
End Namespace