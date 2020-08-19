#Region "Microsoft.VisualBasic::61fc734c0fe6105f14bcebb387af1f26, Library\R.graph\NetworkModule.vb"

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
'     Constructor: (+1 Overloads) Sub New
'     Function: addEdge, addEdges, addNode, addNodes, attributes
'               computeNetwork, connectedNetwork, DecomposeGraph, degree, emptyNetwork
'               getByGroup, getEdges, getElementByID, getNodes, LoadNetwork
'               printGraph, SaveNetwork, setAttributes, typeGroupOfNodes
' 
' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports node = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' package or create network graph and do network analysis.
''' </summary>
<Package("igraph", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
<RTypeExport("graph", GetType(NetworkGraph))>
Public Module NetworkModule

    Sub New()
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of NetworkGraph)(AddressOf printGraph)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of node)(AddressOf printNode)
    End Sub

    Private Function printGraph(obj As Object) As String
        Dim g As NetworkGraph = DirectCast(obj, NetworkGraph)

        Return $"Network graph with {g.vertex.Count} vertex nodes and {g.graphEdges.Count} edges."
    End Function

    Private Function printNode(node As node) As String
        Dim str As New StringBuilder

        Call str.AppendLine($"#{node.ID}  {node.label}")
        Call str.AppendLine($"degree: {node.degree.In} in, {node.degree.Out} out.")
        Call str.AppendLine(node.adjacencies?.ToString)

        If Not node.data Is Nothing Then
            If Not node.data.color Is Nothing Then
                Call str.AppendLine("color: " & node.data.color.ToString)
            End If

            Call str.AppendLine("class: " & (node.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) Or "n/a".AsDefault))
        End If

        Return str.ToString
    End Function

    ''' <summary>
    ''' save the network graph
    ''' </summary>
    ''' <param name="g">the network graph object or [nodes, edges] table data.</param>
    ''' <param name="file">a folder file path for save the network data.</param>
    ''' <param name="properties">a list of property name for save in node table and edge table.</param>
    ''' <returns></returns>
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

    ''' <summary>
    ''' load network graph object from a given file location
    ''' </summary>
    ''' <param name="directory">a directory which contains two data table: nodes and network edges</param>
    ''' <param name="defaultNodeSize">default node size in width and height</param>
    ''' <param name="defaultBrush">default brush texture descriptor string</param>
    ''' <returns></returns>
    <ExportAPI("read.network")>
    Public Function LoadNetwork(directory$, Optional defaultNodeSize As Object = "20,20", Optional defaultBrush$ = "black") As NetworkGraph
        Return NetworkFileIO.Load(directory.GetDirectoryFullPath) _
            .CreateGraph(
                defaultNodeSize:=InteropArgumentHelper.getSize(defaultNodeSize),
                defaultBrush:=defaultBrush
            )
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
    ''' removes duplicated edges in the network
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="directedGraph"></param>
    ''' <returns></returns>
    <ExportAPI("trim.edges")>
    Public Function trimEdges(g As NetworkGraph, Optional directedGraph As Boolean = False) As NetworkGraph
        Return g.RemoveDuplicated(directedGraph)
    End Function

    ''' <summary>
    ''' removes all of the isolated nodes.
    ''' </summary>
    ''' <param name="graph"></param>
    ''' <returns></returns>
    <ExportAPI("connected_graph")>
    Public Function connectedNetwork(graph As NetworkGraph) As NetworkGraph
        Dim g As New NetworkGraph

        For Each node As node In graph.connectedNodes
            Call g.CreateNode(node.label, node.data)
        Next

        For Each edge As Edge In graph.graphEdges
            Call g.CreateEdge(g.GetElementByID(edge.U.label), g.GetElementByID(edge.V.label), edge.weight, edge.data)
        Next

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

    ''' <summary>
    ''' compute network properties' data
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("compute.network")>
    Public Function computeNetwork(g As NetworkGraph) As NetworkGraph
        Call g.ComputeNodeDegrees
        Call g.ComputeBetweennessCentrality

        Return g
    End Function

    ''' <summary>
    ''' a nodes by given label list.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="labels">a character vector of the node labels</param>
    ''' <returns></returns>
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

    ''' <summary>
    ''' Set node attribute data
    ''' </summary>
    ''' <param name="nodes"></param>
    ''' <param name="attrs"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("attrs")>
    Public Function setAttributes(<RRawVectorArgument> nodes As Object,
                                  <RListObjectArgument> attrs As Object,
                                  Optional env As Environment = Nothing) As Object

        Dim attrValues As NamedValue(Of String)() = RListObjectArgumentAttribute _
            .getObjectList(attrs, env) _
            .Select(Function(a)
                        Return New NamedValue(Of String) With {
                            .Name = a.Name,
                            .Value = Scripting.ToString(a.Value)
                        }
                    End Function) _
            .ToArray

        For Each node As node In REnv.asVector(Of node)(nodes)
            For Each a In attrValues
                node.data(a.Name) = a.Value
            Next
        Next

        Return nodes
    End Function

    ''' <summary>
    ''' add edge link into the given network graph
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="u"></param>
    ''' <param name="v"></param>
    ''' <param name="weight"></param>
    ''' <returns></returns>
    <ExportAPI("add.edge")>
    Public Function addEdge(g As NetworkGraph, u$, v$, Optional weight# = 0) As Edge
        Return g.CreateEdge(u, v, weight)
    End Function

    ''' <summary>
    ''' Add edges by a given node label tuple list
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="tuples">a given node label tuple list</param>
    ''' <returns></returns>
    <ExportAPI("add.edges")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function addEdges(g As NetworkGraph, tuples As Object,
                             <RRawVectorArgument>
                             Optional weight As Object = Nothing,
                             Optional ignoreElementNotFound As Boolean = True,
                             Optional env As Environment = Nothing) As Object

        Dim nodeLabels As String()
        Dim edge As Edge
        Dim i As i32 = 1
        Dim weights As Double() = REnv.asVector(Of Double)(weight)
        Dim w As Double

        For Each tuple As NamedValue(Of Object) In list.GetSlots(tuples).IterateNameValues
            nodeLabels = REnv.asVector(Of String)(tuple.Value)
            w = weights.ElementAtOrDefault(CInt(i) - 1)

            If g.GetElementByID(nodeLabels(Scan0)) Is Nothing Then
                If ignoreElementNotFound Then
                    Call g.CreateNode(nodeLabels(Scan0))
                    Call env.AddMessage({"missing target node for create a new edge...", "missing node: " & nodeLabels(Scan0)}, MSG_TYPES.WRN)
                Else
                    Return Internal.debug.stop({"missing target node for create a new edge...", "missing node: " & nodeLabels(Scan0)}, env)
                End If
            End If
            If g.GetElementByID(nodeLabels(1)) Is Nothing Then
                If ignoreElementNotFound Then
                    Call g.CreateNode(nodeLabels(1))
                    Call env.AddMessage({"missing target node for create a new edge...", "missing node: " & nodeLabels(1)}, MSG_TYPES.WRN)
                Else
                    Return Internal.debug.stop({"missing target node for create a new edge...", "missing node: " & nodeLabels(1)}, env)
                End If
            End If

            edge = g.CreateEdge(nodeLabels(0), nodeLabels(1))
            edge.weight = w

            ' 20191226
            ' 如果使用数字作为边的编号的话
            ' 极有可能会出现重复的边编号
            ' 所以在这里判断一下
            ' 尽量避免使用数字作为编号
            If ++i = tuple.Name.ParseInteger Then
                edge.ID = $"{edge.U.label}..{edge.V.label}"
            Else
                edge.ID = tuple.Name
            End If
        Next

        Return g
    End Function

    ''' <summary>
    ''' get node elements by given id
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="id"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("getElementByID")>
    <RApiReturn(GetType(node))>
    Public Function getElementByID(g As NetworkGraph, id As Object, Optional env As Environment = Nothing) As Object
        Dim array As Array

        If id Is Nothing Then
            Return Nothing
        End If

        Dim idtype As Type = id.GetType

        If idtype Is GetType(Integer) Then
            Return g.GetElementByID(DirectCast(id, Integer))
        ElseIf idtype Is GetType(String) Then
            Return g.GetElementByID(DirectCast(id, String))
        ElseIf REnv.isVector(Of Integer)(id) Then
            array = REnv.asVector(Of Integer)(id) _
                .AsObjectEnumerator _
                .Select(Function(i)
                            Return g.GetElementByID(DirectCast(i, Integer))
                        End Function) _
                .ToArray
        ElseIf REnv.isVector(Of String)(id) Then
            array = REnv.asVector(Of String)(id) _
                .AsObjectEnumerator _
                .Select(Function(i)
                            Return g.GetElementByID(DirectCast(i, String))
                        End Function) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(String), id.GetType, env)
        End If

        Return array
    End Function

    ''' <summary>
    ''' Make node groups by given type name
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="type"></param>
    ''' <param name="nodes"></param>
    ''' <returns></returns>
    <ExportAPI("set_group")>
    Public Function typeGroupOfNodes(g As NetworkGraph, type$, nodes As String()) As NetworkGraph
        Call nodes _
            .Select(AddressOf g.GetElementByID) _
            .DoEach(Sub(n)
                        n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = type
                    End Sub)
        Return g
    End Function

    <ExportAPI("vertex")>
    Public Function getNodes(g As NetworkGraph) As node()
        Return g.vertex.ToArray
    End Function

    <ExportAPI("edges")>
    Public Function getEdges(g As NetworkGraph) As Edge()
        Return g.graphEdges.ToArray
    End Function

    <ExportAPI("attributes")>
    Public Function attributes(<RRawVectorArgument> elements As Object, name$,
                               <RByRefValueAssign, RRawVectorArgument>
                               Optional values As Object = Nothing,
                               Optional env As Environment = Nothing) As Object

        If elements Is Nothing Then
            If Not values Is Nothing Then
                Return Internal.debug.stop("target elements for set attribute values can not be nothing!", env)
            Else
                Return Nothing
            End If
        ElseIf TypeOf elements Is node() Then
            If values Is Nothing Then
                Return DirectCast(elements, node()).Select(Function(a) If(a.data(name), "")).ToArray
            Else
                Return Internal.debug.stop(New NotImplementedException, env)
            End If
        ElseIf TypeOf elements Is Edge() Then
            If values Is Nothing Then
                Return DirectCast(elements, Edge()).Select(Function(a) If(a.data(name), "")).ToArray
            Else
                Return Internal.debug.stop(New NotImplementedException, env)
            End If
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(node), elements.GetType, env,, NameOf(elements)), env)
        End If
    End Function

    ''' <summary>
    ''' Node select by group or other condition
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="typeSelector"></param>
    ''' <returns></returns>
    <ExportAPI("select")>
    <RApiReturn(GetType(node))>
    Public Function getByGroup(g As NetworkGraph, typeSelector As Object, Optional env As Environment = Nothing) As Object
        If typeSelector Is Nothing Then
            Return {}
        ElseIf typeSelector.GetType Is GetType(String) Then
            Dim typeStr$ = typeSelector.ToString

            Return g.vertex _
                .Where(Function(n)
                           Return n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = typeStr
                       End Function) _
                .ToArray
        ElseIf REnv.isVector(Of String)(typeSelector) Then
            Dim typeIndex As Index(Of String) = REnv _
                .asVector(Of String)(typeSelector) _
                .AsObjectEnumerator(Of String) _
                .ToArray

            Return g.vertex _
                .Where(Function(n)
                           Return n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) Like typeIndex
                       End Function) _
                .ToArray
        ElseIf typeSelector.GetType.ImplementInterface(GetType(RFunction)) Then
            Dim selector As RFunction = typeSelector

            Return g.vertex _
                .Where(Function(n)
                           Dim test As Object = selector.Invoke(env, InvokeParameter.CreateLiterals(n))
                           ' get test result
                           Return REnv _
                               .asLogical(test) _
                               .FirstOrDefault
                       End Function) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(RFunction), typeSelector.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' Decompose a graph into components, Creates a separate graph for each component of a graph.
    ''' </summary>
    ''' <param name="graph">The original graph.</param>
    ''' <param name="weakMode">
    ''' Character constant giving the type of the components, wither weak for weakly connected 
    ''' components or strong for strongly connected components.
    ''' </param>
    ''' <param name="minVertices">The minimum number of vertices a component should contain in 
    ''' order to place it in the result list. Eg. supply 2 here to ignore isolate vertices.
    ''' </param>
    ''' <returns>A list of graph objects.</returns>
    <ExportAPI("decompose")>
    Public Function DecomposeGraph(graph As NetworkGraph, Optional weakMode As Boolean = True, Optional minVertices As Integer = 5) As NetworkGraph()
        Return graph.DecomposeGraph(weakMode, minVertices).ToArray
    End Function
End Module
