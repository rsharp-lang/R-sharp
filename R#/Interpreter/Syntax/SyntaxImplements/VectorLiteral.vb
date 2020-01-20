#Region "Microsoft.VisualBasic::25d53edb16f3da8b56621f487b6e7620, R#\Interpreter\Syntax\SyntaxImplements\VectorLiteral.vb"

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

    '     Module VectorLiteralSyntax
    ' 
    '         Function: LiteralSyntax, (+2 Overloads) SequenceLiteral, TypeCodeOf, VectorLiteral
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
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module VectorLiteralSyntax

        Public Function LiteralSyntax(token As Token) As SyntaxResult
            Select Case token.name
                Case TokenType.booleanLiteral
                    Return New Literal With {.m_type = TypeCodes.boolean, .value = token.text.ParseBoolean}
                Case TokenType.integerLiteral
                    Return New Literal With {.m_type = TypeCodes.integer, .value = CLng(token.text.ParseInteger)}
                Case TokenType.numberLiteral
                    Return New Literal With {.m_type = TypeCodes.double, .value = token.text.ParseDouble}
                Case TokenType.stringLiteral, TokenType.cliShellInvoke
                    Return New Literal With {.m_type = TypeCodes.string, .value = token.text}
                Case TokenType.missingLiteral
                    Dim type = TypeCodes.generic
                    Dim value As Object

                    Select Case token.text
                        Case "NULL" : value = Nothing
                        Case "NA" : value = GetType(Void)
                        Case "Inf" : value = Double.PositiveInfinity
                        Case Else
                            Return New SyntaxResult(New SyntaxErrorException)
                    End Select

                    Return New Literal With {.m_type = type, .value = value}
                Case Else
                    Return New SyntaxResult(New InvalidExpressionException(token.ToString))
            End Select
        End Function

        Public Function VectorLiteral(tokens As Token()) As SyntaxResult
            Dim blocks As List(Of Token()) = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .SplitByTopLevelDelimiter(TokenType.comma)
            Dim values As New List(Of Expression)
            Dim syntaxTemp As SyntaxResult

            For Each block As Token() In blocks
                If Not (block.Length = 1 AndAlso block(Scan0).name = TokenType.comma) Then
                    syntaxTemp = block.DoCall(AddressOf Expression.CreateExpression)

                    If syntaxTemp.isException Then
                        Return syntaxTemp
                    Else
                        values.Add(syntaxTemp.expression)
                    End If
                End If
            Next

            ' 还会剩余最后一个元素
            ' 所以在这里需要加上
            Return New SyntaxResult(New VectorLiteral(values, values.DoCall(AddressOf TypeCodeOf)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function TypeCodeOf(values As IEnumerable(Of Expression)) As TypeCodes
            With values.ToArray
                ' fix for System.InvalidOperationException: Nullable object must have a value.
                '
                If .Length = 0 Then
                    Return TypeCodes.generic
                Else
                    Return values _
                        .GroupBy(Function(exp) exp.type) _
                        .OrderByDescending(Function(g) g.Count) _
                        .First _
                       ?.Key
                End If
            End With
        End Function

        Public Function SequenceLiteral(from As Token, [to] As Token, steps As Token) As SyntaxResult
            Return SequenceLiteral({from}, {[to]}, {steps})
        End Function

        Public Function SequenceLiteral(from As Token(), [to] As Token(), steps As Token()) As SyntaxResult
            Dim fromSyntax = Expression.CreateExpression(from)
            Dim toSyntax = Expression.CreateExpression([to])

            If fromSyntax.isException Then
                Return fromSyntax
            ElseIf toSyntax.isException Then
                Return toSyntax
            End If

            If steps.IsNullOrEmpty Then
                Return New SequenceLiteral(fromSyntax.expression, toSyntax.expression, New Literal(1))
            ElseIf steps.isLiteral Then
                Dim stepLiteral As SyntaxResult = SyntaxImplements.LiteralSyntax(steps(Scan0))

                If stepLiteral.isException Then
                    Return stepLiteral
                End If

                Return New SequenceLiteral(
                    from:=fromSyntax.expression,
                    [to]:=toSyntax.expression,
                    steps:=stepLiteral.expression
                )
            Else
                Dim stepsSyntax = Expression.CreateExpression(steps)

                If stepsSyntax.isException Then
                    Return stepsSyntax
                Else
                    Return New SequenceLiteral(
                        from:=fromSyntax.expression,
                        [to]:=toSyntax.expression,
                        steps:=stepsSyntax.expression
                    )
                End If
            End If
        End Function
    End Module
End Namespace
