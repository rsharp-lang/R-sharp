Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Public Class PyScanner : Inherits Scanner

        Shared ReadOnly pyKeywords As String() = {
            "and", "as", "assert", "break", "class", "continue", "def", "elif", "else", "except", "false", "finally",
            "for", "from", "global", "if", "import", "in", "is", "lambda", "none", "nonlocal", "not", "or", "pass",
            "raise", "return", "true", "try", "while", "with", "yield"
        }

        <DebuggerStepThrough>
        Sub New(source As [Variant](Of String, CharPtr))
            Call MyBase.New(source)

            Call keywords.Clear()
            Call keywords.Add(pyKeywords).ToArray
            Call nullLiteral.Clear()
            Call nullLiteral.Add("None")

            keepsDelimiter = True
        End Sub

    End Class
End Namespace