Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Public Class JlScanner : Inherits Scanner

        Shared ReadOnly jlKeywords As String() = {
            "baremodule", "begin", "break", "catch", "const",
            "continue", "do", "else", "elseif", "end", "export",
            "false", "finally", "for", "function", "global",
            "if", "import", "include", "let", "local", "macro", "module",
            "quote", "return", "struct", "true", "try", "using",
            "while"
        }

        <DebuggerStepThrough>
        Sub New(source As [Variant](Of String, CharPtr), Optional tokenStringMode As Boolean = False)
            Call MyBase.New(source, tokenStringMode)

            Call keywords.Clear()
            Call keywords.Add(jlKeywords).ToArray
            Call nullLiteral.Clear()
            Call nullLiteral.Add("nothing")

            keepsDelimiter = True
        End Sub
    End Class
End Namespace