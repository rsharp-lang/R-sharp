#Region "Microsoft.VisualBasic::62b37b0c8a140276c042b478213a66ab, R#\Interpreter\ExpressionParser\ExpressionParser.vb"

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

    '     Module ExpressionParser
    ' 
    '         Function: (+2 Overloads) TryParse, Vector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.CodeDOM
Imports ExpressionToken = SMRUCC.Rsharp.Interpreter.Language.Tokens

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
        ''' <param name="tokenBuffer"></param>
        ''' <returns></returns>
        <Extension> Public Function TryParse(tokenBuffer As Pointer(Of Token(Of ExpressionToken)),
                                             environment As Environment,
                                             ByRef funcStack As Boolean) As SimpleExpression

            Dim expression As New SimpleExpression
            Dim t As Token(Of ExpressionToken)
            Dim pre As Token(Of ExpressionToken) = Nothing
            Dim func As FuncCaller = Nothing
            Dim getValue As GetValue = AddressOf environment.GetValue
            Dim Evaluate As FunctionEvaluate = AddressOf environment.Evaluate

            Do While Not tokenBuffer.EndRead
                Dim meta As MetaExpression = Nothing

                t = ++tokenBuffer

                Select Case t.Type

                    Case ExpressionToken.Priority ' (1+2) *3

                        Dim closure As Statement(Of ExpressionToken)

                        If t.IsClosure Then
                            closure = t.Closure.program.First
                        Else
                            closure = t.Arguments.First
                        End If

                        Dim value As SimpleExpression = New Pointer(Of Token(Of ExpressionToken))(closure.tokens).TryParse(environment, False)
                        meta = New MetaExpression(simple:=value, envir:=Function() environment)

                    Case ExpressionToken.Function

                        ' 进行函数调用求值
                        Dim name$ = t.Text

                        ' 假若目标t的closure不为空，并且name为function的话，说明为函数申明
                        ' 反之为函数调用

                        If name = "function" AndAlso Not t.Closure.program.IsNullOrEmpty Then

                            ' 函数申明
                            ' function(x,y, ...) {...}
                            Dim parameters = t.Arguments
                            Dim body = t.Closure


                        Else

                            ' 函数调用
                            ' func(x=1, y=2, ...)

                            ' 需要对参数进行表达式的编译，方便进行求值计算
                            Dim args As NamedValue(Of Func(Of Environment, SimpleExpression))() = t _
                                .Arguments _
                                .SeqIterator _
                                .Select(Function(parm) parm.value.FunctionArgument(parm)) _
                                .ToArray
                            Dim calls As New FuncCaller(name, params:=args)
                            Dim handle = Function() calls.Evaluate(environment)

                            meta = New MetaExpression(handle)

                        End If

                    Case ExpressionToken.Object

                        If t.Text.TextEquals("not") Then
                            ' 如果下一个对象不是操作符，这说明这个元素可能是not操作符
                            If tokenBuffer.Current.Type <> ExpressionToken.Operator Then
                                ' 因为metaExpression是应用于双目运算符的
                                ' 所以在这里单目运算符需要转换为lambda表达式
                                Dim value = ++tokenBuffer
                                Dim expr = Function()
                                               Return environment.EvaluateUnary(New MetaExpression(value.Value, value.Type), t.Text)
                                           End Function

                                meta = New MetaExpression(expr)

                            Else

                                ' 这个not是一个变量
                                meta = New MetaExpression(t.Value, t.Type)

                            End If
                        Else
                            meta = New MetaExpression(t.Value, t.Type)
                        End If

                    Case ExpressionToken.VectorDeclare

                        ' due to the reason of vector norm result is a single value at the same time it is also a vector with only one element
                        ' so that the vector norm will be parsed as a vector declare expression at first.
                        ' and then it will be parse at Case ExpressionToken.VectorNorm branch after evaluation was called.
                        Dim vector = t.Arguments.Select(Function(x) New ValueExpression(x.tokens)).ToArray
                        Dim value = Function()
                                        Dim v = vector _
                                            .Select(Function(x) x.Evaluate(environment)) _
                                            .ToArray

                                        Return v.Vector
                                    End Function

                        meta = New MetaExpression(value)

                    Case ExpressionToken.AbsVector

                        ' 求绝对值只允许标识符的引用，所以在这里只有一个变量对象
                        Dim vector = t.Arguments.First.tokens.First
                        Dim ref As New VariableReference(vector.Text)

                        meta = New MetaExpression(Function()
                                                      Dim value = ref.Evaluate(environment)
                                                      Dim numerics = DirectCast(value.value, Variable).ToVector

                                                      Return numerics _
                                                          .Select(Function(x) Math.Abs(Val(x))) _
                                                          .ToArray
                                                  End Function)

                    Case ExpressionToken.VectorNorm

                        ' 与求绝对值一样，计算norm也是必须要使用标识符来引用的
                        ' 所以在这里也是只有一个对象
                        Dim vector = t.Arguments.First.tokens.First
                        Dim ref As New VariableReference(vector.Text)

                        meta = New MetaExpression(Function()
                                                      Dim value = ref.Evaluate(environment)
                                                      Dim numerics = DirectCast(value.value, Variable).ToVector
                                                      Dim mod# = Aggregate x As Object
                                                                 In numerics
                                                                 Into Sum(Val(x) ^ 2)
                                                      Dim norm# = Math.Sqrt([mod])

                                                      ' 经过计算之后都会被便作为double数值类型
                                                      Return TempValue.Tuple(norm, TypeCodes.double)
                                                  End Function)

                    Case ExpressionToken.Boolean

                        meta = New MetaExpression(t.Text.ParseBoolean, TypeCodes.boolean)

                    Case ExpressionToken.Operator

                        ' 负数
                        If t.Text = "-" Then

                            With ++tokenBuffer
                                Dim exp As SimpleExpression = New MetaExpression(.Value, .Type)
                                meta = New MetaExpression(Function() -exp.Evaluate(environment).value)
                            End With
                        Else
                            ' 如果是其他类型的操作符，则肯定是语法错误了
                            Throw New NotImplementedException(t.Type)
                        End If

                    Case Else

                        Throw New NotImplementedException(t.Type)

                End Select

                If Not tokenBuffer.EndRead Then
                    meta.Operator = (++tokenBuffer).Text
                Else
                    ' 2017-10-18
                    ' 在计算表达式的时候这里使用+运算符可能会出问题，现在改为一个复杂的字符串来防止bug出现
                    meta.Operator = "$$$$ END_OF_EXPRESSION $$$$"
                End If

                Call expression.Add(meta)
            Loop

            Return expression
        End Function

        <Extension>
        Private Function Vector(values As TempValue()) As Object
            Dim allTypes = values _
                .Select(Function(x) x.type) _
                .Distinct _
                .ToArray

            If allTypes.Length = 1 Then
                With allTypes(Scan0)
                    If .IsPrimitive Then

                        Dim type = .DotNETtype
                        Dim vec As Array = Array.CreateInstance(type, values.Length)

                        For i As Integer = 0 To values.Length - 1
                            Call vec.SetValue(values(i).value, i)
                        Next

                        Return New TempValue With {
                            .value = vec,
                            .type = allTypes(0)
                        }
                    Else
                        Return New TempValue With {
                            .value = values.Select(Function(x) x.value).ToArray,
                            .type = allTypes(0)
                        }
                    End If
                End With
            Else
                Return New TempValue With {
                    .type = TypeCodes.generic,
                    .value = values _
                        .Select(Function(x) x.value) _
                        .ToArray
                }
            End If
        End Function
    End Module
End Namespace
