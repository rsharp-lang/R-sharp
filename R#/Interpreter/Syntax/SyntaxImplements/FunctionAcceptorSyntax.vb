#Region "Microsoft.VisualBasic::9248ca698aafc0b3d41cf2a54ce6fadb, R#\Interpreter\Syntax\SyntaxImplements\FunctionAcceptorSyntax.vb"

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

    '     Module FunctionAcceptorSyntax
    ' 
    '         Function: (+3 Overloads) FunctionAcceptorInvoke
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module FunctionAcceptorSyntax

        Public Function FunctionAcceptorInvoke(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")
            Dim funcInvoke As SyntaxResult = code(Scan0) _
                .JoinIterates(code(1)) _
                .DoCall(Function(fi)
                            Return Expression.CreateExpression(fi, opts)
                        End Function)

            If funcInvoke.isException Then
                Return funcInvoke
            Else
                Return DirectCast(funcInvoke.expression, FunctionInvoke).FunctionAcceptorInvoke(code(2), opts)
            End If
        End Function

        <Extension>
        Private Function FunctionAcceptorInvoke(invoke As FunctionInvoke, firstBody As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim Rscript As Rscript = opts.source
            Dim firstParameterBody As Expression() = firstBody _
                .Skip(1) _
                .Take(firstBody.Length - 2) _
                .ToArray _
                .GetExpressions(Rscript, Nothing, opts) _
                .ToArray

            If opts.haveSyntaxErr Then
                Return SyntaxResult.CreateError(opts.error, opts.SetCurrentRange(firstBody))
            End If

            Dim parameterClosure As New AcceptorClosure(firstParameterBody)
            Dim args As New List(Of Expression)

            args.Add(parameterClosure)
            args.AddRange(invoke.parameters)
            invoke.parameters = args.ToArray

            Return New SyntaxResult(invoke)
        End Function

        Public Function FunctionAcceptorInvoke(invoke As Token(), firstBody As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim funcInvoke As SyntaxResult = Expression.CreateExpression(invoke, opts)

            If funcInvoke.isException Then
                Return funcInvoke
            Else
                Return DirectCast(funcInvoke.expression, FunctionInvoke).FunctionAcceptorInvoke(firstBody, opts)
            End If
        End Function
    End Module
End Namespace
