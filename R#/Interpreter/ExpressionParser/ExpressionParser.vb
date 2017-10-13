#Region "Microsoft.VisualBasic::49de36852608b05a494d483a491816ef, ..\R-sharp\R#\Interpreter\ExpressionParser\ExpressionParser.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Runtime
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

                t = ++tokens

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
                    meta.Operator = (++tokens).Text
                Else
                    meta.Operator = "+"
                End If

                Call expression.Add(meta)
            Loop

            Return expression
        End Function
    End Module
End Namespace
