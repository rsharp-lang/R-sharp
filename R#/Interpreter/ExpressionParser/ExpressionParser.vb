Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports ExpressionToken = SMRUCC.Rsharp.Interpreter.LanguageTokens
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.Expression

    ''' <summary>
    ''' 除了函数申明，之类的语句表达式，这个模块是专门用来解析类似于数学运算的表达式的
    ''' 也就是说这个模块之中解析出来的结果是具体的表达式的结构内容
    ''' </summary>
    Module ExpressionParser

        <Extension>
        Public Function TryParse(tokens As IEnumerable(Of Token(Of ExpressionToken))) As Func(Of Environment, SimpleExpression)
            Return New Pointer(Of Token(Of ExpressionToken))(tokens).TryParse
        End Function

        <Extension>
        Public Function TryParse(tokens As Pointer(Of Token(Of ExpressionToken))) As Func(Of Environment, SimpleExpression)
            Return Function(envir)
                       With envir
                           Return tokens.TryParse(envir, False)
                       End With
                   End Function
        End Function

        <Extension> Private Function FunctionArgument(parm As Statement(Of ExpressionToken), i%) As NamedValue(Of Func(Of Environment, SimpleExpression))
            Dim t = parm.tokens
            Dim name$

            If t(1).Text = "=" AndAlso t(1).Type = ExpressionToken.ParameterAssign Then
                name = t(0).Value
            Else
                name = "#" & i
            End If

            Return New NamedValue(Of Func(Of Environment, SimpleExpression))(name, t.Skip(2).TryParse)
        End Function

        ''' <summary>
        ''' 这个解析器还需要考虑Stack的问题
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension> Public Function TryParse(tokens As Pointer(Of Token(Of ExpressionToken)), environment As Environment, ByRef funcStack As Boolean) As SimpleExpression
            Dim expression As New SimpleExpression
            Dim t As Token(Of ExpressionToken)
            Dim pre As Token(Of ExpressionToken) = Nothing
            Dim func As FuncCaller = Nothing
            Dim getValue As GetValue = AddressOf environment.GetValue
            Dim Evaluate As FunctionEvaluate = AddressOf environment.Evaluate

            Do While Not tokens.EndRead
                Dim meta As MetaExpression = Nothing

                t = +tokens

                Select Case t.Type
                    Case ExpressionToken.Priority ' (1+2) *3
                        Dim closure = t.Closure.program.First
                        Dim value As SimpleExpression = New Pointer(Of Token(Of ExpressionToken))(closure.tokens).TryParse(environment, False)
                        meta = New MetaExpression(simple:=value, envir:=Function() environment)
                    Case ExpressionToken.Function

                        ' 进行函数调用求值
                        Dim name$ = t.Text
                        ' 需要对参数进行表达式的编译，方便进行求值计算
                        Dim args As NamedValue(Of Func(Of Environment, SimpleExpression))() = t _
                            .Arguments _
                            .SeqIterator _
                            .Select(Function(parm) parm.value.FunctionArgument(parm)) _
                            .ToArray
                        Dim calls As New FuncCaller(name, params:=args)
                        Dim handle = Function() calls.Evaluate(environment)

                        meta = New MetaExpression(handle)
                    Case ExpressionToken.Object
                        meta = New MetaExpression(t.Value, t.Type)
                End Select

                If Not tokens.EndRead Then
                    meta.Operator = (+tokens).Text
                Else
                    meta.Operator = "+"
                End If

                Call expression.Add(meta)
            Loop

            'Do While Not tokens.EndRead
            '    Dim meta As MetaExpression = Nothing

            '    t = +tokens

            '    Select Case t.Type
            '        Case ExpressionTokens.ParenOpen
            '            If pre Is Nothing Then  ' 前面不是一个未定义的标识符，则在这里是一个括号表达式
            '                meta = New MetaExpression(TryParse(tokens, environment, False))
            '            Else
            '                Dim fstack As Boolean = True

            '                func = New FuncCaller(pre.Text, Evaluate)  ' Get function name, and then removes the last of the expression
            '                o = expression.RemoveLast().Operator

            '                Do While fstack   ' 在这里进行函数的参数列表的解析
            '                    Dim exp = TryParse(tokens, environment, fstack)
            '                    If exp.IsNullOrEmpty Then
            '                        Exit Do
            '                    Else
            '                        func.Params.Add(exp)
            '                    End If
            '                Loop

            '                meta = New MetaExpression(AddressOf func.Evaluate)
            '                ' o = If(tokens.EndRead, "+"c, (+tokens).Text.First)
            '                ' meta.Operator = o
            '                pre = Nothing
            '                ' Call sep.Add(meta)

            '                ' Continue Do
            '            End If
            '        Case ExpressionTokens.ParenClose, ExpressionTokens.ParameterDelimiter
            '            Return expression ' 退出递归栈
            '        Case ExpressionTokens.Object
            '            meta = New MetaExpression(Val(t.Text))
            '        Case ExpressionTokens.undefine

            '            Dim x As String = t.Text
            '            meta = New MetaExpression(Function() GetValue(x))

            '            If tokens.EndRead Then
            '                pre = Nothing
            '            Else
            '                If tokens.Current.name = ExpressionTokens.Operator Then
            '                    pre = Nothing
            '                Else
            '                    pre = t ' probably is a function name
            '                End If
            '            End If

            '        Case ExpressionTokens.Operator
            '            If String.Equals(t.Text, "-") Then

            '                If Not expression.IsNullOrEmpty Then
            '                    If tokens.Current.Type = ExpressionTokens.Object Then
            '                        meta = New MetaExpression(-1 * Val((+tokens).Text))
            '                    Else
            '                        Throw New SyntaxErrorException
            '                    End If
            '                Else
            '                    ' 是一个负数
            '                    meta = New MetaExpression(0)
            '                    meta.Operator = "-"
            '                    Call expression.Add(meta)
            '                    Continue Do
            '                End If
            '            End If
            '    End Select

            '    If tokens.EndRead Then
            '        meta.Operator = "+"
            '        Call expression.Add(meta)
            '    Else
            '        o = (+tokens).Text  ' tokens++ 移动指针到下一个元素

            '        If o = "!" Then
            '            Dim stackMeta = New MetaExpression ' (handle:=Function() Factorial(meta.LEFT, 0))

            '            If tokens.EndRead Then
            '                Call expression.Add(stackMeta)
            '                Exit Do
            '            Else
            '                o = (+tokens).Text.First
            '                If o = ")" Then
            '                    ' 2017-1-26
            '                    ' 在这里是因为需要结束括号，进行退栈，所以指针会往回移动
            '                    ' 假若在这里是函数调用的结束符号右括号的话，假若这里是表达式的最后一个位置，则可能会出错
            '                    ' 现在这个错误已经被修复
            '                    'If Not tokens.EndRead Then
            '                    '    e = (-tokens)
            '                    'End If
            '                    stackMeta.Operator = "+"
            '                    funcStack = False  ' 已经是括号的结束了，则退出栈
            '                    Call expression.Add(stackMeta)
            '                    'If Not tokens.EndRead Then
            '                    '    e = (-tokens)
            '                    'End If
            '                    Return expression
            '                ElseIf o = "," Then
            '                    meta.Operator = "+"
            '                    Call expression.Add(meta)
            '                    ' e = (-tokens)
            '                    Exit Do ' 退出递归栈
            '                Else
            '                    stackMeta.Operator = o
            '                    Call expression.Add(stackMeta)
            '                    Continue Do
            '                End If
            '            End If
            '        ElseIf o = "," Then
            '            meta.Operator = "+"
            '            Call expression.Add(meta)
            '            ' e = (-tokens)
            '            funcStack = True

            '            Exit Do ' 退出递归栈
            '            'ElseIf isCloseStack(o) Then
            '            '    meta.Operator = "+"
            '            '    Call sep.Add(meta)
            '            '    funcStack = False
            '            '    'If funcStack AndAlso Not tokens.EndRead Then
            '            '    '    e = (-tokens)
            '            '    'End If
            '            '    Exit Do ' 退出递归栈
            '            'ElseIf IsOpenStack(o) Then
            '            '    e = -tokens  ' 指针回退一步
            '        End If

            '        meta.Operator = o
            '        Call expression.Add(meta)
            '    End If
            'Loop

            Return expression
        End Function
    End Module
End Namespace