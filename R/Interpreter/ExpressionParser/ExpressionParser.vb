Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports ExpressionTokens = SMRUCC.Rsharp.LanguageTokens

Module ExpressionParser

    ''' <summary>
    ''' 这个解析器还需要考虑Stack的问题
    ''' </summary>
    ''' <param name="tokens"></param>
    ''' <returns></returns>
    <Extension> Public Function TryParse(tokens As Pointer(Of Token(Of ExpressionTokens)), getValue As GetValue, evaluate As FunctionEvaluate, ByRef funcStack As Boolean) As SimpleExpression
        Dim sep As New SimpleExpression
        Dim e As Token(Of ExpressionTokens)
        Dim o As Char
        Dim pre As Token(Of ExpressionTokens) = Nothing
        Dim func As FuncCaller = Nothing

        Do While Not tokens.EndRead
            Dim meta As MetaExpression = Nothing

            e = +tokens

            Select Case e.Type
                Case ExpressionTokens.OpenBracket, ExpressionTokens.OpenStack
                    If pre Is Nothing Then  ' 前面不是一个未定义的标识符，则在这里是一个括号表达式
                        meta = New MetaExpression(TryParse(tokens, getValue, evaluate, False))
                    Else
                        Dim fstack As Boolean = True

                        func = New FuncCaller(pre.Text, evaluate)  ' Get function name, and then removes the last of the expression
                        o = sep.RemoveLast().Operator

                        Do While fstack   ' 在这里进行函数的参数列表的解析
                            Dim exp = TryParse(tokens, getValue, evaluate, fstack)
                            If exp.IsNullOrEmpty Then
                                Exit Do
                            Else
                                func.Params.Add(exp)
                            End If
                        Loop

                        meta = New MetaExpression(AddressOf func.Evaluate)
                        ' o = If(tokens.EndRead, "+"c, (+tokens).Text.First)
                        ' meta.Operator = o
                        pre = Nothing
                        ' Call sep.Add(meta)

                        ' Continue Do
                    End If
                Case ExpressionTokens.CloseStack, ExpressionTokens.CloseBracket, ExpressionTokens.Delimiter
                    Return sep ' 退出递归栈
                Case ExpressionTokens.Number
                    meta = New MetaExpression(Val(e.Text))
                Case ExpressionTokens.undefine

                    Dim x As String = e.Text
                    meta = New MetaExpression(Function() getValue(x))

                    If tokens.EndRead Then
                        pre = Nothing
                    Else
                        If tokens.Current.name = ExpressionTokens.Operator Then
                            pre = Nothing
                        Else
                            pre = e ' probably is a function name
                        End If
                    End If

                Case ExpressionTokens.Operator
                    If String.Equals(e.Text, "-") Then

                        If Not sep.IsNullOrEmpty Then
                            If tokens.Current.Type = ExpressionTokens.Number Then
                                meta = New MetaExpression(-1 * Val((+tokens).Text))
                            Else
                                Throw New SyntaxErrorException
                            End If
                        Else
                            ' 是一个负数
                            meta = New MetaExpression(0)
                            meta.Operator = "-"c
                            Call sep.Add(meta)
                            Continue Do
                        End If
                    End If
            End Select

            If tokens.EndRead Then
                meta.Operator = "+"c
                Call sep.Add(meta)
            Else
                o = (+tokens).Text.First  ' tokens++ 移动指针到下一个元素

                If o = "!"c Then
                    Dim stackMeta = New MetaExpression(Function() Factorial(meta.LEFT, 0))

                    If tokens.EndRead Then
                        Call sep.Add(stackMeta)
                        Exit Do
                    Else
                        o = (+tokens).Text.First
                        If o = ")"c Then
                            ' 2017-1-26
                            ' 在这里是因为需要结束括号，进行退栈，所以指针会往回移动
                            ' 假若在这里是函数调用的结束符号右括号的话，假若这里是表达式的最后一个位置，则可能会出错
                            ' 现在这个错误已经被修复
                            'If Not tokens.EndRead Then
                            '    e = (-tokens)
                            'End If
                            stackMeta.Operator = "+"c
                            funcStack = False  ' 已经是括号的结束了，则退出栈
                            Call sep.Add(stackMeta)
                            'If Not tokens.EndRead Then
                            '    e = (-tokens)
                            'End If
                            Return sep
                        ElseIf o = ","c Then
                            meta.Operator = "+"c
                            Call sep.Add(meta)
                            ' e = (-tokens)
                            Exit Do ' 退出递归栈
                        Else
                            stackMeta.Operator = o
                            Call sep.Add(stackMeta)
                            Continue Do
                        End If
                    End If
                ElseIf o = ","c Then
                    meta.Operator = "+"c
                    Call sep.Add(meta)
                    ' e = (-tokens)
                    funcStack = True

                    Exit Do ' 退出递归栈
                ElseIf IsCloseStack(o) Then
                    meta.Operator = "+"c
                    Call sep.Add(meta)
                    funcStack = False
                    'If funcStack AndAlso Not tokens.EndRead Then
                    '    e = (-tokens)
                    'End If
                    Exit Do ' 退出递归栈
                ElseIf IsOpenStack(o) Then
                    e = -tokens  ' 指针回退一步
                End If

                meta.Operator = o
                Call sep.Add(meta)
            End If
        Loop

        Return sep
    End Function
End Module
