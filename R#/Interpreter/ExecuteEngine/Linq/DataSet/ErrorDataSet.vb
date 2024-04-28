#Region "Microsoft.VisualBasic::28cbe159bc477067ccd997feefdda122, E:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/DataSet/ErrorDataSet.vb"

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

    '   Total Lines: 16
    '    Code Lines: 8
    ' Comment Lines: 4
    '   Blank Lines: 4
    '     File Size: 432 B


    '     Class ErrorDataSet
    ' 
    '         Properties: message
    ' 
    '         Function: PopulatesData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class ErrorDataSet : Inherits DataSet

        Public Property message As Message

        ''' <summary>
        ''' populate nothing
        ''' </summary>
        ''' <returns></returns>
        Friend Overrides Iterator Function PopulatesData() As IEnumerable(Of Object)
        End Function
    End Class
End Namespace
