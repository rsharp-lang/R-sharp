#Region "Microsoft.VisualBasic::d61917e597e90838f11bec5899e66471, E:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/Syntax/BinaryBuilder.vb"

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

    '   Total Lines: 118
    '    Code Lines: 103
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 4.94 KB


    '     Module BinaryBuilder
    ' 
    '         Function: ParseBinary, ParseUnary, ShrinkTokens
    ' 
    '         Sub: JoinBinary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.LINQ.Syntax

    Module BinaryBuilder

        ReadOnly orders As String()() = {
            New String() {"^"},
            New String() {"/", "*", "%"},
            New String() {"+", "-"},
            New String() {"&"},
            New String() {">", "<", ">=", "<=", "==", "!="},
            New String() {"&&", "||"}
        }

        <Extension>
        Private Function ParseUnary(tokenList As Token(), opts As SyntaxBuilderOptions) As SyntaxParserResult
            If tokenList(1).isNumeric Then
                Return New Literal(-Val(tokenList(1).text))
            ElseIf tokenList(1).name = TokenType.identifier Then
                Throw New NotImplementedException
            Else
                Throw New NotImplementedException
            End If
        End Function

        <Extension>
        Public Function ParseBinary(tokenList As Token(), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim shrinks As List(Of [Variant](Of String, SyntaxParserResult))

            If tokenList.Length = 2 AndAlso tokenList(Scan0) = (TokenType.operator, "-") Then
                Return tokenList.ParseUnary(opts)
            Else
                shrinks = tokenList _
                    .SplitOperators _
                    .ShrinkTokens(opts) _
                    .AsList
            End If

            For Each item In From x In shrinks Where Not x Like GetType(String)
                If item.TryCast(Of SyntaxParserResult).isError Then
                    Return item
                End If
            Next

            If shrinks = 3 Then
                Return New BinaryExpression(
                    left:=shrinks(0).TryCast(Of SyntaxParserResult).expression,
                    right:=shrinks(2).TryCast(Of SyntaxParserResult).expression,
                    op:=shrinks(1)
                )
            End If

            For Each level As String() In orders
                Call shrinks.JoinBinary(listOp:=level)
            Next

            If shrinks = 3 Then
                Return New BinaryExpression(
                    left:=shrinks(0).TryCast(Of SyntaxParserResult).expression,
                    right:=shrinks(2).TryCast(Of SyntaxParserResult).expression,
                    op:=shrinks(1)
                )
            ElseIf shrinks = 1 Then
                Return shrinks(Scan0)
            Else
                Throw New SyntaxErrorException
            End If
        End Function

        <Extension>
        Private Sub JoinBinary(ByRef shrinks As List(Of [Variant](Of String, SyntaxParserResult)), listOp As Index(Of String))
            For i As Integer = 1 To shrinks.Count - 1
                If i >= shrinks.Count Then
                    Return
                End If

                If shrinks(i) Like GetType(String) AndAlso listOp.IndexOf(shrinks(i).TryCast(Of String)) > -1 Then
                    Dim bin As New BinaryExpression(
                        left:=shrinks(i - 1).TryCast(Of SyntaxParserResult).expression,
                        right:=shrinks(i + 1).TryCast(Of SyntaxParserResult).expression,
                        op:=shrinks(i)
                    )

                    shrinks.RemoveRange(i - 1, 3)
                    shrinks.Insert(i - 1, New [Variant](Of String, SyntaxParserResult)(New SyntaxParserResult(bin)))
                End If
            Next
        End Sub

        <Extension>
        Private Iterator Function ShrinkTokens(blocks As IEnumerable(Of Token()), opts As SyntaxBuilderOptions) As IEnumerable(Of [Variant](Of String, SyntaxParserResult))
            For Each block As Token() In blocks.Where(Function(b) Not b.IsNullOrEmpty)
                If block.Length = 1 AndAlso block(Scan0).name = TokenType.operator Then
                    Yield block(Scan0).text
                ElseIf block.Length = 3 AndAlso block(1).name = TokenType.operator Then
                    Dim symbol As SyntaxParserResult = ParseToken(block(0), opts)
                    Dim member As SyntaxParserResult = ParseToken(block(2), opts)

                    If symbol.isError Then
                        Yield symbol
                    ElseIf member.isError Then
                        Yield member
                    Else
                        Yield New SyntaxParserResult(New MemberReference(symbol.expression, member.expression))
                    End If
                Else
                    Yield block.ParseExpression(opts)
                End If
            Next
        End Function
    End Module
End Namespace
