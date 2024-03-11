Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Development

    Public Module ConsoleSyntaxHighlightPrinter

        ''' <summary>
        ''' helper function for print code with syntax highlights
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="dev"></param>
        Public Sub PrintCode(code As String, dev As RContentOutput)
            Call PrintCode(code, dev, dev.env)
        End Sub

        ''' <summary>
        ''' helper function for print code with syntax highlights
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="dev"></param>
        Public Sub PrintCode(code As String, dev As TextWriter, env As OutputEnvironments)
            Dim tokens = New Scanner(
                source:=Strings.Trim(code).Trim(ASCII.LF, ASCII.CR),
                tokenStringMode:=False,
                keepsDelimiter:=True
            ) _
                .GetTokens _
                .ToArray

            If env = OutputEnvironments.Html Then
                Call tokens.PrintAsHtmlSpans(dev)
            Else
                Call tokens.PrintAsAnsiSequence(dev)
            End If
        End Sub

        <Extension>
        Private Sub PrintAsAnsiSequence(tokens As IEnumerable(Of Token), dev As TextWriter)
            Dim keyword As New ConsoleFormat With {.Bold = True, .Underline = False, .Foreground = AnsiColor.Black, .Background = AnsiColor.Black}
            Dim comment As New ConsoleFormat With {.Bold = False, .Underline = False, .Foreground = AnsiColor.Green, .Background = AnsiColor.Black}
            Dim annotation As New ConsoleFormat With {.Bold = True, .Underline = True, .Foreground = AnsiColor.White, .Background = AnsiColor.Black}
            Dim text As New ConsoleFormat With {.Bold = False, .Underline = False, .Foreground = AnsiColor.Magenta, .Background = AnsiColor.Black}

            For Each t As Token In tokens
                Select Case t.name
                    Case TokenType.booleanLiteral, TokenType.keyword, TokenType.missingLiteral
                        Call dev.Write(New TextSpan(t.text.ToUpper, keyword))
                    Case TokenType.newLine : Call dev.WriteLine()
                    Case TokenType.annotation : Call dev.Write(New TextSpan(t.text, annotation))
                    Case TokenType.delimiter : Call dev.Write(t.text)
                    Case TokenType.stringLiteral, TokenType.stringInterpolation, TokenType.cliShellInvoke : Call dev.Write(New TextSpan(t.text, text))
                    Case TokenType.comment : Call dev.Write(New TextSpan(t.text, comment))

                    Case Else
                        Call dev.Write(t.text)
                End Select
            Next

            Call dev.WriteLine()
        End Sub

        <Extension>
        Private Sub PrintAsHtmlSpans(tokens As IEnumerable(Of Token), dev As TextWriter)

        End Sub
    End Module
End Namespace