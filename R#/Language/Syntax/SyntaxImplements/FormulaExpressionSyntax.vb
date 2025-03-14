﻿#Region "Microsoft.VisualBasic::cc84ad6327aa16f4f6b8916f80dfcf11, R#\Language\Syntax\SyntaxImplements\FormulaExpressionSyntax.vb"

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

    '   Total Lines: 55
    '    Code Lines: 43 (78.18%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (21.82%)
    '     File Size: 2.34 KB


    '     Module FormulaExpressionSyntax
    ' 
    '         Function: CreateFormula, GetExpressionLiteral, (+2 Overloads) RunParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module FormulaExpressionSyntax

        Public Function RunParse(target As Token, formula As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim y As String = target.text
            Dim expression As SyntaxResult = opts.ParseExpression(formula, opts)

            If expression.isException Then
                Return expression
            End If

            Return New FormulaExpression(y, expression.expression)
        End Function

        Public Function RunParse(code As Token()(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim y As String = code(Scan0)(Scan0).text
            Dim expression As SyntaxResult = opts.ParseExpression(code.Skip(2).IteratesALL, opts)

            If expression.isException Then
                Return expression
            End If

            Return New FormulaExpression(y, expression.expression)
        End Function

        Public Function GetExpressionLiteral(code As Token()(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim literalTokens As Token() = code.IteratesALL.Skip(1).ToArray
            Dim stackName As String = $"[expression_literal] {literalTokens.Select(Function(a) a.text).JoinBy("")}"
            Dim stackFrame As StackFrame = opts.GetStackTrace(code(Scan0)(Scan0), stackName)
            Dim literal As SyntaxResult = opts.ParseExpression(literalTokens, opts)

            If literal.isException Then
                Return literal
            Else
                Return New ExpressionLiteral(literal.expression, stackFrame)
            End If
        End Function

        Public Function CreateFormula(response As SyntaxResult, data As SyntaxResult, opts As SyntaxBuilderOptions) As SyntaxResult
            If response.isException Then
                Return response
            ElseIf data.isException Then
                Return data
            End If

            Return New FormulaExpression(response.expression, data.expression)
        End Function
    End Module
End Namespace
