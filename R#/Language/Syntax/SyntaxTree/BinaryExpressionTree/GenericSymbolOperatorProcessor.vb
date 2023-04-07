#Region "Microsoft.VisualBasic::23280baac6b6c92fe2c254e3ddf08ae7, E:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxTree/BinaryExpressionTree/GenericSymbolOperatorProcessor.vb"

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

    '   Total Lines: 75
    '    Code Lines: 55
    ' Comment Lines: 2
    '   Blank Lines: 18
    '     File Size: 2.57 KB


    '     Class GenericSymbolOperatorProcessor
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: JoinBinaryExpression, ToString
    ' 
    '     Class SyntaxQueue
    ' 
    '         Properties: Count
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Language.Syntax.SyntaxParser

    Friend MustInherit Class GenericSymbolOperatorProcessor

        ReadOnly operatorSymbol As Index(Of String)

        Sub New(opSymbol As String)
            operatorSymbol = New String() {opSymbol}
        End Sub

        Sub New(ParamArray opSymbol As String())
            operatorSymbol = opSymbol.Indexing
        End Sub

        Protected MustOverride Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
        Protected MustOverride Function view() As String

        Public Overrides Function ToString() As String
            Return view()
        End Function

        Public Function JoinBinaryExpression(queue As SyntaxQueue, oplist As List(Of String), opts As SyntaxBuilderOptions) As SyntaxResult
            If queue.buf = 1 Then
                Return Nothing
            End If

            Dim nop As Integer = oplist _
                .AsEnumerable _
                .Count(Function(op)
                           Return op Like operatorSymbol
                       End Function)
            Dim buf As List(Of [Variant](Of SyntaxResult, String)) = queue.buf

            ' 从左往右计算
            For i As Integer = 0 To nop - 1
                For j As Integer = 0 To buf.Count - 1
                    If buf(j) Like GetType(String) AndAlso buf(j).VB Like operatorSymbol Then
                        ' j-1 and j+1
                        Dim a = buf(j - 1) ' parameter
                        Dim b = buf(j + 1) ' function invoke
                        Dim exp As SyntaxResult = expression(a, b, opts)

                        If exp.isException Then
                            Return exp
                        Else
                            Call buf.RemoveRange(j - 1, 3)
                            Call buf.Insert(j - 1, exp)
                        End If

                        Exit For
                    End If
                Next
            Next

            Return Nothing
        End Function

    End Class

    Friend Class SyntaxQueue

        Public buf As List(Of [Variant](Of SyntaxResult, String))

        Public ReadOnly Property Count As Integer
            Get
                Return buf.Count
            End Get
        End Property

    End Class
End Namespace
