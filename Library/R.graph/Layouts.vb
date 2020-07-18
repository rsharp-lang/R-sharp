#Region "Microsoft.VisualBasic::9637ac74287de7d12687186d8b934d93, Library\R.graph\Layouts.vb"

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

    ' Module Layouts
    ' 
    '     Function: forceDirect, orthogonalLayout, randomLayout
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' Do network layouts
''' </summary>
<Package("igraph.layouts", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
Module Layouts

    ''' <summary>
    ''' do random layout of the given network graph object and then returns the given graph object.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("layout.random")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function randomLayout(g As NetworkGraph, Optional env As Environment = Nothing) As Object
        If g Is Nothing Then
            Return Internal.debug.stop("the given network graph object can not be nothing!", env)
        Else
            Return g.doRandomLayout
        End If
    End Function

    ''' <summary>
    ''' Do force directed layout
    ''' </summary>
    ''' <param name="g">A network graph object.</param>
    ''' <param name="iterations">The number of layout iterations.</param>
    ''' <param name="clearScreen">
    ''' Clear of the console screen when display the progress bar.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("layout.force_directed")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function forceDirect(g As NetworkGraph,
                                Optional stiffness# = 80,
                                Optional repulsion# = 4000,
                                Optional damping# = 0.83,
                                Optional iterations% = 1000,
                                Optional clearScreen As Boolean = False,
                                Optional showProgress As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        If g.CheckZero Then
            env.AddMessage("all of the vertex node in your network graph is in ZERO location, do random layout at first...", MSG_TYPES.WRN)
            g = g.doRandomLayout
        End If

        If Not showProgress Then
            Call "Do force directed layout...".__INFO_ECHO
        End If

        Call g.doForceLayout(
            showProgress:=showProgress,
            iterations:=iterations,
            clearScreen:=clearScreen,
            Stiffness:=stiffness,
            Repulsion:=repulsion,
            Damping:=damping
        )

        Dim allNan = g.vertex.All(Function(a) a.data.initialPostion.isNaN)

        If allNan Then
            Dim tooLarge As Boolean = g.graphEdges.Count / g.connectedNodes.Length > 10

            If tooLarge Then
                Return Internal.debug.stop({
                    "do network graph layout failure...",
                    "your network size is too large, please consider reduce the edge connection size...",
                    "size: [" & g.vertex.Count & ", " & g.graphEdges.Count & "]"
                }, env)
            Else
                Return Internal.debug.stop({
                    "do network graph layout failure...",
                    "please consider adjust one of the physics parameters: stiffness, repulsion or damping..."
                }, env)
            End If
        Else
            Return g
        End If
    End Function

    ''' <summary>
    ''' do orthogonal layout for the network graph input
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="gridSize"></param>
    ''' <param name="delta">the node movement delta</param>
    ''' <param name="layoutIteration">
    ''' the iteration number for run the layout process, ``-1`` means auto calculation.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("layout.orthogonal")>
    Public Function orthogonalLayout(g As NetworkGraph,
                                     <RRawVectorArgument>
                                     Optional gridSize As Object = "1000,1000",
                                     Optional delta# = 13,
                                     Optional layoutIteration% = -1) As NetworkGraph

        Dim size As Size = InteropArgumentHelper _
            .getSize(gridSize) _
            .SizeParser

        Call Orthogonal.DoLayout(g, size, delta, debug:=True, iterationCount:=layoutIteration)
        Call Orthogonal.DoEdgeLayout(g)

        Return g
    End Function
End Module
