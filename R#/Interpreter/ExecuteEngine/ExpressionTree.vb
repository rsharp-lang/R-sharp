#Region "Microsoft.VisualBasic::a53402d219f7d4b81548f9a2bc4bda14, R#\Interpreter\ExecuteEngine\ExpressionTree.vb"

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
    '         Function: CreateTree, ParseBinaryExpression, ParseExpressionTree
    ' 
    '         Sub: genericSymbolOperatorProcessor, processNameMemberReference, processNamespaceReference, processOperators, processPipeline
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine

    Module ExpressionTree

        <Extension>
        Public Function CreateTree(tokens As Token()) As Expression
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.comma)

            If blocks = 1 Then
                ' 是一个复杂的表达式
                Return blocks(Scan0).ParseExpressionTree
            Else
                Throw New NotImplementedException
            End If
        End Function

        <Extension>
        Private Function ParseExpressionTree(tokens As Token()) As Expression
            Dim blocks As List(Of Token())

            If tokens.Length = 1 Then
                If tokens(Scan0).name = TokenType.stringInterpolation Then
                    Return New StringInterpolation(tokens(Scan0))
                ElseIf tokens(Scan0).name = TokenType.cliShellInvoke Then
                    Return New CommandLine(tokens(Scan0))
                ElseIf tokens(Scan0) = (TokenType.operator, "$") Then
                    Return New SymbolReference("$")
                Else
                    blocks = New List(Of Token()) From {tokens}
                End If
            Else
                blocks = tokens.SplitByTopLevelDelimiter(TokenType.operator)
            End If

            If blocks = 1 Then
                Dim complexSequence = tokens.SplitByTopLevelDelimiter(TokenType.sequence)

                If complexSequence.Count = 3 OrElse complexSequence.Count = 5 Then
                    Return New SequenceLiteral(complexSequence(0), complexSequence(2), complexSequence.ElementAtOrDefault(4))
                End If

                ' 简单的表达式
                If tokens.isFunctionInvoke Then
                    Return New FunctionInvoke(tokens)
                ElseIf tokens.isSymbolIndexer Then
                    Return New SymbolIndexer(tokens)
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
                        Throw New NotImplementedException
                    End If
                ElseIf tokens(Scan0).name = TokenType.stringInterpolation Then
                    Return New StringInterpolation(tokens(Scan0))
                End If
            Else
                Return ParseBinaryExpression(blocks)
            End If

            Throw New NotImplementedException
        End Function
    End Module
End Namespace
