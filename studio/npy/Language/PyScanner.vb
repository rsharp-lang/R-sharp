Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language
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
            Call shortOperators.Add("."c)

            keepsDelimiter = True
        End Sub

        Public Overrides Function GetTokens() As IEnumerable(Of Token)
            Dim all As Token() = MyBase.GetTokens().ToArray

            ' keyword . symbol . symbol
            ' as.data.frame
            ' keyword is not allowed follow . symbol
            For i As Integer = 0 To all.Length - 1
                If all(i).name = TokenType.keyword Then
                    If all(i + 1) = (TokenType.operator, ".") Then
                        all(i).name = TokenType.identifier
                    End If
                End If
            Next

            Return all
        End Function

    End Class
End Namespace