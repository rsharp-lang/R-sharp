#Region "Microsoft.VisualBasic::f3b3c282a7a9668feb82f5f51e95727d, R-sharp\R#\Interpreter\ExecuteEngine\Linq\DataSet\DataFrameDataSet.vb"

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

        Total Lines:   29
        Code Lines:    15
        Comment Lines: 7
        Blank Lines:   7
        File Size:     875.00 B


    '     Class DataFrameDataSet
    ' 
    '         Properties: dataframe
    ' 
    '         Function: PopulatesData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' populate by rows
    ''' </summary>
    Public Class DataFrameDataSet : Inherits DataSet

        Public Property dataframe As dataframe

        ''' <summary>
        ''' populate a list of javascript object
        ''' </summary>
        ''' <returns></returns>
        Friend Overrides Iterator Function PopulatesData() As IEnumerable(Of Object)
            Dim nrows As Integer = dataframe.nrows

            For i As Integer = 0 To nrows - 1
                Dim list As Dictionary(Of String, Object) = dataframe.getRowList(i, drop:=True)
                Dim js As New JavaScriptObject(list)

                Yield js
            Next
        End Function
    End Class

End Namespace
