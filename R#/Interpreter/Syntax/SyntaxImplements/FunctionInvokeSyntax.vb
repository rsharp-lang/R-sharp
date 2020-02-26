#Region "Microsoft.VisualBasic::3121cc270e0db45ef2523f3d141a6d33, R#\Interpreter\Syntax\SyntaxImplements\FunctionInvokeSyntax.vb"

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

    '     Module FunctionInvokeSyntax
    ' 
    '         Function: (+2 Overloads) AnonymousFunctionInvoke, FunctionInvoke, getInvokeParameters, getNameRef
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module FunctionInvokeSyntax

        Private Function getNameRef(token As Token) As Expression
            If token.name = TokenType.regexp Then
                Return New Regexp(token.text)
            Else
                Return New Literal(token.text)
            End If
        End Function

        Public Function FunctionInvoke(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim funcName As Expression = getNameRef(tokens(Scan0))
            Dim span As CodeSpan = tokens(Scan0).span
            Dim parameters As New List(Of Expression)
            Dim frame As New StackFrame With {
                .File = opts.source.fileName,
                .Line = tokens(Scan0).span.line,
                .Method = New Method With {
                    .Method = funcName.ToString,
                    .[Module] = "call_function",
                    .[Namespace] = "SMRUCC/R#"
                }
            }

            For Each a As SyntaxResult In tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .getInvokeParameters(opts)

                If a.isException Then
                    Return a
                Else
                    parameters.Add(a.expression)
                End If
            Next

            Return New FunctionInvoke(funcName, frame, parameters.ToArray)
        End Function

        <Extension>
        Private Iterator Function getInvokeParameters(params As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As IEnumerable(Of SyntaxResult)
            For Each token As SyntaxResult In params _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .Select(Function(param)
                            ' name = value
                            ' value
                            ' fix bugs of using the keyword as identifier
                            Dim parts = param.SplitByTopLevelDelimiter(TokenType.operator, False, "=")

                            If parts = 3 Then
                                ' name = value
                                Return SyntaxImplements.ValueAssign(parts, opts)
                            Else
                                ' is a value expression
                                Return Expression.CreateExpression(param, opts)
                            End If
                        End Function)

                Yield token
            Next
        End Function

        Public Function AnonymousFunctionInvoke(anonymous As Token(), invoke As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            ' declares anonymous function
            Dim code As New List(Of Token()) From {
                New Token() {anonymous(Scan0)},
                anonymous.Skip(1).ToArray
            }
            Dim declares = DeclareNewFunctionSyntax.DeclareAnonymousFunction(code, opts)

            If declares.isException Then
                Return declares
            Else
                Return declares.AnonymousFunctionInvoke(
                    invoke:=invoke,
                    lineNum:=anonymous(Scan0).span.line,
                    opts:=opts
                )
            End If
        End Function

        <Extension>
        Public Function AnonymousFunctionInvoke(declares As SyntaxResult, invoke As Token(), lineNum%, opts As SyntaxBuilderOptions) As SyntaxResult
            Dim parameters As New List(Of Expression)
            Dim frame As New StackFrame With {
                .File = opts.source.fileName,
                .Line = lineNum,
                .Method = New Method With {
                    .Method = DirectCast(declares.expression, IRuntimeTrace).stackFrame.Method.Method,
                    .[Module] = "call_function",
                    .[Namespace] = "SMRUCC/R#"
                }
            }

            For Each a As SyntaxResult In invoke _
                .Skip(1) _
                .Take(invoke.Length - 2) _
                .getInvokeParameters(opts)

                If a.isException Then
                    Return a
                Else
                    parameters.Add(a.expression)
                End If
            Next

            Return New FunctionInvoke(declares.expression, frame, parameters.ToArray)
        End Function
    End Module
End Namespace
