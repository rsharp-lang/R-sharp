#Region "Microsoft.VisualBasic::7fb2f56a68ce3d12e834bf3432bae640, ..\R-sharp\R#\Interpreter\Language\TokenIcer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Interpreter.Language
Imports RSharpLang = SMRUCC.Rsharp.Interpreter.Language.Tokens

Namespace Interpreter.Language

    ''' <summary>
    ''' Convert the text data as the script model
    ''' </summary>
    Public Module TokenIcer

        ''' <summary>
        ''' Parsing the input script text as the statement models
        ''' </summary>
        ''' <param name="s$"></param>
        ''' <returns></returns>
        <Extension> Public Iterator Function Parse(s$) As IEnumerable(Of Statement(Of Tokens))
            Dim buffer As New Pointer(Of Char)(Strings.Trim(s$))
            Dim it As New Value(Of Statement(Of Tokens))
            Dim line% ' the line number
            Dim args As ParserArgs = ParserArgs.[New]

            Do While Not buffer.EndRead
                If Not (it = buffer.Parse(Nothing, line, args)) Is Nothing Then
                    Yield it
                End If
            Loop
        End Function

        Private Structure ParserArgs

            Dim PiplineOpen As Boolean
            Dim VerticalBarOpen As Boolean
            Dim statementBuffer As List(Of Char)
            Dim VectorNormOpen As Boolean
            Dim AbsClose As Boolean

            Sub New(args As ParserArgs)
                With args
                    PiplineOpen = .PiplineOpen
                    VectorNormOpen = .VectorNormOpen
                    VerticalBarOpen = .VerticalBarOpen
                    statementBuffer = .statementBuffer
                End With
            End Sub

            Public Function CloseAbs() As ParserArgs
                Return New ParserArgs(args:=Me) With {
                    .AbsClose = True
                }
            End Function

            Public Function OpenVectorNorm() As ParserArgs
                Return New ParserArgs(args:=Me) With {
                    .VectorNormOpen = True
                }
            End Function

            Public Function OpenVerticalBar() As ParserArgs
                Return New ParserArgs(args:=Me) With {
                    .VerticalBarOpen = True
                }
            End Function

            Public Shared Function [New]() As ParserArgs
                Return New ParserArgs With {
                    .statementBuffer = New List(Of Char),
                    .VerticalBarOpen = False,
                    .PiplineOpen = False
                }
            End Function
        End Structure

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="parent"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 对于``{}``而言，其含义为当前的token的closure
        ''' 对于``()``而言，其含义为当前的token的innerStack
        ''' 对于``[]``而言，起含义为当前的token的innerStack，与``()``的含义几乎一致
        ''' </remarks>
        <Extension> Private Function Parse(buffer As Pointer(Of Char),
                                           ByRef parent As List(Of Statement(Of RSharpLang)),
                                           ByRef line%,
                                           ByRef args As ParserArgs) As Statement(Of Tokens)

            Dim quotOpen As Boolean = False
            Dim commentOpen As Boolean = False ' 当出现注释符的时候，会一直持续到遇见换行符为止
            Dim tmp As New List(Of Char)
            Dim tokens As New List(Of Token)
            Dim last As Statement(Of RSharpLang)
            Dim varDefInit = Function()
                                 If tokens.Count = 0 AndAlso tmp.SequenceEqual("var") Then
                                     Return True
                                 Else
                                     Return False
                                 End If
                             End Function
            Dim bufferEquals = Function(c As Char)
                                   If tmp.Count = 1 AndAlso tmp.First = c Then
                                       Return True
                                   Else
                                       Return False
                                   End If
                               End Function
            Dim createNewToken = _
 _
                Sub()
                    If tmp = 0 Then Return

                    ' 创建除了字符串之外的其他的token
                    If varDefInit() Then
                        tokens += New Token(RSharpLang.Variable, "var")
                    ElseIf tmp = "<-" Then
                        tokens += New Token(RSharpLang.LeftAssign, "<-")
                    ElseIf tmp = "TRUE" OrElse tmp = "FALSE" OrElse tmp = "true" OrElse tmp = "false" Then
                        tokens += New Token(RSharpLang.Boolean) With {
                            .Value = New String(tmp)
                        }
                    Else
                        tokens += New Token(RSharpLang.Object) With {
                            .Value = New String(tmp)
                        }
                    End If

                    tmp *= 0
                End Sub

            Do While Not buffer.EndRead
                Dim c As Char = ++buffer

                args.statementBuffer += c
#Const DEBUG = False
#If DEBUG Then
                Call Console.Write(c)
#End If
                If quotOpen Then ' 当前所解析的状态为字符串解析
                    If c = ASCII.Quot AndAlso Not tmp.StartEscaping Then
                        ' 当前的字符为双引号，并且不是转义状态，则结束字符串
                        tokens += New Token With {
                            .name = RSharpLang.String,
                            .Value = New String(tmp)
                        }
                        tmp *= 0
                        quotOpen = False
                    Else
                        ' 任然是字符串之中的一部分字符，则继续添加进入tmp之中
                        tmp += c
                    End If
                ElseIf commentOpen Then
                    If c = ASCII.CR OrElse c = ASCII.LF Then
                        ' 遇见了换行符，则结束注释
                        tokens += New Token With {
                            .name = RSharpLang.Comment,
                            .Value = New String(tmp)
                        }
                        tmp *= 0
                        commentOpen = False
                    Else
                        ' 任然是注释串之中的一部分字符，则继续添加进入tmp之中
                        tmp += c
                    End If
                Else
                    ' 遇见了字符串的起始的第一个双引号
                    If Not quotOpen AndAlso c = ASCII.Quot Then
                        quotOpen = True
                        createNewToken()
                    ElseIf Not commentOpen AndAlso c = "#"c Then
                        commentOpen = True
                        createNewToken()
                    Else

                        ' 遇见了语句的结束符号
                        Select Case c

                            Case ";"c

                                ' 结束当前的statement的解析
                                createNewToken()
                                last = New Statement(Of Tokens) With {
                                    .tokens = tokens,
                                    .Trace = New LineValue With {
                                        .line = line,
                                        .text = New String(args.statementBuffer)
                                    }
                                }
                                tokens *= 0
                                args.statementBuffer *= 0

                                If parent Is Nothing Then
                                    Return last
                                Else
                                    parent += last
                                End If

                            Case ":"c

                                ' 1. shared method from .NET imports
                                '
                                ' <type_namespace>::method.name()
                                '
                                ' 2. using .NET object method
                                '
                                ' obj:method.name()
                                '
                                ' 3. reference R slot
                                '
                                ' obj$property
                                '

                                ' 这是方法调用的符号
                                createNewToken()

                                If buffer.Current = ":"c Then
                                    c = ++buffer
                                    tokens += New Token(RSharpLang.DotNetSharedAPI, "::")
                                Else
                                    tokens += New Token(RSharpLang.DotNetMethodCall, ":")
                                End If

                            Case "("c

                                ' 新的堆栈
                                ' closure stack open
                                Dim childs As New List(Of Statement(Of Tokens))

                                Call createNewToken()
                                Call buffer.Parse(childs, line, args)

                                ' (1+2) * 3
                                ' don't have any tokens

                                ' 3 * (1+2)
                                ' the last token is an operator

                                ' x <- (1+2)
                                ' x = (1+2)
                                ' the last token is a value assign

                                If tokens = 0 Then
                                    ' 没有tokens元素，则说明可能是
                                    ' (1+2) *3
                                    ' 这种类型的表达式

                                    tokens += New Token(RSharpLang.Priority, "()")
                                Else

                                    With tokens.Last.Type
                                        If .Equals(RSharpLang.LeftAssign) OrElse .Equals(RSharpLang.Operator) OrElse .Equals(RSharpLang.ParameterAssign) Then
                                            tokens += New Token(RSharpLang.Priority, "()")
                                        End If
                                    End With
                                End If

                                With tokens.Last
                                    If .name = RSharpLang.Object Then
                                        ' function
                                        .name = RSharpLang.Function
                                        .Arguments = childs
                                    Else
                                        If .name = RSharpLang.Operator Then
                                            tokens += New Token(RSharpLang.Priority, "()")
                                        ElseIf .name <> RSharpLang.Priority Then
                                            tokens += New Token(RSharpLang.ParenOpen, c)
                                        ElseIf .name = RSharpLang.Priority Then
                                            tokens.Last.Closure = New Main(Of Tokens) With {
                                                .program = childs
                                            }
                                            Continue Do
                                        End If

                                        ' 不要将下面的代码放在Else分支中
                                        tokens.Last.Arguments = childs ' 因为上一行添加了新的token，所以last已经不是原来的了，不可以引用with的last
                                        If .name <> RSharpLang.Operator AndAlso .name <> RSharpLang.Priority Then
                                            tokens += New Token(RSharpLang.ParenClose, close(c))
                                        End If
                                    End If
                                End With

                            Case "["c

                                ' 1. global variable reference
                                '
                                ' # Only allows one identifier
                                ' [x]; # means the variable in the parent environment
                                '  x;  # means the variable in the current environment
                                '
                                ' 2. tuple declare
                                '
                                ' # should contains at least two identifier
                                ' [x,y];
                                ' [[x], y];    # global variable can be reference by using [...]
                                '
                                ' 3. matrix declare
                                ' 
                                ' # all of the elements in the [...] bracket should be vector declare expression
                                ' # identifier not allowed, as the syntax may conflicts with the tuple declare
                                ' # allowes split into multiple line, this will makes the code comment more easier and elgant 
                                ' [
                                '    |x,y,z|,   # code comment goes here
                                '    |a,b,c|,
                                '    |i,j,k|    # index
                                ' ]
                                '

                                ' 新的堆栈
                                ' closure stack open
                                Dim childs As New List(Of Statement(Of Tokens))

                                Call createNewToken()
                                Call buffer.Parse(childs, line, args)

                                If childs.Count = 1 Then
                                    With childs(0)
                                        If .tokens.Length = 1 AndAlso .tokens(Scan0).IsObject Then
                                            ' 这里所解析到的是对全局变量的引用
                                            ' [x] <- 3
                                            ' 3 + [x]
                                            tokens += New Token(RSharpLang.Object, $"[{ .tokens(Scan0).Text}]")
                                            Continue Do
                                        End If
                                    End With
                                End If

                                With New Token(RSharpLang.Tuple, "[...]")
                                    .Arguments = childs
                                    tokens += .ref
                                End With

                            Case "{"c

                                ' {...} closure is a kind of expression which can produce values, 
                                ' not only a statement collection
                                ' 
                                ' return keyword only apply on the function closure
                                ' if the code branch its last statement is a value expression, then the value produced by that 
                                ' Expression will be the value that produce by current {...} closure
                                '
                                ' so that the function closure have two ways to produce the value:
                                ' 1. using return keyword
                                ' 2. using value expression
                                '
                                ' for/if/try...catch can not using return keyword, so that they just using 
                                ' Value Expression for produce the closure value.
                                ' if a return keyword located in the for/if/try...catch closure, that means 
                                ' early breaks of the function closure And returns Value as the value that 
                                ' produced by the function closure.
                                '
                                ' # example of produce value by {...} closure
                                ' var x <- function() {
                                '     return -1;
                                ' }
                                '
                                ' x();  # produce value by function closure
                                '
                                ' # produce value by if closure
                                ' var switchValue <- if (assert()) {
                                '     truevalue;
                                ' } else {
                                '     falseValue;
                                ' }
                                '
                                ' # produce value by try... catch exception handler closure
                                ' var possibleError <- try {
                                '     run_function();
                                ' } catch(e) {
                                '     exception_returns;
                                ' }
                                '
                                ' # produce a numeric vector by for loop closure???
                                ' var f <- for (x in 1:100) {
                                '     x ^ 2;
                                ' }
                                '
                                '

                                ' 新的堆栈
                                ' closure stack open
                                Dim childs As New List(Of Statement(Of Tokens))

                                If bufferEquals("=") Then
                                    Call createNewToken()

                                    With tokens.Last
                                        .name = RSharpLang.ParameterAssign
                                    End With
                                Else
                                    createNewToken() ' 因为newtoken会清空tmp缓存，而bufferEquals函数需要tmp来判断，所以newtoken不能先于bufferequals函数执行
                                    ' 因为上下文变了，所以这里的newtoken调用也不能够合并
                                End If

                                Call buffer.Parse(childs, line, args)

                                Dim matrixOpen = tokens.Count = 0

                                If matrixOpen Then

                                    ' 可能为matrix语法
                                    tokens += New Token(RSharpLang.ParenOpen, "{")

                                End If

                                tokens.Last.Closure = New Main(Of Tokens) With {
                                    .program = childs
                                }

                                If matrixOpen Then
                                    ' 关闭matrix
                                    tokens += New Token(RSharpLang.ParenClose, "}")
                                End If

                                last = New Statement(Of Tokens) With {
                                    .tokens = tokens.ToArray,
                                    .Trace = New LineValue With {
                                        .line = line,
                                        .text = New String(args.statementBuffer)
                                    }
                                }

                                args.statementBuffer *= 0

                                If Not parent Is Nothing Then
                                    parent += last
                                    tokens *= 0 ' }会结束statement，故而需要将tokens清零
                                Else
                                    Return last
                                End If

                            Case ")"c, "]"c

                                ' closure stack close
                                ' 仅结束stack，但是不像{}一样结束statement
                                createNewToken()
                                last = New Statement(Of Tokens) With {
                                    .tokens = tokens,
                                    .Trace = New LineValue With {
                                        .line = line,
                                        .text = New String(args.statementBuffer)
                                    }
                                }
                                args.statementBuffer *= 0
                                tokens *= 0
                                parent += last  ' 右花括号必定是结束堆栈 

                                Return Nothing

                            Case "|"c

                                ' 1. pipeline operator
                                '
                                ' # pipeline operator should be the first character on the line, 
                                ' # and the last character of this line should be the symbol ')', indicate the 
                                ' # current pipeline function invokes.
                                ' object
                                ' |func1()
                                ' |func2()
                                ' ;
                                '
                                ' 2. vector declare
                                ' 
                                ' # should includes more than one identifier, and delimiter using ',' symbol
                                ' |x,y,z|
                                '
                                ' # allows multiple line, probably this syntax would makes the code looks weird, 
                                ' # but this will makes the code comment more easier
                                ' var x as integer <- |
                                '   x,   # value from ....
                                '   y,   # using for ...
                                '   z
                                ' |;
                                '
                                ' 3. abs function
                                '
                                ' # just allow one identifier, x can be numeric, integer vector
                                ' |x|
                                '
                                ' 4. the norm of the vector x
                                '
                                ' # just allow one identifier, x can be numeric, integer, character factor vector
                                ' ||x||

                                Call createNewToken()

                                If tokens = 1 AndAlso Not args.VerticalBarOpen Then
                                    ' 只有一个元素，并且没有打开 | 栈，则可能是管道操作
                                    Dim pipArgs As New ParserArgs With {
                                        .statementBuffer = New List(Of Char),
                                        .PiplineOpen = True
                                    }
                                ElseIf tokens = 0 AndAlso args.VerticalBarOpen = True Then

                                    '  已经打开了 | 栈，并且目前又遇到了 | 栈，则可能是||x||求向量的模
                                    Dim childs As New List(Of Statement(Of Tokens))
                                    Dim internalArgs As ParserArgs = args.OpenVectorNorm

                                    Call buffer.Parse(childs, line, internalArgs)

                                    ' 解析完栈之后，生成token
                                    With New Token(RSharpLang.VectorNorm, "||x||")
                                        .Arguments = childs
                                        tokens += .ref
                                    End With

                                ElseIf tokens = 1 AndAlso args.VectorNormOpen Then

                                    ' 查看后面的一个符号是不是都是 |？因为在循环的开始对c进行赋值的时候已经步进了一个字符，所以直接取当前字符即可
                                    If buffer.Current = "|"c Then

                                        last = New Statement(Of Tokens) With {
                                            .tokens = tokens,
                                            .Trace = New LineValue With {
                                                .line = line,
                                                .text = New String(args.statementBuffer)
                                            }
                                        }
                                        parent += last
                                        c = ++buffer ' 必须要将最后的这个 | 给移除掉，否则后面会再次错误的进入 | 栈的

                                        Return Nothing

                                    Else
                                        ' 语法错误
                                        Throw New SyntaxErrorException
                                    End If

                                ElseIf Not parent.IsNullOrEmpty AndAlso Not args.VectorNormOpen AndAlso args.VerticalBarOpen Then

                                    ' tokens 不是0个或者1个，而是又很多个，则可能是向量的申明的结束标志
                                    last = New Statement(Of Tokens) With {
                                        .tokens = tokens,
                                        .Trace = New LineValue With {
                                            .line = line,
                                            .text = New String(args.statementBuffer)
                                        }
                                    }
                                    parent += last

                                    Return Nothing

                                ElseIf tokens = 1 AndAlso Not args.VectorNormOpen AndAlso args.VerticalBarOpen Then

                                    ' tokens 只有一个元素，则是绝对值的运算的结束符 |x|
                                    last = New Statement(Of Tokens) With {
                                        .tokens = tokens,
                                        .Trace = New LineValue With {
                                            .line = line,
                                            .text = New String(args.statementBuffer)
                                        }
                                    }
                                    args = args.CloseAbs
                                    parent += last

                                    Return Nothing

                                Else
                                    ' 打开 | 栈
                                    Dim childs As New List(Of Statement(Of Tokens))
                                    Dim internalArgs As ParserArgs = args.OpenVerticalBar

                                    Call buffer.Parse(childs, line, internalArgs)

                                    ' 解析完栈之后，生成token
                                    If internalArgs.AbsClose Then

                                        With New Token(RSharpLang.AbsVector, "|x|")
                                            .Arguments = childs
                                            tokens += .ref
                                        End With

                                    Else

                                        With New Token(RSharpLang.VectorDeclare, "|...|")
                                            .Arguments = childs
                                            tokens += .ref
                                        End With

                                    End If
                                End If

                            Case "&"c
                                ' 字符串拼接
                                createNewToken()
                                tokens += New Token(RSharpLang.StringContact, "&")

                            Case ","c
                                createNewToken()
                                last = New Statement(Of Tokens) With {
                                    .tokens = tokens,
                                    .Trace = New LineValue With {
                                        .line = line,
                                        .text = New String(args.statementBuffer)
                                    }
                                }
                                args.statementBuffer *= 0
                                tokens *= 0
                                parent += last  ' 逗号分隔只产生新的statement，但是不退栈

                            Case "="c

                                If bufferEquals("<"c) Then
                                    tokens += New Token(RSharpLang.Operator, "<=")
                                    tmp *= 0
                                ElseIf bufferEquals("="c) Then
                                    tokens += New Token(RSharpLang.Operator, "==")
                                    tmp *= 0
                                Else
                                    If tmp.Count = 0 Then
                                        ' 可能是==的第一个等号，则只添加
                                        tmp += c
                                    Else
                                        createNewToken()
                                        tokens += New Token(RSharpLang.ParameterAssign, "=")
                                    End If
                                End If

                            Case "}"c

                                ' closure stack close
                                ' 结束当前的statement，相当于分号
                                createNewToken()
                                last = New Statement(Of Tokens) With {
                                    .tokens = tokens,
                                    .Trace = New LineValue With {
                                        .line = line,
                                        .text = New String(args.statementBuffer)
                                    }
                                }
                                args.statementBuffer *= 0
                                tokens *= 0
                                parent += last  ' 右花括号必定是结束堆栈 

                                Return Nothing

                            Case " "c, ASCII.TAB, ASCII.LF, ASCII.CR

                                If c = ASCII.LF Then   ' 只使用\n来判断行号
                                    line += 1
                                End If

                                ' 遇见了空格，结束当前的token
                                If bufferEquals("=") Then
                                    Call createNewToken()

                                    With tokens.Last
                                        .name = RSharpLang.ParameterAssign
                                    End With
                                Else
                                    createNewToken() ' 因为newtoken会清空tmp缓存，而bufferEquals函数需要tmp来判断，所以newtoken不能先于bufferequals函数执行
                                    ' 因为上下文变了，所以这里的newtoken调用也不能够合并
                                End If

                            Case "-"c

                                If bufferEquals("<"c) Then
                                    tmp += "-"c
                                    createNewToken()
                                Else
                                    createNewToken() ' 这两个newtoken调用不可以合并到一起，因为他们的上下文环境变了 
                                    tokens += New Token(RSharpLang.Operator, c)
                                End If

                            Case "+"c, "*"c, "/"c, "\"c, "^", "@"c
                                createNewToken()
                                tokens += New Token(RSharpLang.Operator, c)

                            Case Else

                                ' identifier, number values
                                tmp += c
                        End Select
                    End If
                End If
            Loop

            Return New Statement(Of Tokens) With {
                .tokens = tokens
            }
        End Function

        <Extension> Public Function GetSourceTree(s As IEnumerable(Of Statement(Of Tokens))) As String
            Return New Main(Of Tokens) With {
                .program = s.ToArray.Trim
            }.GetXml()
        End Function

        ''' <summary>
        ''' Removes the blank statements
        ''' </summary>
        ''' <param name="src"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Trim(src As Statement(Of Tokens)()) As Statement(Of Tokens)()
            Return src _
                .Select(Function(s) s.Trim) _
                .Where(Function(s) Not s.tokens.IsNullOrEmpty) _
                .ToArray
        End Function

        <Extension>
        Public Function Trim(src As Statement(Of Tokens)) As Statement(Of Tokens)
            With src
                .tokens = src.tokens _
                    .Select(Function(t) t.Trim) _
                    .Where(Function(t) Not t.IsNullOrEmpty) _
                    .ToArray
                Return .ref
            End With
        End Function

        <Extension>
        Public Function Trim(t As Token(Of Tokens)) As Token(Of Tokens)
            If Not t.Arguments.IsNullOrEmpty Then
                t.Arguments = t.Arguments.Trim
            End If
            If Not t.Closure Is Nothing AndAlso Not t.Closure.program.IsNullOrEmpty Then
                t.Closure.program = t.Closure.program.Trim
            End If

            Return t
        End Function

        <Extension>
        Public Function IsNullOrEmpty(t As Token(Of Tokens)) As Boolean
            If Not t.Value.StringEmpty Then
                Return False
            End If
            If Not t.UNDEFINED Then
                Return False
            End If
            If Not t.Arguments.IsNullOrEmpty Then
                Return False
            End If
            If Not t.Closure Is Nothing AndAlso Not t.Closure.program.IsNullOrEmpty Then
                Return False
            End If

            Return True
        End Function

        ReadOnly close As New Dictionary(Of Char, Char) From {
            {"("c, ")"c},
            {"["c, "]"c},
            {"{"c, "}"c}
        }

    End Module
End Namespace
