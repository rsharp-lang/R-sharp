Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Class TsScanner : Inherits Scanner

    Shared ReadOnly tsKeywords As String() = {
        "if", "for", "let", "const", "super", "class", "var", "in", "of", "continue",
        "module", "namespace", "function", "return", "typeof", "instanceof", "extends",
        "import", "from",
        "throw"
    }

    Public Sub New(source As [Variant](Of String, CharPtr), Optional tokenStringMode As Boolean = False)
        MyBase.New(source, tokenStringMode)

        Call keywords.Clear()
        Call keywords.Add(tsKeywords).ToArray
        Call nullLiteral.Clear()
        Call nullLiteral.Add("null")
        Call shortOperators.Clear()
        Call shortOperators.AddList("."c, "+"c, "-"c, "*"c, "/"c, "\"c, "!"c, "|"c, "&"c, "^")
        Call longOperatorParts.Add("/"c)

        keepsDelimiter = True
        dollarAsSymbol = True
        commentFlag = "//"
    End Sub
End Class
