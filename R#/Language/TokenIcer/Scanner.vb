Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Interpreter

Namespace Language.TokenIcer

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
            Me.code = source.SolveStream
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim token As New Value(Of Token)
            Dim start As Integer = 0

            Do While Not code
                If Not (token = walkChar(++code)) Is Nothing Then
                    Select Case token.Value.name
                        ' 这三个类型的符号都是带有token分割的功能的
                        Case TokenType.comma,
                             TokenType.open,
                             TokenType.close,
                             TokenType.terminator,
                             TokenType.operator

                            With populateToken()
                                If Not .IsNothing Then
                                    Yield .DoCall(Function(t) finalizeToken(t, start))
                                End If
                            End With
                    End Select

                    Yield finalizeToken(token, start)
                End If
            Loop

            If buffer > 0 Then
                With populateToken()
                    If Not .IsNothing Then
                        Yield .DoCall(Function(t) finalizeToken(t, start))
                    End If
                End With
            End If
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

        ReadOnly delimiter As Index(Of Char) = {" "c, ASCII.TAB, ASCII.CR, ASCII.LF, "="c}
        ReadOnly open As Index(Of Char) = {"[", "{", "("}
        ReadOnly close As Index(Of Char) = {"]", "}", ")"}

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
                    ' 在这里不可以将 buffer += c 放在前面
                    ' 否则下面的lastCharIsEscapeSplash会因为添加了一个字符串符号之后失效
                    If Not lastCharIsEscapeSplash Then
                        buffer += c
                        ' end string escape
                        Return New Token With {
                            .name = TokenType.stringLiteral,
                            .text = buffer _
                                .PopAll _
                                .CharString _
                                .GetStackValue(escape.stringEscape, escape.stringEscape)
                        }
                    Else
                        buffer += c
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
            ElseIf c Like open Then
                Return New Token With {.name = TokenType.open, .text = c}
            ElseIf c Like close Then
                Return New Token With {.name = TokenType.close, .text = c}
            ElseIf c = ","c Then
                Return New Token With {.name = TokenType.comma, .text = ","}
            ElseIf c = ";"c Then
                Return New Token With {.name = TokenType.terminator, .text = ";"}
            ElseIf c = ":"c Then
                Return New Token With {.name = TokenType.comma, .text = ":"}
            ElseIf c = "+"c OrElse c = "*"c OrElse c = "/"c OrElse c = "%"c OrElse c = "^"c Then
                Return New Token With {.name = TokenType.operator, .text = c}
            ElseIf c Like delimiter Then
                ' token delimiter
                If buffer > 0 Then
                    Return populateToken()
                Else
                    buffer += c
                    Return populateToken()
                End If
            Else
                buffer += c
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 这个函数的调用会将<see cref="buffer"/>清空
        ''' </summary>
        ''' <returns></returns>
        Private Function populateToken() As Token
            Dim text As String

            If buffer = 0 Then
                Return Nothing
            Else
                text = buffer.PopAll.CharString
            End If

            If text.Trim(" "c, ASCII.TAB) = "" Then
                Return Nothing
            ElseIf escape.comment AndAlso text.First = "#"c Then
                Return New Token With {.name = TokenType.comment, .text = text}
            End If

            Select Case text
                Case RInterpreter.LastVariableName
                    Return New Token With {.name = TokenType.identifier, .text = text}
                Case ":>", "+", "-", "*", "=", "/", ">", "<", "~", "<=", ">=", "!", "<-"
                    Return New Token With {.name = TokenType.operator, .text = text}
                Case "let", "declare", "function", "return", "as", "integer", "double", "boolean", "string", "const", "imports", "require",
                     "if", "else", "for", "loop", "while"
                    Return New Token With {.name = TokenType.keyword, .text = text}
                Case "true", "false", "yes", "no"
                    Return New Token With {.name = TokenType.booleanLiteral, .text = text}
                Case Else
                    If text.IsPattern("\d+") Then
                        Return New Token With {.name = TokenType.integerLiteral, .text = text}
                    ElseIf text.IsPattern(NumericPattern) Then
                        Return New Token With {.name = TokenType.numberLiteral, .text = text}
                    ElseIf text.IsPattern("[a-z_][a-z0-9\.]*") Then
                        Return New Token With {.name = TokenType.identifier, .text = text}
                    End If
#If DEBUG Then
                    Throw New NotImplementedException(text)
#Else
                    Throw New SyntaxErrorException(text)
#End If
            End Select
        End Function
    End Class
End Namespace