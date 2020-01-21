#Region "Microsoft.VisualBasic::5a24ccd0a23fd575dfa16d8021cceea1, R#\Interpreter\Syntax\SyntaxTree\BinaryExpressionTree\GenericSymbolOperatorProcessor.vb"

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

    '     Class GenericSymbolOperatorProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: JoinBinaryExpression
    ' 
    '     Class SyntaxQueue
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Interpreter.SyntaxParser

    Friend MustInherit Class GenericSymbolOperatorProcessor

        ReadOnly operatorSymbol As String

        Sub New(opSymbol As String)
            operatorSymbol = opSymbol
        End Sub

        Protected MustOverride Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult

        Public Function JoinBinaryExpression(queue As SyntaxQueue, oplist As List(Of String), opts As SyntaxBuilderOptions) As SyntaxQueue
            If queue.buf = 1 Then
                Return queue
            End If

            Dim nop As Integer = oplist _
                .AsEnumerable _
                .Count(Function(op) op = operatorSymbol)
            Dim buf = queue.buf

            ' 从左往右计算
            For i As Integer = 0 To nop - 1
                For j As Integer = 0 To buf.Count - 1
                    If buf(j) Like GetType(String) AndAlso operatorSymbol = buf(j).VB Then
                        ' j-1 and j+1
                        Dim a = buf(j - 1) ' parameter
                        Dim b = buf(j + 1) ' function invoke
                        Dim exp As SyntaxResult = expression(a, b, opts)

                        Call buf.RemoveRange(j - 1, 3)
                        Call buf.Insert(j - 1, exp)

                        Exit For
                    End If
                Next
            Next

            Return queue
        End Function

    End Class

    Friend Class SyntaxQueue

        Public buf As List(Of [Variant](Of SyntaxResult, String))

    End Class
End Namespace
