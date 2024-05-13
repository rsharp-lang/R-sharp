#Region "Microsoft.VisualBasic::ade0c2adc5b942d42f9279a276d27d29, R#\System\Document\Printer\ConsoleSyntaxHighlightPrinter.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 120
    '    Code Lines: 96
    ' Comment Lines: 11
    '   Blank Lines: 13
    '     File Size: 6.20 KB


    '     Module ConsoleSyntaxHighlightPrinter
    ' 
    '         Sub: PrintAsAnsiSequence, PrintAsHtmlSpans, (+2 Overloads) PrintCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
            Dim keyword As New ConsoleFormat With {.Bold = True, .Underline = False, .Foreground = AnsiColor.Blue, .Background = AnsiColor.Black}
            Dim comment As New ConsoleFormat With {.Bold = False, .Underline = False, .Foreground = AnsiColor.Green, .Background = AnsiColor.Black}
            Dim annotation As New ConsoleFormat With {.Bold = True, .Underline = True, .Foreground = AnsiColor.White, .Background = AnsiColor.Black}
            Dim text As New ConsoleFormat With {.Bold = False, .Underline = False, .Foreground = AnsiColor.Magenta, .Background = AnsiColor.Black}
            Dim word As New ConsoleFormat With {.Bold = False, .Underline = False, .Background = AnsiColor.Black, .Foreground = AnsiColor.White, .Inverted = False}
            Dim link As New ConsoleFormat With {.Bold = False, .Underline = True, .Background = AnsiColor.Black, .Foreground = AnsiColor.Blue}
            Dim number As New ConsoleFormat With {.Bold = False, .Underline = False, .Inverted = False, .Background = AnsiColor.Black, .Foreground = AnsiColor.BrightCyan}
            Dim terminator As New ConsoleFormat With {.Bold = True, .Inverted = False, .Background = AnsiColor.Black, .Foreground = AnsiColor.Red, .Underline = False}
            Dim [default] As New ConsoleFormat(Console.ForegroundColor, Console.BackgroundColor)

            Call dev.WriteLine()
            Call dev.WriteLine()

            For Each t As Token In tokens
                Select Case t.name
                    Case TokenType.booleanLiteral, TokenType.missingLiteral
                        Call dev.Write(New TextSpan(t.text.ToUpper, keyword))
                    Case TokenType.keyword
                        Call dev.Write(New TextSpan(t.text, keyword))
                    Case TokenType.newLine : Call dev.WriteLine()
                    Case TokenType.annotation : Call dev.Write(New TextSpan(t.text, annotation))
                    Case TokenType.delimiter : Call dev.Write(New TextSpan(t.text, word))
                    Case TokenType.stringLiteral
                        If t.text.IsURLPattern Then
                            Call dev.Write(New TextSpan("""" & t.text & """", link))
                        Else
                            Call dev.Write(New TextSpan("""" & t.text & """", text))
                        End If
                    Case TokenType.stringInterpolation, TokenType.cliShellInvoke
                        Call dev.Write(New TextSpan(t.text, text))
                    Case TokenType.comment : Call dev.Write(New TextSpan(t.text, comment))
                    Case TokenType.integerLiteral, TokenType.numberLiteral
                        Call dev.Write(New TextSpan(t.text, number))
                    Case TokenType.terminator
                        Call dev.Write(New TextSpan(t.text, terminator))
                    Case Else
                        Call dev.Write(New TextSpan(t.text, word))
                End Select
            Next

            ' restore to default style
            Call dev.Write(New TextSpan(" ", [default]))
            Call dev.WriteLine()
            Call dev.WriteLine()
        End Sub

        <Extension>
        Private Sub PrintAsHtmlSpans(tokens As IEnumerable(Of Token), dev As TextWriter)
            Dim keyword As String = "color: blue;"
            Dim comment As String = "color: green;"
            Dim annotation As String = ""
            Dim text As String = "color: red;"

            For Each t As Token In tokens
                Select Case t.name
                    Case TokenType.booleanLiteral, TokenType.missingLiteral
                        Call dev.Write($"<span style='{keyword}'>{t.text.ToUpper}</span>")
                    Case TokenType.keyword
                        Call dev.Write($"<span style='{keyword}'>{t.text}</span>")
                    Case TokenType.newLine : Call dev.WriteLine()
                    Case TokenType.annotation : Call dev.Write($"<span style='{annotation}'>{t.text}</span>")
                    Case TokenType.delimiter : Call dev.Write(t.text)
                    Case TokenType.stringLiteral, TokenType.stringInterpolation, TokenType.cliShellInvoke
                        Call dev.Write($"<span style='{text}'>{t.text}</span>")
                    Case TokenType.comment
                        Call dev.Write($"<span style='{comment}'>{t.text}</span>")

                    Case Else
                        Call dev.Write(t.text)
                End Select
            Next

            Call dev.WriteLine()
        End Sub
    End Module
End Namespace
