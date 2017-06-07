Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports langToken = Microsoft.VisualBasic.Scripting.TokenIcer.Token(Of R.LanguageTokens)

Public Module TokenIcer

    <Extension> Public Iterator Function Parse(s$) As IEnumerable(Of Statement)
        Dim buffer As New Pointer(Of Char)(Trim(s$))
        Dim it As New Value(Of Statement)

        Do While Not buffer.EndRead
            If Not (it = buffer.Parse(Nothing, False)) Is Nothing Then
                Yield it
            End If
        Loop
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <param name="parent"></param>
    ''' <param name="indexStack"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 对于``{}``而言，其含义为当前的token的closure
    ''' 对于``()``而言，其含义为当前的token的innerStack
    ''' 对于``[]``而言，起含义为当前的token的innerStack，与``()``的含义几乎一致
    ''' </remarks>
    <Extension> Private Function Parse(buffer As Pointer(Of Char), ByRef parent As List(Of Statement), indexStack As Boolean) As Statement
        Dim quotOpen As Boolean = False
        Dim commentOpen As Boolean = False ' 当出现注释符的时候，会一直持续到遇见换行符为止
        Dim tmp As New List(Of Char)
        Dim tokens As New List(Of langToken)
        Dim last As Statement
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
        Dim newToken =
            Sub()
                If tmp.Count = 0 Then
                    Return
                End If

                ' 创建除了字符串之外的其他的token
                If varDefInit() Then
                    tokens += New langToken(LanguageTokens.var, "var")
                ElseIf tmp.SequenceEqual("<-") Then
                    tokens += New langToken(LanguageTokens.LeftAssign, "<-")
                Else
                    tokens += New langToken(LanguageTokens.Object) With {
                        .Value = New String(tmp)
                    }
                End If

                tmp *= 0
            End Sub
        Dim closure As Statement() = Nothing
        Dim arguments As Statement() = Nothing

        Do While Not buffer.EndRead
            Dim c As Char = +buffer

            If quotOpen Then ' 当前所解析的状态为字符串解析
                If c = ASCII.Quot AndAlso Not tmp.StartEscaping Then
                    ' 当前的字符为双引号，并且不是转义状态，则结束字符串
                    tokens += New langToken With {
                        .Name = LanguageTokens.String,
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
                    tokens += New langToken With {
                        .Name = LanguageTokens.Comment,
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
                    newToken()
                ElseIf Not commentOpen AndAlso c = "#"c Then
                    commentOpen = True
                    newToken()
                Else
                    ' 遇见了语句的结束符号
                    If c = ";"c Then
                        ' 结束当前的statement的解析
                        newToken()
                        last = New Statement With {
                            .Tokens = tokens,
                            .arguments = arguments,
                            .closure = closure
                        }
                        tokens *= 0

                        If parent Is Nothing Then
                            Return last
                        Else
                            parent += last
                        End If
                    ElseIf c = ":"c Then
                        ' 这是方法调用的符号
                        newToken()
                        tokens += New langToken(LanguageTokens.methodCall, ":")
                    ElseIf c = "("c OrElse c = "["c OrElse c = "{"c Then
                        ' 新的堆栈
                        ' closure stack open
                        Dim childs As New List(Of Statement)
                        Call buffer.Parse(childs, False)
                        Call newToken()

                        tokens += New langToken(LanguageTokens.ParenOpen, c)
                        tokens += New langToken(LanguageTokens.ParenClose, close(c))

                        If c = "{"c Then
                            last = New Statement With {
                                .Tokens = tokens.ToArray,
                                .closure = childs,
                                .arguments = arguments
                            }
                            If Not parent Is Nothing Then
                                parent += last
                            Else
                                Return last
                            End If
                        Else
                            arguments = childs
                        End If

                        ' tokens *= 0
                    ElseIf c = ")"c OrElse c = "]"c Then
                        ' closure stack close
                        ' 仅结束stack，但是不像{}一样结束statement
                        newToken()
                        last = New Statement With {
                            .Tokens = tokens,
                            .closure = closure,
                            .arguments = arguments
                        }
                        tokens *= 0
                        parent += last  ' 右花括号必定是结束堆栈 
                        Return Nothing
                        'ElseIf c = "["c Then
                        '    ' closure stack open
                        '    Dim childs As New List(Of Statement)
                        '    Call buffer.Parse(childs, False)
                        '    Call newToken 

                        '    last = New Statement With {
                        '        .Tokens = tokens.ToArray,
                        '        .arguments = childs
                        '    }
                        '    tokens *= 0
                        '    If Not parent Is Nothing Then
                        '        parent += last
                        '    Else
                        '        Return last
                        '    End If
                        'ElseIf c = "]"c Then
                        '    newToken()
                        '    tokens += New langToken(LanguageTokens.IndexClose, "]"c)
                    ElseIf c = "&"c Then
                        ' 字符串拼接
                        newToken()
                        tokens += New langToken(LanguageTokens.StringContact, "&")
                    ElseIf c = ","c Then
                        newToken()
                        last = New Statement With {
                            .Tokens = tokens,
                            .arguments = arguments,
                            .closure = closure
                        }
                        tokens *= 0
                        parent += last  ' 逗号分隔只产生新的statement，但是不退栈
                    ElseIf c = "="c Then
                        If bufferEquals("<"c) Then
                            tokens += New langToken(LanguageTokens.Operator, "<=")
                            tmp *= 0
                        ElseIf bufferEquals("="c) Then
                            tokens += New langToken(LanguageTokens.Operator, "==")
                            tmp *= 0
                        Else
                            If tmp.Count = 0 Then
                                ' 可能是==的第一个等号，则只添加
                                tmp += c
                            Else
                                newToken()
                            End If
                        End If
                        'ElseIf c = "{"c Then
                        '    ' closure stack open
                        '    Dim childs As New List(Of Statement)
                        '    Call buffer.Parse(childs, False)
                        '    newToken()
                        '    tokens += New langToken(LanguageTokens.ParenOpen, "{")
                        '    last = New Statement With {
                        '        .Tokens = tokens.ToArray,
                        '        .closure = childs
                        '    }
                        '    tokens *= 0
                        '    If Not parent Is Nothing Then
                        '        parent += last
                        '    Else
                        '        Return last
                        '    End If
                    ElseIf c = "}"c Then
                        ' closure stack close
                        ' 结束当前的statement，相当于分号
                        newToken()
                        last = New Statement With {
                            .Tokens = tokens,
                            .closure = closure,
                            .arguments = arguments
                        }
                        tokens *= 0
                        parent += last  ' 右花括号必定是结束堆栈 
                        Return Nothing
                    ElseIf c = " "c OrElse c = ASCII.TAB OrElse c = ASCII.LF OrElse c = ASCII.CR Then
                        ' 遇见了空格，结束当前的token
                        newToken()
                    Else
                        tmp += c
                    End If
                End If
            End If
        Loop

        Return New Statement With {.Tokens = tokens}
    End Function

    <Extension> Public Function GetSourceTree(s As IEnumerable(Of Statement)) As String
        Return New Main With {.program = s.ToArray}.GetXml
    End Function

    ReadOnly close As New Dictionary(Of Char, Char) From {
        {"("c, ")"c},
        {"["c, "]"c},
        {"{"c, "}"c}
    }

End Module
