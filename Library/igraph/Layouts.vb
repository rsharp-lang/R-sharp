#Region "Microsoft.VisualBasic::5c0bb834cbff1b4fa2e2ddf4fa2352d4, Library\igraph\Layouts.vb"

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

    '   Total Lines: 245
    '    Code Lines: 193 (78.78%)
    ' Comment Lines: 29 (11.84%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (9.39%)
    '     File Size: 10.62 KB


    ' Module Layouts
    ' 
    '     Function: (+2 Overloads) forceDirected, orthogonalLayout, randomLayout, SpringForce
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.ForceDirected
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting

''' <summary>
''' Do network layouts
''' </summary>
<Package("layouts", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
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

    <ExportAPI("layout.circular_force")>
    Public Function forceDirected(g As NetworkGraph,
                                  Optional ejectFactor As Integer = 6,
                                  Optional condenseFactor As Integer = 3,
                                  Optional maxtx As Integer = 4,
                                  Optional maxty As Integer = 3,
                                  <RRawVectorArgument> Optional dist As Object = "30,250",
                                  <RRawVectorArgument> Optional size As Object = "1000,1000",
                                  Optional iterations As Integer = 200,
                                  Optional env As Environment = Nothing) As NetworkGraph
        If g.CheckZero Then
            env.AddMessage("all of the vertex node in your network graph is in ZERO location, do random layout at first...", MSG_TYPES.WRN)
            g = g.doRandomLayout
        End If

        Dim sizeStr = InteropArgumentHelper.getSize(size, env, "10000,10000")
        Dim distStr = InteropArgumentHelper.getSize(dist, env, "30,256")
        Dim physics As Planner = New CircularPlanner(
            g:=g,
            ejectFactor:=ejectFactor,
            condenseFactor:=condenseFactor,
            maxtx:=maxtx,
            maxty:=maxty,
            dist_threshold:=distStr,
            size:=sizeStr
        )

        For i As Integer = 0 To iterations
            Call physics.Collide()

            If (100 * i / iterations) Mod 5 = 0 Then
                Console.WriteLine($"- Completed {i + 1} of {iterations} [{CInt(100 * i / iterations)}%]")
            End If
        Next

        Return g
    End Function

    ''' <summary>
    ''' Do force directed layout
    ''' </summary>
    ''' <param name="g">A network graph object.</param>
    ''' <param name="iterations">The number of layout iterations.</param>
    ''' <returns></returns>
    <ExportAPI("layout.force_directed")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function forceDirected(g As NetworkGraph,
                                  Optional ejectFactor As Integer = 6,
                                  Optional condenseFactor As Integer = 3,
                                  Optional maxtx As Integer = 4,
                                  Optional maxty As Integer = 3,
                                  <RRawVectorArgument> Optional dist As Object = "30,250",
                                  <RRawVectorArgument> Optional size As Object = "1000,1000",
                                  Optional iterations As Integer = 200,
                                  <RRawVectorArgument(GetType(String))>
                                  Optional algorithm As Object = "force_directed|degree_weighted|group_weighted|edge_weighted",
                                  Optional groupAttraction As Double = 5,
                                  Optional groupRepulsive As Double = 5,
                                  Optional weightedFactor As Double = 8,
                                  Optional avoids As RectangleF() = Nothing,
                                  Optional env As Environment = Nothing) As Object
        If g.CheckZero Then
            env.AddMessage("all of the vertex node in your network graph is in ZERO location, do random layout at first...", MSG_TYPES.WRN)
            g = g.doRandomLayout
        End If

        Dim sizeStr = InteropArgumentHelper.getSize(size, env, "10000,10000")
        Dim distStr = InteropArgumentHelper.getSize(dist, env, "30,256")
        Dim physics As Planner

        algorithm = CLRVector.asCharacter(algorithm).FirstOrDefault.Split("|"c).First

        Select Case any.ToString(algorithm)
            Case "force_directed"
                physics = New Planner(
                    g:=g,
                    ejectFactor:=ejectFactor,
                    condenseFactor:=condenseFactor,
                    maxtx:=maxtx,
                    maxty:=maxty,
                    dist_threshold:=distStr,
                    size:=sizeStr,
                    avoidRegions:=avoids
                )
            Case "degree_weighted"
                physics = New DegreeWeightedPlanner(
                    g:=g,
                    ejectFactor:=ejectFactor,
                    condenseFactor:=condenseFactor,
                    maxtx:=maxtx,
                    maxty:=maxty,
                    dist_threshold:=distStr,
                    size:=sizeStr,
                    avoidRegions:=avoids
                )
            Case "group_weighted"
                physics = New GroupPlanner(
                    g:=g,
                    ejectFactor:=ejectFactor,
                    condenseFactor:=condenseFactor,
                    maxtx:=maxtx,
                    maxty:=maxty,
                    dist_threshold:=distStr,
                    size:=sizeStr,
                    groupAttraction:=groupAttraction,
                    groupRepulsive:=groupRepulsive,
                    avoidRegions:=avoids
                )
            Case "edge_weighted"
                physics = New EdgeWeightedPlanner(
                    g:=g,
                    maxW:=weightedFactor,
                    ejectFactor:=ejectFactor,
                    condenseFactor:=condenseFactor,
                    maxtx:=maxtx,
                    maxty:=maxty,
                    dist_threshold:=distStr,
                    size:=sizeStr,
                    avoidRegions:=avoids
                )
            Case Else
                Return Internal.debug.stop($"invalid algorithm name: {algorithm}", env)
        End Select

        For i As Integer = 0 To iterations
            Call physics.Collide()

            If (100 * i / iterations) Mod 5 = 0 Then
                Call base.cat($"- Completed {i + 1} of {iterations} [{CInt(100 * i / iterations)}%]\n",,, env)
            End If
        Next

        Return g
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
    <ExportAPI("layout.springForce")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function SpringForce(g As NetworkGraph,
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
    ''' <returns></returns>
    <ExportAPI("layout.orthogonal")>
    Public Function orthogonalLayout(g As NetworkGraph,
                                     Optional num_of_attempts As Integer = 30,
                                     Optional optimize As Boolean = True,
                                     Optional simplify As Boolean = True,
                                     Optional fix_non_orthogonal As Boolean = True) As NetworkGraph

        Return Orthogonal.DoLayout(g, num_of_attempts, optimize, simplify, fix_non_orthogonal)
    End Function
End Module
