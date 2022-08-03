#Region "Microsoft.VisualBasic::b95c1b49b78a1230917d0fc25ef3fd0b, R-sharp\Library\igraph\builder.vb"

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

    '   Total Lines: 29
    '    Code Lines: 18
    ' Comment Lines: 8
    '   Blank Lines: 3
    '     File Size: 1.19 KB


    ' Module builder
    ' 
    '     Function: FromCorrelations
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("igraph.builder")>
Module builder

    ''' <summary>
    ''' create a network graph based on the item correlations
    ''' </summary>
    ''' <param name="x">a correlation matrix</param>
    ''' <param name="threshold">the absolute threshold value of the correlation value</param>
    ''' <param name="pvalue"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("correlation.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function FromCorrelations(x As CorrelationMatrix,
                                     Optional threshold As Double = 0.65,
                                     Optional pvalue As Double = 1,
                                     Optional env As Environment = Nothing) As Object

        Return x.BuildNetwork(threshold, pvalue).Item1
    End Function
End Module
