#Region "Microsoft.VisualBasic::c39bf7bbfb01a4c03da03d032e823594, R#\Interpreter\Syntax\SyntaxTree\BinaryExpressionTree\NameMemberReference.vb"

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
    '         Function: expression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Interpreter.SyntaxParser

    Friend Class NameMemberReferenceProcessor : Inherits GenericSymbolOperatorProcessor

        Sub New()
            Call MyBase.New("$")
        End Sub

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim nameSymbol As String

            If a.VA.isException Then
                Return a
            ElseIf b.VA.isException Then
                Return b
            End If

            Dim typeofName As Type = b.VA.expression.GetType

            If typeofName Is GetType(SymbolReference) Then
                nameSymbol = DirectCast(b.VA.expression, SymbolReference).symbol
            ElseIf typeofName Is GetType(Literal) Then
                nameSymbol = DirectCast(b.VA.expression, Literal).value
            ElseIf typeofName Is GetType(FunctionInvoke) Then
                Dim invoke As FunctionInvoke = b.VA.expression
                Dim funcVar As New SymbolIndexer(a.VA.expression, invoke.funcName)

                Return New SyntaxResult(New FunctionInvoke(funcVar, invoke.parameters.ToArray))
            Else
                Return New SyntaxResult(New NotImplementedException, opts.debug)
            End If

            ' a$b symbol reference
            Dim symbolRef As New SymbolIndexer(a.VA.expression, New Literal(nameSymbol))
            Return New SyntaxResult(symbolRef)
        End Function
    End Class
End Namespace
