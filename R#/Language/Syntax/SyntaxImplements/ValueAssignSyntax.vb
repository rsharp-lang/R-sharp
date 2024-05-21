#Region "Microsoft.VisualBasic::bd7dfab18d07e70e49174d5cd9594342, R#\Language\Syntax\SyntaxImplements\ValueAssignSyntax.vb"

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

    '   Total Lines: 80
    '    Code Lines: 69 (86.25%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 11 (13.75%)
    '     File Size: 3.32 KB


    '     Module ValueAssignSyntax
    ' 
    '         Function: ValueAssign, ValueIterateAssign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module ValueAssignSyntax

        Public Function ValueIterateAssign(target As Token(), op As String, value As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim symbolNames = DeclareNewSymbolSyntax.getNames(target, opts)

            If symbolNames Like GetType(SyntaxErrorException) Then
                Return SyntaxResult.CreateError(
                    err:=symbolNames.TryCast(Of SyntaxErrorException),
                    opts:=opts.SetCurrentRange(target)
                )
            ElseIf symbolNames.TryCast(Of String()).Length > 1 Then
                Return SyntaxResult.CreateError(
                    err:=New NotImplementedException("iterate assign of tuple is not supported yet"),
                    opts:=opts.SetCurrentRange(target)
                )
            Else
                op = op.First
            End If

            Dim targetSymbols As Literal() = symbolNames _
                .TryCast(Of String()) _
                .Select(Function(name) New Literal(name)) _
                .ToArray
            Dim valueExpr As SyntaxResult = {value}.AsList.ParseExpression(opts)

            If valueExpr.isException Then
                Return valueExpr
            Else
                Dim expr As Expression = BinaryExpressionTree.CreateBinary(
                    a:=New SymbolReference(targetSymbols(Scan0).ValueStr),
                    b:=valueExpr.expression,
                    opToken:=op,
                    opts:=opts
                )
                Dim assign As New ValueAssignExpression(targetSymbols, expr) With {
                    .isByRef = True
                }

                Return assign
            End If
        End Function

        Public Function ValueAssign(tokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim symbolNames = DeclareNewSymbolSyntax.getNames(tokens(Scan0), opts)

            If symbolNames Like GetType(SyntaxErrorException) Then
                Return SyntaxResult.CreateError(
                    err:=symbolNames.TryCast(Of SyntaxErrorException),
                    opts:=opts.SetCurrentRange(tokens(Scan0))
                )
            End If

            Dim targetSymbols As Literal() = symbolNames _
                .TryCast(Of String()) _
                .Select(Function(name) New Literal(name)) _
                .ToArray
            Dim isByRef As Boolean = tokens(Scan0)(Scan0).text = "="
            Dim value As SyntaxResult = tokens.Skip(2) _
                .AsList _
                .ParseExpression(opts)

            If value.isException Then
                Return value
            Else
                Return New ValueAssignExpression(targetSymbols, value.expression) With {
                    .isByRef = isByRef
                }
            End If
        End Function
    End Module
End Namespace
