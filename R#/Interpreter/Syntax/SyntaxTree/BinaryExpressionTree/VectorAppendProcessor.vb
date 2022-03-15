#Region "Microsoft.VisualBasic::39d121e29e4fca465ba1de21e96628cf, R-sharp\R#\Interpreter\Syntax\SyntaxTree\BinaryExpressionTree\VectorAppendProcessor.vb"

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

        Total Lines:   27
        Code Lines:    22
        Comment Lines: 0
        Blank Lines:   5
        File Size:     976.00 B


    '     Class VectorAppendProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: expression, view
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Interpreter.SyntaxParser

    Friend Class VectorAppendProcessor : Inherits GenericSymbolOperatorProcessor

        Public Sub New()
            MyBase.New("<<")
        End Sub

        Protected Overrides Function view() As String
            Return "vector << element"
        End Function

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
            If a.VA.isException Then
                Return a
            ElseIf b.VA.isException Then
                Return b
            Else
                Return New SyntaxResult(New AppendOperator(a.VA.expression, b.VA.expression))
            End If
        End Function
    End Class
End Namespace
