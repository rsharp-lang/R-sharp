#Region "Microsoft.VisualBasic::5773906fa65566bb64e84f2c1c49a894, Library\R.graph\NetworkModule.vb"

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

' Module NetworkModule
' 
'     Function: SaveNetwork
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports R.graphics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Interop
Imports node = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("igraph")>
Public Module NetworkModule

    <ExportAPI("save.network")>
    Public Function SaveNetwork(g As Object, file$, Optional properties As String() = Nothing) As Boolean
        If g Is Nothing Then
            Throw New ArgumentNullException("g")
        End If

        Dim tables As NetworkTables

        If g.GetType Is GetType(NetworkGraph) Then
            tables = DirectCast(g, NetworkGraph).Tabular(properties)
        ElseIf g.GetType Is GetType(NetworkTables) Then
            tables = g
        Else
            Throw New InvalidProgramException(g.GetType.FullName)
        End If

        Return tables.Save(file)
    End Function

    <ExportAPI("read.network")>
    Public Function LoadNetwork(directory$, Optional defaultNodeSize As Object = "20,20") As NetworkGraph
        Return NetworkFileIO.Load(directory).CreateGraph(defaultNodeSize:=InteropArgumentHelper.getSize(defaultNodeSize))
    End Function

    ''' <summary>
    ''' Create a new network graph or clear the given network graph
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("empty.network")>
    Public Function emptyNetwork(Optional g As NetworkGraph = Nothing) As NetworkGraph
        If g Is Nothing Then
            g = New NetworkGraph
        Else
            g.Clear()
        End If

        Return g
    End Function

    ''' <summary>
    ''' Calculate node degree in given graph
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("degree")>
    Public Function degree(g As NetworkGraph) As Dictionary(Of String, Integer)
        Return g.ComputeNodeDegrees
    End Function

    <ExportAPI("add.nodes")>
    Public Function addNodes(g As NetworkGraph, labels$()) As NetworkGraph
        For Each label As String In labels
            Call g.CreateNode(label)
        Next

        Return g
    End Function

    <ExportAPI("add.node")>
    Public Function addNode(g As NetworkGraph, label$,
                            <RListObjectArgument>
                            Optional attrs As Object = Nothing,
                            Optional env As Environment = Nothing) As node

        Dim node As node = g.CreateNode(label)

        For Each attribute As NamedValue(Of Object) In RListObjectArgumentAttribute.getObjectList(attrs, env)
            node.data.Add(attribute.Name, Scripting.ToString(attribute.Value))
        Next

        Return node
    End Function

    <ExportAPI("add.edge")>
    Public Function addEdge(g As NetworkGraph, u$, v$) As Edge
        Return g.CreateEdge(u, v)
    End Function

    ''' <summary>
    ''' Add edges by a given node label tuple list
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="tuples">a given node label tuple list</param>
    ''' <returns></returns>
    <ExportAPI("add.edges")>
    Public Function addEdges(g As NetworkGraph, tuples As Object) As NetworkGraph
        Dim nodeLabels As String()
        Dim edge As Edge

        For Each tuple As NamedValue(Of Object) In list.GetSlots(tuples).IterateNameValues
            nodeLabels = REnv.asVector(Of String)(tuple.Value)
            edge = g.CreateEdge(nodeLabels(0), nodeLabels(1))
            edge.ID = tuple.Name
        Next

        Return g
    End Function

    <ExportAPI("type_groups")>
    Public Function typeGroupOfNodes(g As NetworkGraph, type$, nodes As String()) As NetworkGraph
        Call nodes _
            .Select(AddressOf g.GetElementByID) _
            .DoEach(Sub(n)
                        n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = type
                    End Sub)
        Return g
    End Function

End Module
