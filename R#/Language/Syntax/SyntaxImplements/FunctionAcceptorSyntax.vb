#Region "Microsoft.VisualBasic::2d7ceeb78e1a574c7546b6499791a373, R#\Language\Syntax\SyntaxImplements\FunctionAcceptorSyntax.vb"

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

    '   Total Lines: 64
    '    Code Lines: 53 (82.81%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (17.19%)
    '     File Size: 2.72 KB


    '     Module FunctionAcceptorSyntax
    ' 
    '         Function: CreateInvoke, (+3 Overloads) FunctionAcceptorInvoke
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module FunctionAcceptorSyntax

        Public Function FunctionAcceptorInvoke(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")
            Dim funcInvoke As SyntaxResult = code(Scan0) _
                .JoinIterates(code(1)) _
                .DoCall(Function(fi)
                            Return opts.ParseExpression(fi, opts)
                        End Function)

            If funcInvoke.isException Then
                Return funcInvoke
            Else
                Dim invoke As FunctionInvoke = TryCast(funcInvoke.expression, FunctionInvoke)

                If funcInvoke.expression IsNot Nothing AndAlso invoke Is Nothing Then
                    Dim msg As String = $"expression {funcInvoke.expression.GetType.Name} can not be convert to function invoke!"
                    Dim start As CodeSpan = tokens.First.span
                    Dim ends As CodeSpan = tokens.Last.span

                    Return New SyntaxResult(SyntaxError.CreateError(opts, msg, start, ends))
                Else
                    Return invoke.FunctionAcceptorInvoke(code(2), opts)
                End If
            End If
        End Function

        <Extension>
        Private Function FunctionAcceptorInvoke(invoke As FunctionInvoke, firstBody As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim firstParameterBody As Expression() = firstBody _
                .Skip(1) _
                .Take(firstBody.Length - 2) _
                .ToArray _
                .GetExpressions(Nothing, opts) _
                .ToArray

            If opts.haveSyntaxErr Then
                Return SyntaxResult.CreateError(opts.error, opts.SetCurrentRange(firstBody))
            Else
                Return invoke.CreateInvoke(firstParameterBody, opts)
            End If
        End Function

        <Extension>
        Public Function CreateInvoke(invoke As FunctionInvoke, program As IEnumerable(Of Expression), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim parameterClosure As New AcceptorClosure(program.ToArray)
            Dim args As New List(Of Expression)

            args.Add(parameterClosure)
            args.AddRange(invoke.parameters)
            invoke.parameters = args.ToArray

            Return New SyntaxResult(invoke)
        End Function

        Public Function FunctionAcceptorInvoke(invoke As Token(), firstBody As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim funcInvoke As SyntaxResult = opts.ParseExpression(invoke, opts)

            If funcInvoke.isException Then
                Return funcInvoke
            Else
                Return DirectCast(funcInvoke.expression, FunctionInvoke).FunctionAcceptorInvoke(firstBody, opts)
            End If
        End Function
    End Module
End Namespace
