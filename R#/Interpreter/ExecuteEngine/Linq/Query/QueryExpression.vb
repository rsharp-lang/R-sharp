#Region "Microsoft.VisualBasic::2762d0cc78708eeb25c5242041de51d0, R#\Interpreter\ExecuteEngine\Linq\Query\QueryExpression.vb"

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

    '     Class QueryExpression
    ' 
    '         Properties: Previews
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetDataSet, GetSeqValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace Interpreter.ExecuteEngine.LINQ

    Public MustInherit Class QueryExpression : Inherits Expression
        Implements IVisualStudioPreviews

        Protected ReadOnly sequence As Expression

        Protected Friend ReadOnly symbol As SymbolDeclare
        Protected Friend ReadOnly executeQueue As Expression()

        Protected Friend dataset As DataSet

        Sub New(symbol As SymbolDeclare, sequence As Expression, execQueue As IEnumerable(Of Expression))
            Me.symbol = symbol
            Me.sequence = sequence
            Me.executeQueue = execQueue.ToArray
        End Sub

        Public ReadOnly Property Previews As String Implements IVisualStudioPreviews.Previews
            Get
                Return ToString()
            End Get
        End Property

        ''' <summary>
        ''' get sequence value
        ''' 
        ''' evaluate expression for get ``IN ...`` data source
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Public Function GetSeqValue(context As ExecutableContext) As Object
            Return sequence.Exec(context)
        End Function

        ''' <summary>
        ''' get data source iterator for query
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Protected Function GetDataSet(context As ExecutableContext) As DataSet
            Return DataSet.CreateDataSet(Me, context)
        End Function

        ''' <summary>
        ''' do optimization of the current query expression.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function Compile() As QueryExpression
            Return Me
        End Function
    End Class
End Namespace
