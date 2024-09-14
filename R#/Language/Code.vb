﻿#Region "Microsoft.VisualBasic::2ff165ec5c3cc8ead8c47c7411a788e9, R#\Language\Code.vb"

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

    '   Total Lines: 83
    '    Code Lines: 65 (78.31%)
    ' Comment Lines: 6 (7.23%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 12 (14.46%)
    '     File Size: 3.12 KB


    '     Module Code
    ' 
    '         Function: GetCodeSpan, Indent, isLINQKeyword, joinNext, ParseScript
    '                   ParseVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language

    Module Code

        <Extension>
        Public Function Indent(exp As Expression, Optional n As Integer = 3) As String
            Dim indent_str As New String(" "c, n)
            Dim script As String = exp.ToString _
                .LineTokens _
                .Select(Function(line) indent_str & line) _
                .JoinBy(vbLf)

            Return script
        End Function

        Public Function ParseVector(expr As String) As VectorLiteral
            Dim tokens As Token() = ParseScript(expr)
            Dim opt As New SyntaxBuilderOptions(AddressOf Expression.CreateExpression, Function(c, s) New Scanner(c, s)) With {
                .debug = False,
                .source = Rscript.FromText(expr)
            }
            Dim vec As SyntaxResult = VectorLiteralSyntax.VectorLiteral(tokens, opt)

            If vec.isException Then
                Throw New InvalidExpressionException(vec.error.ToString)
            Else
                Return vec.expression
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function ParseScript(script As String) As Token()
            Return New Scanner(script).GetTokens().ToArray
        End Function

        ''' <summary>
        ''' For parse the raw script text
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetCodeSpan(code As IEnumerable(Of Token)) As IntRange
            With code.OrderBy(Function(t) t.span.start).ToArray
                Return New IntRange(.First.span.start, .Last.span.stops)
            End With
        End Function

        <Extension>
        Friend Function joinNext(tken As Token, token As String) As JoinToken
            Dim nextToken As Token = Scanner.MeasureToken(token, Scanner.Rkeywords, Scanner.RNullLiteral, AddressOf isLINQKeyword)
            Dim join As New JoinToken With {
                .name = tken.name,
                .text = tken.text,
                .span = tken.span,
                .[next] = nextToken
            }

            Return join
        End Function

        Private Function isLINQKeyword(word As String) As Boolean
            Dim lwd As String = Strings.LCase(word)

            Select Case lwd
                ' Linq中没有if表达式
                Case "if"
                    Return False
                Case Else
                    Return Strings.LCase(word) Like Scanner.Rkeywords
            End Select
        End Function
    End Module
End Namespace
