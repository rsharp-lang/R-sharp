#Region "Microsoft.VisualBasic::f5fc17f20cd95c1925ac4661dc8b312b, R#\Interpreter\Syntax\SyntaxTree\BinaryExpressionTree\NameMemberReference.vb"

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

    '     Class NameMemberReferenceProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: createIndexer, expression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Interpreter.SyntaxParser

    Friend Class NameMemberReferenceProcessor : Inherits GenericSymbolOperatorProcessor

        Sub New()
            Call MyBase.New("$")
        End Sub

        Protected Overrides Function view() As String
            Return "x$member"
        End Function

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
            If a.VA.isException Then
                Return a
            ElseIf b.VA.isException Then
                Return b
            Else
                Return createIndexer(a.VA.expression, b.VA.expression, opts)
            End If
        End Function

        Private Shared Function createIndexer(a As Expression, b As Expression, opts As SyntaxBuilderOptions) As SyntaxResult
            Dim typeofName As Type = b.GetType
            Dim nameSymbol As String

            If typeofName Is GetType(SymbolReference) Then
                nameSymbol = DirectCast(b, SymbolReference).symbol
            ElseIf typeofName Is GetType(Literal) Then
                nameSymbol = DirectCast(b, Literal).value
            ElseIf typeofName Is GetType(FunctionInvoke) Then
                Dim invoke As FunctionInvoke = DirectCast(b, FunctionInvoke)
                Dim funcVar As New SymbolIndexer(a, invoke.funcName)
                Dim stacktrace As New StackFrame With {
                    .File = opts.source.fileName,
                    .Line = "n/a",
                    .Method = New Method With {
                        .Method = funcVar.ToString,
                        .[Module] = "call_function",
                        .[Namespace] = "SMRUCC/R#"
                    }
                }

                Return New SyntaxResult(New FunctionInvoke(funcVar, stacktrace, invoke.parameters.ToArray))
            ElseIf typeofName Is GetType(BinaryInExpression) Then
                ' merge symbol into binary expression
                ' symbol$name in ...
                Dim bin As BinaryInExpression = DirectCast(b, BinaryInExpression)
                Dim left As SyntaxResult = createIndexer(a, bin.left, opts)

                If left.isException Then
                    Return left
                Else
                    bin.left = left.expression
                End If

                Return New SyntaxResult(bin)
            ElseIf typeofName Is GetType(BinaryExpression) Then
                ' merge symbol into binary expression
                ' symbol$name <op> ...
                Dim bin As BinaryExpression = DirectCast(b, BinaryExpression)
                Dim left As SyntaxResult = createIndexer(a, bin.left, opts)

                If left.isException Then
                    Return left
                Else
                    ' 20201216
                    ' replace left value
                    bin = New BinaryExpression(left.expression, bin.right, bin.operator)
                End If

                Return New SyntaxResult(bin)
            ElseIf typeofName Is GetType(SymbolIndexer) Then
                ' a$b[x]
                ' use symbol b as index name
                Dim bin As SymbolIndexer = DirectCast(b, SymbolIndexer)
                Dim left As SyntaxResult = createIndexer(a, bin.symbol, opts)

                If left.isException Then
                    Return left
                Else
                    bin.symbol = left.expression
                End If

                Return New SyntaxResult(bin)
            Else
                Return SyntaxResult.CreateError(New NotImplementedException, opts)
            End If

            ' a$b symbol reference
            Dim symbolRef As New SymbolIndexer(a, New Literal(nameSymbol))
            Return New SyntaxResult(symbolRef)
        End Function
    End Class
End Namespace
