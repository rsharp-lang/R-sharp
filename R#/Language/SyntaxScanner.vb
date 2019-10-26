Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    ''' <summary>
    ''' The token scanner
    ''' </summary>
    Public Class Scanner

        Dim code As CharPtr
        Dim buffer As New List(Of Char)
        Dim escape As New Escapes
        ''' <summary>
        ''' 当前的代码行号
        ''' </summary>
        Dim lineNumber As Integer = 1

        Private Class Escapes

            Friend comment, [string] As Boolean
            Friend stringEscape As Char

            Public Overrides Function ToString() As String
                If comment Then
                    Return "comment"
                ElseIf [string] Then
                    Return $"{stringEscape}string{stringEscape}"
                Else
                    Return "code"
                End If
            End Function
        End Class

        Private ReadOnly Property lastCharIsEscapeSplash As Boolean
            Get
                Return buffer.LastOrDefault = "\"c
            End Get
        End Property

        Sub New(source As String)
            Me.code = source
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim token As New Value(Of Token)
            Dim start As Integer = 0

            Do While Not code
                If Not (token = walkChar(++code)) Is Nothing Then
                    Yield finalizeToken(token, start)
                End If
            Loop
        End Function

        ''' <summary>
        ''' Add stack trace and then try to reset the escape status
        ''' </summary>
        ''' <param name="token"></param>
        ''' <param name="start%"></param>
        ''' <returns></returns>
        Private Function finalizeToken(token As Token, ByRef start%) As Token
            token.span = New CodeSpan With {
                .start = start,
                .stops = code.Position,
                .line = lineNumber
            }
            start = code.Position

            If token.name = TokenType.comment Then
                escape.comment = False
            ElseIf token.name = TokenType.stringLiteral Then
                escape.string = False
            End If

            Return token
        End Function

        Private Function walkChar(c As Char) As Token
            If c = ASCII.LF Then
                lineNumber += 1
            End If

            If escape.comment Then
                If c = ASCII.CR OrElse c = ASCII.LF Then
                    Return New Token With {
                        .name = TokenType.comment,
                        .text = buffer.PopAll.CharString
                    }
                Else
                    buffer += c
                End If
            ElseIf escape.string Then
                If c = escape.stringEscape Then
                    If lastCharIsEscapeSplash Then
                        buffer += c
                    Else
                        ' end string escape
                        Return New Token With {
                            .name = TokenType.stringLiteral,
                            .text = buffer.PopAll.CharString
                        }
                    End If
                Else
                    buffer += c
                End If
            ElseIf c = "#"c AndAlso buffer = 0 Then
                escape.comment = True
                buffer += c
            ElseIf c = "'"c OrElse c = """"c OrElse c = "`" Then
                escape.string = True
                escape.stringEscape = c
                buffer += c
            ElseIf c = " "c OrElse c = ASCII.TAB Then
                ' token delimiter
                If buffer > 0 Then
                    Return populateToken()
                End If
            Else
                buffer += c
            End If

            Return Nothing
        End Function

        Private Function populateToken() As Token
            Dim text As String = buffer.PopAll.CharString

            Select Case text
                Case ":>", "*", "="
                    Return New Token With {.name = TokenType.operator, .text = text}
                Case "let", "declare", "function", "return", "as", "integer", "double", "boolean", "string"
                    Return New Token With {.name = TokenType.keyword, .text = text}
                Case "true", "false", "yes", "no"
                    Return New Token With {.name = TokenType.booleanLiteral, .text = text}
                Case Else
#If DEBUG Then
                    Throw New NotImplementedException(text)
#Else
                    Throw New SyntaxErrorException(text)
#End If
            End Select
        End Function
    End Class
End Namespace