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

        Friend Class Escapes

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

        Sub New(source As [Variant](Of String, CharPtr))
            If source Like GetType(String) Then
                Me.code = source.TryCast(Of String).SolveStream
            Else
                Me.code = source.TryCast(Of CharPtr)
            End If
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

                            Dim symbol = token.Value.text

                            If symbol.Length = 1 AndAlso Not symbol Like longOperatorParts Then
                                With populateToken()
                                    If Not .IsNothing Then
                                        Yield .DoCall(Function(t) finalizeToken(t, start))
                                    End If
                                End With
                            End If
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
            ElseIf token.name = TokenType.stringLiteral OrElse token.name = TokenType.stringInterpolation Then
                escape.string = False
            End If

            Return token
        End Function

        ReadOnly delimiter As Index(Of Char) = {" "c, ASCII.TAB, ASCII.CR, ASCII.LF, "="c}
        ReadOnly open As Index(Of Char) = {"[", "{", "("}
        ReadOnly close As Index(Of Char) = {"]", "}", ")"}

        ''' <summary>
        ''' 这里的操作符都是需要多个字符构成的，例如
        ''' 
        ''' + &lt;- 
        ''' + &lt;=
        ''' + &lt;&lt;
        ''' + :>
        ''' + =>
        ''' + &amp;&amp;
        ''' + ||
        ''' + ==
        ''' </summary>
        ReadOnly longOperatorParts As Index(Of Char) = {"<"c, ">"c, "&"c, "|"c, ":"c, "="c, "-"c, "+"c}
        ReadOnly longOperators As Index(Of String) = {"<=", "<-", "&&", "||", ":>", "<<", "->", "=>", ">=", "==", "++", "--"}

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
                        Dim expressionType = If(escape.stringEscape = "`"c, TokenType.stringInterpolation, TokenType.stringLiteral)

                        ' add last string quote symbol
                        buffer += c
                        ' end string escape
                        Return New Token With {
                            .name = expressionType,
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

            ElseIf c Like longOperatorParts Then
                Return populateToken(bufferNext:=c)

            ElseIf c Like open Then
                Return New Token With {.name = TokenType.open, .text = c}
            ElseIf c Like close Then
                Return New Token With {.name = TokenType.close, .text = c}
            ElseIf c = ","c Then
                Return New Token With {.name = TokenType.comma, .text = ","}
            ElseIf c = ";"c Then
                Return New Token With {.name = TokenType.terminator, .text = ";"}
            ElseIf c = "?"c Then
                Return New Token With {.name = TokenType.iif, .text = "?"}
            ElseIf c = ":"c Then
                Return New Token With {.name = TokenType.sequence, .text = ":"}
            ElseIf c = "+"c OrElse c = "*"c OrElse c = "/"c OrElse c = "%"c OrElse c = "^"c OrElse c = "!"c Then
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
                If buffer = 1 AndAlso buffer(Scan0) Like longOperatorParts Then
                    Return populateToken(bufferNext:=c)
                Else
                    buffer += c
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 这个函数的调用会将<see cref="buffer"/>清空
        ''' </summary>
        ''' <param name="bufferNext">
        ''' 这个参数是为了诸如 || 或者 &lt;- 此类需要两个字符构成的操作符的解析而设定的
        ''' 当这个参数不是空的时候，会在清空buffer之后将这个字符添加进入buffer，解决双字符的操作符的解析的问题
        ''' </param>
        ''' <returns></returns>
        Private Function populateToken(Optional bufferNext As Char? = Nothing) As Token
            Dim text As String

            If buffer = 0 Then
                If Not bufferNext Is Nothing Then
                    Call buffer.Add(bufferNext)
                End If

                Return Nothing
            Else
                If Not bufferNext Is Nothing Then
                    If buffer = 1 Then
                        Dim c As Char = buffer(Scan0)
                        Dim t As Char = bufferNext

                        text = c & t

                        If text Like longOperators Then
                            buffer *= 0

                            Return New Token With {
                                .name = TokenType.operator,
                                .text = text
                            }
                        Else

                        End If
                    End If

                    text = buffer.PopAll.CharString
                    buffer += bufferNext.Value
                Else
                    text = buffer.PopAll.CharString
                End If
            End If

            If text.Trim(" "c, ASCII.TAB) = "" OrElse text = vbCr OrElse text = vbLf Then
                Return Nothing
            ElseIf escape.comment AndAlso text.First = "#"c Then
                Return New Token With {.name = TokenType.comment, .text = text}
            Else
                text = text.Trim
            End If

            Select Case text
                Case RInterpreter.lastVariableName
                    Return New Token With {.name = TokenType.identifier, .text = text}
                Case ":>", "+", "-", "*", "=", "/", ">", "<", "~", "<=", ">=", "!", "<-", "&&", "&", "||"
                    Return New Token With {.name = TokenType.operator, .text = text}
                Case ":"
                    Return New Token With {.name = TokenType.sequence, .text = text}
                Case "NULL", "NA", "Inf"
                    Return New Token With {.name = TokenType.missingLiteral, .text = text}
                Case "let", "declare", "function", "return", "as", "integer", "double", "boolean", "string",
                     "const", "imports", "require",
                     "if", "else", "for", "loop", "while",
                     "in", "like", "which", "from", "where", "order", "by", "distinct", "select",
                     "ascending", "descending"
                    Return New Token With {.name = TokenType.keyword, .text = text}
                Case "true", "false", "yes", "no", "T", "F", "TRUE", "FALSE"
                    Return New Token With {.name = TokenType.booleanLiteral, .text = text}
                Case "✔"
                    Return New Token With {.name = TokenType.booleanLiteral, .text = "true"}
                Case Else
                    If text.IsPattern("\d+") Then
                        Return New Token With {.name = TokenType.integerLiteral, .text = text}
                    ElseIf text.IsNumeric Then
                        Return New Token With {.name = TokenType.numberLiteral, .text = text}
                    ElseIf text.IsPattern("[a-z][a-z0-9_\.]*") Then
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