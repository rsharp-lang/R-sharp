Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports ExpressionTokens = SMRUCC.Rsharp.LanguageTokens

''' <summary>
''' 除了函数申明，之类的语句表达式，这个模块是专门用来解析类似于数学运算的表达式的
''' 也就是说这个模块之中解析出来的结果是具体的表达式的结构内容
''' </summary>
Module ExpressionParser

    <Extension>
    Public Function TryParse(tokens As Pointer(Of Token(Of ExpressionTokens))) As Func(Of Environment, SimpleExpression)
        Return Function(envir)
                   With envir
                       Return tokens.TryParse(.GetValue, .Evaluate, False)
                   End With
               End Function
    End Function

    <Extension>
    Public Function TryParseInternal(tokens As Pointer(Of Token(Of ExpressionTokens))) As Func(Of GetValue, FunctionEvaluate, SimpleExpression)
        Return Function(GetValue, FunctionEvaluate)
                   Return tokens.TryParse(GetValue, FunctionEvaluate, False)
               End Function
    End Function

    <Extension>
    Public Function TryParseValue(statement As Statement(Of ExpressionTokens), getValue As GetValue, evaluate As FunctionEvaluate) As ValueExpression
        Return New Pointer(Of Token(Of ExpressionTokens))(statement.tokens).TryParse(getValue, evaluate, False)
    End Function

    ''' <summary>
    ''' 这个解析器还需要考虑Stack的问题
    ''' </summary>
    ''' <param name="tokens"></param>
    ''' <returns></returns>
    <Extension> Public Function TryParse(tokens As Pointer(Of Token(Of ExpressionTokens)), getValue As GetValue, evaluate As FunctionEvaluate, ByRef funcStack As Boolean) As SimpleExpression
        Dim expression As New SimpleExpression
        Dim t As Token(Of ExpressionTokens)
        Dim o$
        Dim pre As Token(Of ExpressionTokens) = Nothing
        Dim func As FuncCaller = Nothing

        Do While Not tokens.EndRead
            Dim meta As MetaExpression = Nothing

            t = +tokens

            Select Case t.Type
                Case ExpressionTokens.Priority ' (1+2) *3
                    Dim closure = t.Closure.program.First
                    Dim value As SimpleExpression = New Pointer(Of Token(Of ExpressionTokens))(closure).TryParse(getValue, evaluate, False)
                    meta = New MetaExpression(simple:=value)
                Case ExpressionTokens.Function
                    Dim name$ = t.Text
                    Dim args = t.Arguments.Select()
                    Dim handle = Function()
                                     Dim params = args.Select(Function(x) x).ToArray
                                     Dim value As Object = evaluate(name,)
                                 End Function

                    meta = New MetaExpression(handle)
            End Select

            meta.Operator = (+tokens).Text
            expression.Add(meta)
        Loop

        Do While Not tokens.EndRead
            Dim meta As MetaExpression = Nothing

            t = +tokens

            Select Case t.Type
                Case ExpressionTokens.ParenOpen
                    If pre Is Nothing Then  ' 前面不是一个未定义的标识符，则在这里是一个括号表达式
                        meta = New MetaExpression(TryParse(tokens, getValue, evaluate, False))
                    Else
                        Dim fstack As Boolean = True

                        func = New FuncCaller(pre.Text, evaluate)  ' Get function name, and then removes the last of the expression
                        o = expression.RemoveLast().Operator

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
                Case ExpressionTokens.ParenClose, ExpressionTokens.ParameterDelimiter
                    Return expression ' 退出递归栈
                Case ExpressionTokens.Object
                    meta = New MetaExpression(Val(t.Text))
                Case ExpressionTokens.undefine

                    Dim x As String = t.Text
                    meta = New MetaExpression(Function() getValue(x))

                    If tokens.EndRead Then
                        pre = Nothing
                    Else
                        If tokens.Current.name = ExpressionTokens.Operator Then
                            pre = Nothing
                        Else
                            pre = t ' probably is a function name
                        End If
                    End If

                Case ExpressionTokens.Operator
                    If String.Equals(t.Text, "-") Then

                        If Not expression.IsNullOrEmpty Then
                            If tokens.Current.Type = ExpressionTokens.Object Then
                                meta = New MetaExpression(-1 * Val((+tokens).Text))
                            Else
                                Throw New SyntaxErrorException
                            End If
                        Else
                            ' 是一个负数
                            meta = New MetaExpression(0)
                            meta.Operator = "-"
                            Call expression.Add(meta)
                            Continue Do
                        End If
                    End If
            End Select

            If tokens.EndRead Then
                meta.Operator = "+"
                Call expression.Add(meta)
            Else
                o = (+tokens).Text  ' tokens++ 移动指针到下一个元素

                If o = "!" Then
                    Dim stackMeta = New MetaExpression ' (handle:=Function() Factorial(meta.LEFT, 0))

                    If tokens.EndRead Then
                        Call expression.Add(stackMeta)
                        Exit Do
                    Else
                        o = (+tokens).Text.First
                        If o = ")" Then
                            ' 2017-1-26
                            ' 在这里是因为需要结束括号，进行退栈，所以指针会往回移动
                            ' 假若在这里是函数调用的结束符号右括号的话，假若这里是表达式的最后一个位置，则可能会出错
                            ' 现在这个错误已经被修复
                            'If Not tokens.EndRead Then
                            '    e = (-tokens)
                            'End If
                            stackMeta.Operator = "+"
                            funcStack = False  ' 已经是括号的结束了，则退出栈
                            Call expression.Add(stackMeta)
                            'If Not tokens.EndRead Then
                            '    e = (-tokens)
                            'End If
                            Return expression
                        ElseIf o = "," Then
                            meta.Operator = "+"
                            Call expression.Add(meta)
                            ' e = (-tokens)
                            Exit Do ' 退出递归栈
                        Else
                            stackMeta.Operator = o
                            Call expression.Add(stackMeta)
                            Continue Do
                        End If
                    End If
                ElseIf o = "," Then
                    meta.Operator = "+"
                    Call expression.Add(meta)
                    ' e = (-tokens)
                    funcStack = True

                    Exit Do ' 退出递归栈
                    'ElseIf isCloseStack(o) Then
                    '    meta.Operator = "+"
                    '    Call sep.Add(meta)
                    '    funcStack = False
                    '    'If funcStack AndAlso Not tokens.EndRead Then
                    '    '    e = (-tokens)
                    '    'End If
                    '    Exit Do ' 退出递归栈
                    'ElseIf IsOpenStack(o) Then
                    '    e = -tokens  ' 指针回退一步
                End If

                meta.Operator = o
                Call expression.Add(meta)
            End If
        Loop

        Return expression
    End Function

End Module
