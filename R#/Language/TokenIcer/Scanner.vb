#Region "Microsoft.VisualBasic::c042aa61ed79b8323726ca0ce41a0092, D:/GCModeller/src/R-sharp/R#//Language/TokenIcer/Scanner.vb"

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

'   Total Lines: 562
'    Code Lines: 400
' Comment Lines: 90
'   Blank Lines: 72
'     File Size: 23.67 KB


'     Class Scanner
' 
'         Properties: lastCharIsEscapeSplash
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: finalizeToken, GetRKeywords, GetTokens, isLINQKeyword, MeasureToken
'                   populateToken, walkChar
'         Class Escapes
' 
'             Function: ToString
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

<Assembly: InternalsVisibleTo("npy")>
<Assembly: InternalsVisibleTo("nsas")>

Namespace Language.TokenIcer

    ''' <summary>
    ''' The token scanner
    ''' </summary>
    Public Class Scanner

        Dim code As CharPtr
        Dim buffer As New CharBuffer
        Dim escape As New Escapes
        ''' <summary>
        ''' 当前的代码行号
        ''' </summary>
        Dim lineNumber As Integer = 1
        Dim lastPopoutToken As Token
        Dim tokenStringMode As Boolean
        Dim isStringInterpolateClose As Boolean = False

        Protected commentFlag As String = "#"
        ''' <summary>
        ''' keeps the blankspace, cr, lf, tab as the delimiter/expression terminator
        ''' could be used for parse the typescript,javascript,python
        ''' </summary>
        Protected keepsDelimiter As Boolean = False
        ''' <summary>
        ''' use the dollar symbol $ as symbol name?
        ''' 
        ''' set false for R# language by default, and should turn on this option for 
        ''' javascript/typescript language parser.
        ''' </summary>
        Protected dollarAsSymbol As Boolean = False
        Protected ReadOnly keywords As New Index(Of String)(Rkeywords.Objects)
        Protected ReadOnly nullLiteral As New Index(Of String)(RNullLiteral)
        Protected ReadOnly shortOperators As New Index(Of Char)(RshortOperators)

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
                Return buffer.GetLastOrDefault = "\"c
            End Get
        End Property

        <DebuggerStepThrough>
        Sub New(source As [Variant](Of String, CharPtr), Optional tokenStringMode As Boolean = False)
            If source Like GetType(String) Then
                Me.code = source.TryCast(Of String).SolveStream
            Else
                Me.code = source.TryCast(Of CharPtr)
            End If

            Me.tokenStringMode = tokenStringMode
        End Sub

        Public Overridable Iterator Function GetTokens() As IEnumerable(Of Token)
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

                            If symbol.Length = 1 AndAlso
                                (Not symbol Like longOperatorParts) AndAlso
                                (Not symbol = "@") Then

                                With populateToken()
                                    If Not .IsNothing Then
                                        Yield .DoCall(Function(t) finalizeToken(t, start))
                                    End If
                                End With
                            End If
                    End Select

                    Yield finalizeToken(token, start)

                    If TypeOf token.Value Is JoinToken Then
                        Yield finalizeToken(CType(token, JoinToken).next, start)
                    End If
                End If

                If tokenStringMode AndAlso isStringInterpolateClose Then
                    Exit Do
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
            lastPopoutToken = token

            If token.name = TokenType.comment Then
                escape.comment = False
            ElseIf token.name = TokenType.stringLiteral OrElse
                   token.name = TokenType.stringInterpolation OrElse
                   token.name = TokenType.cliShellInvoke OrElse
                   token.name = TokenType.regexp Then

                escape.string = False
            End If

            isStringInterpolateClose = token.name = TokenType.close AndAlso token.text = "}"

            Return token
        End Function

        ReadOnly stringLiteralSymbols As Index(Of Char) = {""""c, "'"c, "`"c}
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
        Protected ReadOnly longOperatorParts As Index(Of Char) = {"<"c, ">"c, "&"c, "|"c, ":"c, "="c, "-"c, "+"c, "!"}

        Shared ReadOnly longOperators As Index(Of String) = {
            "<=", ">=", "==", "!=",
            "&&", "||",
            "<-", "<<", "->", "=>",
            "++", "--",
            "+=", "-=", "*=", "/=",
            "|>", ":>", "::"
        }
        Shared ReadOnly RshortOperators As Char() = {
            "$"c, ' member getter/setter if found between two symbol, regexp symbol if found it before a string literal value
            "@"c, ' array loop getter if found between two symbol, commandline shell if found it before a string expression
            "+"c, "*"c, "/"c, "%"c, ' math operators
            "^"c, ' math power
            "!"c, ' factorial symbol if this operator is found after an expression, or the logical not if found it before an expression
            "→"c  ' a special character for represents the pipeline forward operator(:> or |>)
        }

        Friend Shared ReadOnly Rkeywords As Index(Of String) = {
            "let", "declare", "function", "return", "as", "integer", "double", "boolean", "string",
            "const", "imports", "require", "library",
            "if", "else", "for", "loop", "while", "repeat", "step", "break", "next",
            "between", "in", "like", "from", "where", "order", "by", "distinct", "select", "take", "skip", "into", "aggregate", "join", "on",
            "ascending", "descending",
            "suppress",
            "typeof", "modeof", "valueof",
            "using",
            "new", "try", "switch", "of"
        }

        Private Function isLINQKeyword(word As String) As Boolean
            Dim lwd As String = Strings.LCase(word)

            Select Case lwd
                ' Linq中没有if表达式
                Case "if"
                    Return False
                Case Else
                    Return Strings.LCase(word) Like Rkeywords
            End Select
        End Function

        Public Shared Function GetRKeywords() As String()
            Return Rkeywords.Objects
        End Function

        Private Function walkChar(c As Char) As Token
            If c = ASCII.LF Then
                lineNumber += 1
            End If

            If escape.comment Then
                If c = ASCII.CR OrElse c = ASCII.LF Then
                    Dim commentText As New String(buffer.PopAllChars)

                    ' 20220116 makes bugs fixed for python indent 
                    buffer += c

                    Return New Token With {
                        .name = TokenType.comment,
                        .text = commentText
                    }
                Else
                    buffer += c
                End If
            ElseIf escape.string Then
                If c = escape.stringEscape Then
                    ' 在这里不可以将 buffer += c 放在前面
                    ' 否则下面的lastCharIsEscapeSplash会因为添加了一个字符串符号之后失效
                    If Not lastCharIsEscapeSplash Then
                        Dim expressionType As TokenType

                        ' add last string quote symbol
                        buffer += c

                        If buffer(Scan0) = "@"c Then
                            ' cli shell invoke
                            expressionType = TokenType.cliShellInvoke
                        ElseIf buffer(Scan0) = "$"c Then
                            expressionType = TokenType.regexp
                        Else
                            expressionType = If(escape.stringEscape = "`"c, TokenType.stringInterpolation, TokenType.stringLiteral)
                        End If

                        ' end string escape
                        Return New Token With {
                            .name = expressionType,
                            .text = buffer _
                                .PopAllChars _
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
            ElseIf c = "#"c Then

                Dim token As Token = populateToken(Nothing)

                escape.comment = True
                buffer += c

                Return token

            ElseIf c = "'"c OrElse c = """"c OrElse c = "`" Then
                Dim token As Token = populateToken(Nothing)

                escape.string = True
                escape.stringEscape = c
                buffer += c

                Return token

            ElseIf c = "!"c AndAlso code.PeekNext(6) = "script" Then
                Call code.PopNext(6)

                ' special name
                If buffer = 0 Then
                    Return New Token With {
                        .text = "!script",
                        .name = TokenType.identifier
                    }
                Else
                    Throw New SyntaxErrorException
                End If

            ElseIf c = "@"c Then

                Return populateToken(bufferNext:=c)

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
            ElseIf c = "~"c Then
                Return New Token With {.name = TokenType.operator, .text = "~"}
            ElseIf c = "?"c Then
                Return New Token With {.name = TokenType.iif, .text = "?"}
            ElseIf c = ":"c Then
                Return New Token With {.name = TokenType.sequence, .text = ":"}
            ElseIf c Like shortOperators Then
                Dim peekNext As Char = code.Current

                If c = "$"c AndAlso peekNext Like stringLiteralSymbols Then
                    Static [like] As (TokenType, String) = (TokenType.keyword, "like")

                    ' 20200528
                    ' 如果上一个单词是一个对象引用符号或者小括号
                    ' 则可能是symbol index引用
                    ' 则$符号不应该被加入到缓存之中
                    If lastPopoutToken Is Nothing OrElse
                        lastPopoutToken.name = TokenType.open OrElse
                        lastPopoutToken.name = TokenType.operator OrElse
                        lastPopoutToken.name = TokenType.comma OrElse
                        lastPopoutToken.name = TokenType.terminator Then

                        If buffer > 0 Then
                            ' a$"name b"
                            Return populateToken().joinNext(c)
                        Else
                            buffer += "$"c
                            Return Nothing
                        End If
                    ElseIf Not (lastPopoutToken.name = TokenType.identifier OrElse lastPopoutToken.name = TokenType.close) Then
                        ' 正则表达式语法
                        ' $"regexp"
                        buffer += "$"c
                        Return Nothing
                        'ElseIf lastPopoutToken.name = TokenType.identifier OrElse lastPopoutToken.name = TokenType.close Then
                        '    Return populateToken().joinNext(c)
                        'Else
                        '    Throw New SyntaxErrorException
                    End If
                ElseIf c = "."c AndAlso (peekNext = "0"c OrElse (peekNext >= "1"c AndAlso peekNext <= "9")) Then
                    ' 20220208
                    ' makes bugs fixed for python/julia/typescript language parser
                    buffer += "."c
                    Return Nothing
                End If

                Return New Token With {.name = TokenType.operator, .text = c}
            ElseIf c Like delimiter Then
                ' token delimiter
                If buffer > 0 Then
                    If keepsDelimiter Then
                        Return populateToken(bufferNext:=c)
                    Else
                        Return populateToken()
                    End If
                Else
                    buffer += c
                    Return populateToken()
                End If
            Else
                If buffer = 1 AndAlso buffer(Scan0) Like longOperatorParts Then
                    Return populateToken(bufferNext:=c)
                ElseIf keepsDelimiter AndAlso buffer = 1 AndAlso buffer(Scan0) Like delimiter Then
                    Return populateToken(bufferNext:=c)
                ElseIf lastPopoutToken IsNot Nothing AndAlso
                    (lastPopoutToken.name = TokenType.identifier OrElse lastPopoutToken.name = TokenType.keyword) AndAlso
                    buffer = 1 AndAlso
                    buffer(Scan0) = "@" Then

                    Return populateToken(bufferNext:=c)
                Else
                    buffer += c
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' .128MB
        ''' _
        ''' .
        ''' ...
        ''' _128MB
        ''' </summary>
        Public Const RSymbol$ = "([_\.])*[a-z0-9_\.]*"

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
                    buffer += bufferNext
                End If

                Return Nothing
            Else
                If Not bufferNext Is Nothing Then
                    If bufferNext = "-"c AndAlso (buffer.Last = "e"c OrElse buffer.Last = "E"c) Then
                        ' xxxE-xxx科学计数法
                        buffer += bufferNext.Value
                        Return Nothing
                    Else
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
                            ElseIf text = commentFlag Then
                                ' handling comment for javascript language parser
                                escape.comment = True
                                buffer += c

                                Return Nothing
                            Else

                            End If
                        End If

                        text = New String(buffer.PopAllChars)
                        buffer += bufferNext.Value
                    End If
                ElseIf buffer = "@"c Then
                    Return Nothing
                ElseIf buffer = "$"c Then
                    If dollarAsSymbol Then
                        Return New Token With {
                            .name = TokenType.identifier,
                            .text = New String(buffer.PopAllChars)
                        }
                    Else
                        Return Nothing
                    End If
                Else
                    text = New String(buffer.PopAllChars)
                End If
            End If

            If text = "@"c Then
                Return New Token With {.name = TokenType.operator, .text = "@"}
            ElseIf text.First = "@"c Then
                Return New Token With {.name = TokenType.annotation, .text = text}
            ElseIf text.Trim(" "c, ASCII.TAB) = "" OrElse text = vbCr OrElse text = vbLf Then
                If keepsDelimiter Then
                    If text = vbCr OrElse text = vbLf Then
                        Return New Token With {.name = TokenType.newLine, .text = text}
                    Else
                        Return New Token With {.name = TokenType.delimiter, .text = text}
                    End If
                Else
                    Return Nothing
                End If
            ElseIf escape.comment AndAlso text.StartsWith(commentFlag) Then
                Return New Token With {.name = TokenType.comment, .text = text}
            Else
                Return MeasureToken(text, keywords, nullLiteral, AddressOf isLINQKeyword)
            End If
        End Function

        Friend Shared ReadOnly RNullLiteral As String() = {"NULL", "NA", "Inf", "NA_real_", "NA_integer_"}

        Public Shared Function MeasureToken(text As String,
                                            keywords As Index(Of String),
                                            nullLiteral As Index(Of String),
                                            isLinqKeyword As Func(Of String, Boolean)) As Token
            text = text.Trim

            If text Like keywords OrElse isLinqKeyword(text) Then
                ' 在这里转换为小写是因为
                ' R关键词都是小写的
                ' 但是LINQ的关键词是不区分大小写的
                ' 为了保持二者兼容而设定的
                Return New Token With {
                    .name = TokenType.keyword,
                    .text = text.ToLower
                }
            End If

            Select Case text
                'Case RInterpreter.lastVariableName
                '    Return New Token With {.name = TokenType.identifier, .text = text}
                Case "|>", ":>", "+", "-", "*", "=", "/", ">", "<", "~", "<=", ">=", "!", "<-", "&&", "&", "||", "$", "@", "→"
                    Return New Token With {.name = TokenType.operator, .text = text}
                Case ":"
                    Return New Token With {.name = TokenType.sequence, .text = text}
                Case "true", "false", "yes", "no", "TRUE", "FALSE", "True", "False" ' , "T", "F"
                    ' 20200216 在R语言之中，T和F这两个符号是默认值为TRUE或者FALSE的变量
                    ' 当对T或者F进行赋值之后，原来所拥有的逻辑值将会被新的值替代
                    ' 所以在这里取消掉T以及F对逻辑值的常量表示
                    '
                    ' > T
                    ' [1] TRUE
                    ' > T <- 99
                    ' > T
                    ' [1] 99
                    Return New Token With {.name = TokenType.booleanLiteral, .text = text}
                Case "✔"
                    Return New Token With {.name = TokenType.booleanLiteral, .text = "true"}
                Case "_", ".", "..."
                    Return New Token With {.name = TokenType.identifier, .text = text}
                Case Else
                    If text Like nullLiteral Then
                        Return New Token With {.name = TokenType.missingLiteral, .text = text}
                    ElseIf text.IsPattern("\d+") Then
                        Return New Token With {.name = TokenType.integerLiteral, .text = text}
                    ElseIf Double.TryParse(text, Nothing) Then
                        Return New Token With {.name = TokenType.numberLiteral, .text = text}
                    ElseIf text.IsPattern("\d+L") Then
                        ' integer literal
                        Return New Token With {.name = TokenType.integerLiteral, .text = CInt(Val(text)).ToString}
                    ElseIf text.IsPattern(NumericPattern & "F") Then
                        ' float literal
                        Return New Token With {.name = TokenType.numberLiteral, .text = Val(text).ToString}
                    ElseIf text.IsPattern(RSymbol) Then
                        Return New Token With {.name = TokenType.identifier, .text = text}
                    End If
#If DEBUG Then
                    Throw New NotImplementedException(text)
#Else
                    ' Throw New SyntaxErrorException(text)
                    Return New Token With {
                        .name = TokenType.invalid,
                        .text = text
                    }
#End If
            End Select
        End Function
    End Class
End Namespace
