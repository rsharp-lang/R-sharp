#Region "Microsoft.VisualBasic::f8410fdda8baa937120158c07e1fd8be, R#\Interpreter\Syntax\SyntaxImplements\VectorLiteral.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module VectorLiteralSyntax

        Public Function LiteralSyntax(token As Token, opts As SyntaxBuilderOptions) As SyntaxResult
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
                            Return New SyntaxResult(New SyntaxErrorException, opts.debug)
                    End Select

                    Return New Literal With {.m_type = type, .value = value}
                Case Else
                    Return New SyntaxResult(New InvalidExpressionException(token.ToString), opts.debug)
            End Select
        End Function

        Public Function VectorLiteral(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim blocks As List(Of Token()) = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .SplitByTopLevelDelimiter(TokenType.comma)
            Dim values As New List(Of Expression)
            Dim syntaxTemp As SyntaxResult

            For Each block As Token() In blocks
                If Not (block.Length = 1 AndAlso block(Scan0).name = TokenType.comma) Then
                    syntaxTemp = block.DoCall(Function(code) Expression.CreateExpression(code, opts))

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

        ''' <summary>
        ''' get type code value of the vector literal 
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function TypeCodeOf(values As IEnumerable(Of Expression)) As TypeCodes
            With values.ToArray
                ' fix for System.InvalidOperationException: Nullable object must have a value.
                '
                If .Length = 0 Then
                    Return TypeCodes.generic
                ElseIf .Length = 1 Then
                    Return DirectCast(.GetValue(Scan0), Expression).type
                Else
                    ' generic > string > double > float > long > integer > byte > boolean
                    Static typeCodeWeights As Index(Of TypeCodes) = {
                        TypeCodes.boolean,
                        TypeCodes.integer,
                        TypeCodes.double,
                        TypeCodes.string,
                        TypeCodes.generic
                    }

                    ' get unique types
                    Dim types As TypeCodes() = .Select(Function(exp) exp.type) _
                                               .Distinct _
                                               .ToArray
                    If types.Length = 1 Then
                        Return types(Scan0)
                    Else
                        Dim maxType As TypeCodes = TypeCodes.boolean
                        Dim maxWeight As Integer

                        For Each code As TypeCodes In types
                            If typeCodeWeights.IndexOf(code) > maxWeight Then
                                maxType = code
                                maxWeight = typeCodeWeights(maxType)
                            End If
                        Next

                        Return maxType
                    End If
                End If
            End With
        End Function

        Public Function SequenceLiteral(from As Token, [to] As Token, steps As Token, opts As SyntaxBuilderOptions) As SyntaxResult
            Return SequenceLiteral({from}, {[to]}, {steps}, opts)
        End Function

        Public Function SequenceLiteral(from As Token(), [to] As Token(), steps As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim fromSyntax = Expression.CreateExpression(from, opts)
            Dim toSyntax = Expression.CreateExpression([to], opts)

            If fromSyntax.isException Then
                Return fromSyntax
            ElseIf toSyntax.isException Then
                Return toSyntax
            End If

            If steps.IsNullOrEmpty Then
                Return New SequenceLiteral(fromSyntax.expression, toSyntax.expression, New Literal(1))
            ElseIf steps.isLiteral Then
                Dim stepLiteral As SyntaxResult = SyntaxImplements.LiteralSyntax(steps(Scan0), opts)

                If stepLiteral.isException Then
                    Return stepLiteral
                End If

                Return New SequenceLiteral(
                    from:=fromSyntax.expression,
                    [to]:=toSyntax.expression,
                    steps:=stepLiteral.expression
                )
            Else
                Dim stepsSyntax As SyntaxResult = Expression.CreateExpression(steps, opts)

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
