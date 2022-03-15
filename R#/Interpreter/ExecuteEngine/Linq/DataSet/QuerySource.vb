#Region "Microsoft.VisualBasic::3930026d4419b5e392b116b35b3ea9b8, R-sharp\R#\Interpreter\ExecuteEngine\Linq\DataSet\QuerySource.vb"

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


     Code Statistics:

        Total Lines:   79
        Code Lines:    59
        Comment Lines: 4
        Blank Lines:   16
        File Size:     3.04 KB


    '     Class QuerySource
    ' 
    '         Properties: symbolName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumerateFields, (+2 Overloads) getSymbolName, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports any = Microsoft.VisualBasic.Scripting
Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression
Imports RLiteral = SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets.Literal
Imports RSymbolRef = SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets.SymbolReference

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class QuerySource

        Protected Friend ReadOnly sequence As Expression
        Protected Friend ReadOnly symbol As SymbolDeclare

        ''' <summary>
        ''' symbol name of the data source sequence
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property symbolName As String
            Get
                Return getSymbolName(sequence)
            End Get
        End Property

        Sub New(symbol As SymbolDeclare, sequence As Expression)
            Me.symbol = symbol
            Me.sequence = sequence
        End Sub

        Friend Shared Function getSymbolName(varX As Expression) As String
            Dim name As String = varX.ToString

            If TypeOf varX Is Literal Then
                name = any.ToString(DirectCast(varX, Literal).value)
            ElseIf TypeOf varX Is SymbolReference Then
                name = DirectCast(varX, SymbolReference).symbolName
            ElseIf TypeOf varX Is RunTimeValueExpression Then
                name = getSymbolName(DirectCast(varX, RunTimeValueExpression).R)
            End If

            Return name
        End Function

        Private Shared Function getSymbolName(varX As RExpression) As String
            Dim name As String = varX.ToString

            If TypeOf varX Is RLiteral Then
                name = DirectCast(varX, RLiteral).ValueStr
            ElseIf TypeOf varX Is RSymbolRef Then
                name = DirectCast(varX, RSymbolRef).symbol
            End If

            Return name
        End Function

        Public Iterator Function EnumerateFields(addSymbol As Boolean) As IEnumerable(Of NamedValue(Of Expression))
            If symbol.isTuple Then
                Dim sourceSymbol As String = getSymbolName(sequence)

                For Each varX In DirectCast(symbol.symbol, VectorLiteral).elements
                    Dim name As String = getSymbolName(varX)

                    If addSymbol Then
                        name = $"{sourceSymbol}.{name}"
                    End If

                    Yield New NamedValue(Of Expression)(name, varX)
                Next
            ElseIf addSymbol Then
                Yield New NamedValue(Of Expression)(any.ToString(symbol.symbol), symbol.symbol)
            Else
                Yield New NamedValue(Of Expression)("*", symbol.symbol)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"FROM {symbol} IN {sequence}"
        End Function
    End Class
End Namespace
