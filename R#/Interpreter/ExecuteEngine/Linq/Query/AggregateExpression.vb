#Region "Microsoft.VisualBasic::c5f565d0fcf38a2a05e36014fc82d606, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/Query/AggregateExpression.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 705 B


    '     Class AggregateExpression
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Exec
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' aggregate ... into ...
    ''' </summary>
    Public Class AggregateExpression : Inherits QueryExpression

        Public Overrides ReadOnly Property name As String
            Get
                Return "aggregate ... into ..."
            End Get
        End Property

        Sub New(symbol As SymbolDeclare, sequence As Expression, exec As IEnumerable(Of Expression))
            Call MyBase.New(symbol, sequence, exec)
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
