#Region "Microsoft.VisualBasic::6156ce6978af481cbb1d0f1e9df62c46, R#\Interpreter\Syntax\SyntaxTree\ExpressionTree.vb"

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

    '     Module ExpressionTree
    ' 
    '         Function: CreateTree, ParseExpressionTree, simpleSequence
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser

    Module ExpressionTree

        <Extension>
        Public Function CreateTree(tokens As Token()) As SyntaxResult
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.comma)

            If blocks = 1 Then
                Dim exp As Expression = blocks(Scan0).simpleSequence

                If Not exp Is Nothing Then
                    Return exp
                Else
                    ' 是一个复杂的表达式
                    Return blocks(Scan0).ParseExpressionTree
                End If
            Else
                Return New SyntaxResult(New NotImplementedException)
            End If
        End Function

        <Extension>
        Private Function simpleSequence(tokens As Token()) As SequenceLiteral
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.sequence)

            If blocks = 3 Then
                Return New SequenceLiteral(blocks(Scan0), blocks(2), Nothing)
            ElseIf blocks = 5 Then
                Return New SequenceLiteral(blocks(Scan0), blocks(2), blocks.ElementAtOrDefault(4))
            Else
                Return Nothing
            End If
        End Function

        <Extension>
        Private Function ParseExpressionTree(tokens As Token()) As SyntaxResult
            Dim blocks As List(Of Token())

            If tokens.Length = 1 Then
                If tokens(Scan0).name = TokenType.stringInterpolation Then
                    Return SyntaxImplements.StringInterpolation(tokens(Scan0))
                ElseIf tokens(Scan0).name = TokenType.cliShellInvoke Then
                    Return SyntaxImplements.CommandLine(tokens(Scan0))
                ElseIf tokens(Scan0) = (TokenType.operator, "$") Then
                    Return New SymbolReference("$")
                Else
                    blocks = New List(Of Token()) From {tokens}
                End If
            ElseIf tokens.Length = 2 AndAlso tokens(Scan0).name = TokenType.iif Then
                Return New CommandLineArgument(tokens)
            Else
                blocks = tokens.SplitByTopLevelDelimiter(TokenType.operator)
            End If

            If blocks = 1 Then
                ' 简单的表达式
                If tokens.isFunctionInvoke Then
                    Return SyntaxImplements.FunctionInvoke(tokens)
                ElseIf tokens.isSimpleSymbolIndexer Then
                    Return SyntaxImplements.SymbolIndexer(tokens)
                ElseIf tokens(Scan0).name = TokenType.open Then
                    Dim openSymbol = tokens(Scan0).text

                    If openSymbol = "[" Then
                        Return New VectorLiteral(tokens)
                    ElseIf openSymbol = "(" Then
                        ' 是一个表达式
                        Return tokens _
                            .Skip(1) _
                            .Take(tokens.Length - 2) _
                            .SplitByTopLevelDelimiter(TokenType.operator) _
                            .DoCall(AddressOf ParseBinaryExpression)
                    ElseIf openSymbol = "{" Then
                        ' 是一个可以产生值的closure
                        Return New ClosureExpression(tokens)
                    End If
                ElseIf tokens(Scan0).name = TokenType.stringInterpolation Then
                    Return SyntaxImplements.StringInterpolation(tokens(Scan0))
                Else
                    Dim indexer As SyntaxResult = tokens.parseComplexSymbolIndexer

                    If Not indexer Is Nothing Then
                        Return indexer
                    End If
                End If
            Else
                Return ParseBinaryExpression(blocks)
            End If

            Return New SyntaxResult(New NotImplementedException)
        End Function
    End Module
End Namespace
