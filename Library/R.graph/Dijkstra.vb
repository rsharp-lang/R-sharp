#Region "Microsoft.VisualBasic::004295d7ccb1e61074f84816dc0b900d, Library\R.graph\Dijkstra.vb"

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

    ' Module Dijkstra
    ' 
    '     Function: CreateRouter, DijkstraRoutine
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphTheory.Dijkstra
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("igraph.dijkstra")>
Module Dijkstra

    <ExportAPI("router.dijkstra")>
    Public Function CreateRouter(g As NetworkGraph, Optional undirected As Boolean = False) As DijkstraRouter
        Return DijkstraRouter.FromNetwork(g, undirected)
    End Function

    Public Function DijkstraRoutine()

    End Function
End Module

