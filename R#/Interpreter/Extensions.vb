#Region "Microsoft.VisualBasic::578319d4e1e6e39ca54951811fc0c776, R#\Interpreter\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) GetExpressions, isTerminator
    ' 
    '         Sub: SyntaxErrorHelper
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter

    <HideModuleName> Module Extensions

        ReadOnly ignores As Index(Of TokenType) = {
            TokenType.comment,
            TokenType.terminator
        }

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Private Function isTerminator(block As Token()) As Boolean
            Return block.Length = 1 AndAlso block(Scan0).name Like ignores
        End Function

        <Extension>
        Public Function GetExpressions(Rscript As Rscript, opts As SyntaxBuilderOptions) As IEnumerable(Of Expression)
            Return Rscript.GetTokens.GetExpressions(Rscript, Nothing, opts)
        End Function

        <Extension>
        Public Iterator Function GetExpressions(tokens As Token(), Rscript As Rscript, errHandler As Action(Of SyntaxResult), opts As SyntaxBuilderOptions) As IEnumerable(Of Expression)
            For Each block In tokens.SplitByTopLevelDelimiter(TokenType.terminator)
                If block.Length = 0 OrElse block.isTerminator Then
                    ' skip code comments
                    ' do nothing
                Else
                    ' have some bugs about
                    ' handles closure
                    Dim parts() = block _
                        .Where(Function(t) Not t.name = TokenType.comment) _
                        .SplitByTopLevelDelimiter(TokenType.close,, "}") _
                        .Split(2)
                    Dim expr As SyntaxResult

                    For Each joinBlock In parts
                        block = joinBlock(Scan0).JoinIterates(joinBlock.ElementAtOrDefault(1)).ToArray
                        expr = Expression.CreateExpression(block, opts)

                        If expr.isException Then
                            If errHandler Is Nothing Then
                                Call SyntaxErrorHelper(
                                    syntaxResult:=expr,
                                    Rscript:=Rscript,
                                    tokens:=block
                                )
                            Else
                                Call errHandler(expr)
                                Exit For
                            End If
                        Else
                            Yield expr.expression
                        End If
                    Next
                End If
            Next
        End Function

        Private Sub SyntaxErrorHelper(Rscript As Rscript, tokens As Token(), syntaxResult As SyntaxResult)
            Dim rawText As String = Rscript.GetRawText(tokens)
            Dim err As Exception = syntaxResult.error
            Dim message As String = err.ToString

            message &= vbCrLf & vbCrLf & "Syntax error nearby:"
            message &= vbCrLf & vbCrLf & rawText
            message &= vbCrLf & vbCrLf & "The parser stack trace:"
            message &= vbCrLf & vbCrLf & syntaxResult.stackTrace
            message &= vbCrLf & vbCrLf

            Throw New Exception(message)
        End Sub
    End Module
End Namespace
