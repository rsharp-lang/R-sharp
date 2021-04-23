#Region "Microsoft.VisualBasic::774fbb3c052bbc812dbe3f59a695355c, LINQ\LINQ\Script\Builders\BinaryBuilder.vb"

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

' Module BinaryBuilder
' 
'     Function: ParseBinary, ShrinkTokens
' 
'     Sub: JoinBinary
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.LINQ.Syntax

    Module BinaryBuilder

        ReadOnly orders As String()() = {
            New String() {"^"},
            New String() {"/", "*", "%"},
            New String() {"+", "-"},
            New String() {"&"},
            New String() {"and", "or"}
        }

        <Extension>
        Public Function ParseBinary(tokenList As Token()) As Expression
            Dim shrinks As List(Of [Variant](Of String, Expression)) = tokenList.SplitOperators.ShrinkTokens.AsList

            If shrinks = 3 Then
                Return New BinaryExpression(shrinks(0), shrinks(2), shrinks(1))
            End If

            For Each level As String() In orders
                Call shrinks.JoinBinary(listOp:=level)
            Next

            If shrinks = 3 Then
                Return New BinaryExpression(shrinks(0), shrinks(2), shrinks(1))
            ElseIf shrinks = 1 Then
                Return shrinks(Scan0)
            Else
                Throw New SyntaxErrorException
            End If
        End Function

        <Extension>
        Private Sub JoinBinary(ByRef shrinks As List(Of [Variant](Of String, Expression)), listOp As Index(Of String))
            For i As Integer = 1 To shrinks.Count - 1
                If i >= shrinks.Count Then
                    Return
                End If

                If shrinks(i) Like GetType(String) AndAlso listOp.IndexOf(shrinks(i).TryCast(Of String)) > -1 Then
                    Dim bin As New BinaryExpression(shrinks(i - 1), shrinks(i + 1), shrinks(i))

                    shrinks.RemoveRange(i - 1, 3)
                    shrinks.Insert(i - 1, bin)
                End If
            Next
        End Sub

        <Extension>
        Private Iterator Function ShrinkTokens(blocks As IEnumerable(Of Token())) As IEnumerable(Of [Variant](Of String, Expression))
            For Each block As Token() In blocks.Where(Function(b) Not b.IsNullOrEmpty)
                If block.Length = 1 AndAlso block(Scan0).name = TokenType.operator Then
                    Yield block(Scan0).text
                ElseIf block.Length = 3 AndAlso block(1).name = Tokentype.operator Then
                    Yield New MemberReference(ParseToken(block(0)), ParseToken(block(2)))
                Else
                    Yield block.ParseExpression
                End If
            Next
        End Function
    End Module
End Namespace