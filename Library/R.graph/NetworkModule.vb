﻿#Region "Microsoft.VisualBasic::8224818fe21215525042046d4dab7b89, Library\R.graph\NetworkModule.vb"

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
'     Function: addEdge, addEdges, addNode, addNodes, attributes
'               components, computeNetwork, connectedNetwork, DecomposeGraph, degree
'               deleteNode, edgeAttributes, emptyNetwork, eval, extractAdjacenciesSubNetwork
'               getByGroup, getEdges, getElementByID, getNodes, LoadNetwork
'               metaData, nodeAttributes, nodeMass, nodeNames, printGraph
'               printNode, SaveNetwork, setAttributes, summaryNodes, trimEdges
'               typeGroupOfNodes, V
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports node = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node
Imports rDataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' package or create network graph and do network analysis.
''' </summary>
<Package("igraph", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
<RTypeExport("graph", GetType(NetworkGraph))>
Public Module NetworkModule

    Friend Sub Main()
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of NetworkGraph)(AddressOf printGraph)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of node)(AddressOf printNode)

        REnv.Internal.generic.add("summary", GetType(node()), AddressOf summaryNodes)
    End Sub

    Private Function summaryNodes(nodes As node(), args As list, env As Environment) As Object
        Dim summary As New StringBuilder

        For Each v As node In nodes
            Call summary.AppendLine($"#{v.ID}  {v.label}; degree: {v.degree.In} in, {v.degree.Out} out; class: {v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) Or "n/a".AsDefault}")
        Next

        Return summary.ToString
    End Function

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
    ''' get graph vertex collection
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("V")>
    <RApiReturn(GetType(V))>
    Public Function V(g As NetworkGraph) As Object
        Return New V(g)
    End Function

    ''' <summary>
    ''' extract sub-network from a given network via a specific network node as centroid. 
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="fromPoint"></param>
    ''' <returns></returns>
    <ExportAPI("subgraphFromPoint")>
    Public Function extractAdjacenciesSubNetwork(g As NetworkGraph, fromPoint As String) As NetworkGraph
        Dim target As node = g.GetElementByID(fromPoint)
        Dim connected As node() = target.adjacencies _
            .EnumerateAllEdges _
            .Select(Function(e) e.Iterate2Nodes) _
            .IteratesALL _
            .Where(Function(d) Not d Is target) _
            .GroupBy(Function(d) d.label) _
            .Select(Function(d) d.First) _
            .ToArray
        Dim subnet As New NetworkGraph

        Call subnet.CreateNode(target.label, target.data)

        For Each V As node In connected
            Call subnet.CreateNode(V.label, V.data)
        Next

        For Each directLink As Edge In target.adjacencies.EnumerateAllEdges
            Call subnet.CreateEdge(
                subnet.GetElementByID(directLink.U.label),
                subnet.GetElementByID(directLink.V.label),
                directLink.weight,
                directLink.data
            )
        Next

        ' add edges between the connected nodes
        Dim duplicated As New Index(Of String)

        For Each x As node In connected
            For Each y As node In connected
                For Each link As Edge In g.GetEdges(x, y)
                    Dim key As String = {link.U.label, link.V.label}.OrderBy(Function(str) str).JoinBy("+")

                    If key Like duplicated Then
                        Continue For
                    Else
                        Call duplicated.Add(key)
                        Call subnet.CreateEdge(
                            subnet.GetElementByID(link.U.label),
                            subnet.GetElementByID(link.V.label),
                            link.weight,
                            link.data
                        )
                    End If
                Next
            Next
        Next

        Return subnet
    End Function

    ''' <summary>
    ''' create meta data for network tabular model.
    ''' </summary>
    ''' <param name="title"></param>
    ''' <param name="description"></param>
    ''' <param name="creators"></param>
    ''' <param name="create_time"></param>
    ''' <param name="links"></param>
    ''' <param name="keywords"></param>
    ''' <param name="meta"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("metadata")>
    Public Function metaData(title As String,
                             Optional description As String = "n/a",
                             Optional creators As String() = Nothing,
                             Optional create_time As String = Nothing,
                             Optional links As String() = Nothing,
                             Optional keywords As String() = Nothing,
                             <RListObjectArgument>
                             Optional meta As list = Nothing,
                             Optional env As Environment = Nothing) As MetaData

        Dim metalist As Dictionary(Of String, String) = meta.AsGeneric(Of String)(env)
        Dim data As New MetaData With {
            .additionals = metalist,
            .create_time = create_time,
            .creators = creators,
            .description = description,
            .keywords = keywords,
            .links = links,
            .title = title
        }

        Return data
    End Function

    ''' <summary>
    ''' save the network graph
    ''' </summary>
    ''' <param name="g">the network graph object or [nodes, edges] table data.</param>
    ''' <param name="file">a folder file path for save the network data.</param>
    ''' <param name="properties">a list of property name for save in node table and edge table.</param>
    ''' <returns></returns>
    <ExportAPI("save.network")>
    <RApiReturn(GetType(Boolean))>
    Public Function SaveNetwork(g As Object, file$,
                                Optional properties As String() = Nothing,
                                Optional meta As MetaData = Nothing,
                                Optional env As Environment = Nothing) As Object

        Dim tables As NetworkTables

        If g Is Nothing Then
            Return Internal.debug.stop("the required network graph object can not be nothing!", env)
        End If

        If g.GetType Is GetType(NetworkGraph) Then
            tables = DirectCast(g, NetworkGraph).Tabular(properties, meta:=meta)
        ElseIf g.GetType Is GetType(NetworkTables) Then
            tables = g

            If Not meta Is Nothing Then
                tables.meta = meta
            End If
        Else
            Return Internal.debug.stop(New InvalidProgramException(g.GetType.FullName), env)
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
    Public Function LoadNetwork(directory$,
                                <RRawVectorArgument>
                                Optional defaultNodeSize As Object = "20,20",
                                Optional defaultBrush$ = "black",
                                Optional ignoresBrokenLinks As Boolean = False,
                                Optional env As Environment = Nothing) As NetworkGraph

        Return NetworkFileIO.Load(directory.GetDirectoryFullPath) _
            .CreateGraph(
                defaultNodeSize:=InteropArgumentHelper.getSize(defaultNodeSize, env),
                defaultBrush:=defaultBrush,
                ignoresBrokenLinks:=ignoresBrokenLinks
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
    Public Function trimEdges(g As NetworkGraph,
                              Optional directedGraph As Boolean = False,
                              Optional removesTuples As Boolean = False) As NetworkGraph

        g = g.RemoveDuplicated(directedGraph)

        If removesTuples Then
            Dim index = New GraphIndex(Of node, Edge)().nodes(g.vertex).edges(g.graphEdges)
            Dim tuples = g.graphEdges _
                .Where(Function(e) e.isTupleEdge(index)) _
                .ToArray

            For Each edge As Edge In tuples
                Call g.DetachNode(edge.U)
                Call g.DetachNode(edge.V)
            Next
        End If

        Return g
    End Function

    ''' <summary>
    ''' removes all of the isolated nodes.
    ''' </summary>
    ''' <param name="graph"></param>
    ''' <returns></returns>
    <ExportAPI("connected_graph")>
    Public Function connectedNetwork(graph As NetworkGraph) As NetworkGraph
        Return graph.GetConnectedGraph
    End Function

    ''' <summary>
    ''' set node data names
    ''' </summary>
    ''' <param name="graph"></param>
    ''' <param name="setNames">
    ''' a list object with node id to node name mapping. if the given name label in this 
    ''' list object is null or empty, then it will removes the name label value of the 
    ''' specific node object.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("node.names")>
    Public Function nodeNames(graph As NetworkGraph,
                              <RByRefValueAssign>
                              Optional setNames As list = Nothing,
                              Optional env As Environment = Nothing) As Object

        If setNames Is Nothing Then
            Return graph.vertex _
                .Select(Function(a) a.data.label) _
                .ToArray
        Else
            Dim names As New List(Of String)
            Dim sets As Dictionary(Of String, String) = setNames.AsGeneric(Of String)(env)

            For Each node As node In graph.vertex
                If sets.ContainsKey(node.label) Then
                    node.data.label = sets(node.label)
                    node.data.origID = sets(node.label)
                End If

                Call names.Add(node.data.label)
            Next

            Return names.ToArray
        End If
    End Function

    ''' <summary>
    ''' Calculate node degree in given graph
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("degree")>
    Public Function degree(g As NetworkGraph) As list
        Return New list With {
            .slots = g _
                .ComputeNodeDegrees _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return CObj(a.Value)
                              End Function)
        }
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
    ''' evaluate node/edge property values
    ''' </summary>
    ''' <param name="elements">a node collection or edge collection</param>
    ''' <param name="formula"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("eval")>
    Public Function eval(<RRawVectorArgument> elements As Object, formula As Expression, Optional env As Environment = Nothing) As Object
        Dim result As New Dictionary(Of String, Object)
        Dim var As Symbol
        Dim value As Object
        Dim data As GraphData
        Dim label As String

        If TypeOf elements Is V Then
            elements = DirectCast(elements, V).vertex
        End If

        Using closure As New Environment(env, env.stackFrame, isInherits:=False)
            For Each v As Object In elements
                If v Is Nothing Then
                    Continue For
                ElseIf TypeOf v Is node Then
                    data = DirectCast(v, node).data
                    label = DirectCast(v, node).label
                ElseIf TypeOf v Is Edge Then
                    data = DirectCast(v, Edge).data
                    label = DirectCast(v, Edge).ID
                Else
                    Return Message.InCompatibleType(GetType(node), v.GetType, env)
                End If

                For Each symbol In data.Properties
                    var = closure.FindSymbol(symbol.Key, [inherits]:=False)
                    value = DataImports.ParseVector({symbol.Value})

                    If var Is Nothing Then
                        closure.Push(symbol.Key, value, [readonly]:=False)
                    Else
                        var.SetValue(value, env)
                    End If
                Next

                value = formula.Evaluate(closure)

                If Program.isException(value) Then
                    Return value
                Else
                    result(v.label) = value
                End If
            Next
        End Using

        Return New list With {
            .slots = result
        }
    End Function

    <ExportAPI("delete")>
    Public Function deleteNode(g As NetworkGraph, node As Object, Optional env As Environment = Nothing) As Object
        Dim v As node

        If node Is Nothing Then
            Return g
        ElseIf TypeOf node Is String Then
            v = g.GetElementByID(DirectCast(node, String))

            If v Is Nothing Then
                Return Internal.debug.stop({$"we are not able to found a node with label id: {node}!", $"label: {node}"}, env)
            End If
        ElseIf TypeOf node Is node Then
            v = node
        Else
            Return Message.InCompatibleType(GetType(node), node.GetType, env)
        End If

        Call g.RemoveNode(v)

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

        For Each attr As NamedValue(Of Object) In RListObjectArgumentAttribute.getObjectList(attrs, env)
            Select Case attr.Name
                Case "label"
                    node.data.label = any.ToString(attr.Value)
                Case "group"
                    node.data.Add(NamesOf.REFLECTION_ID_MAPPING_NODETYPE, any.ToString(attr.Value))
                Case Else
                    Call node.data.Add(attr.Name, any.ToString(attr.Value))
            End Select
        Next

        Return node
    End Function

    ''' <summary>
    ''' set or get mass value of the nodes in the given graph
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="mass"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("mass")>
    Public Function nodeMass(g As NetworkGraph,
                             Optional labelID As String = Nothing,
                             <RByRefValueAssign>
                             Optional mass As Object = Nothing,
                             Optional env As Environment = Nothing) As Object

        If Not mass Is Nothing Then
            If TypeOf mass Is list Then
                Dim massList As list = DirectCast(mass, list)

                For Each key As String In massList.slots.Keys
                    If Not g.GetElementByID(key) Is Nothing Then
                        g.GetElementByID(key).data.mass = massList.getValue(key, env, 0.0)
                    End If
                Next
            ElseIf Not labelID.StringEmpty Then
                g.GetElementByID(labelID).data.mass = REnv _
                    .asVector(Of Double)(mass) _
                    .AsObjectEnumerator(Of Double) _
                    .DefaultFirst

                Return g.GetElementByID(labelID).data.mass
            End If
        ElseIf Not labelID.StringEmpty Then
            Return g.GetElementByID(labelID).data.mass
        End If

        Return New list With {
            .slots = g.vertex _
                .ToDictionary(Function(v) v.label,
                              Function(v)
                                  Return CObj(v.data.mass)
                              End Function)
        }
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
                            .Value = any.ToString(a.Value)
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

    <ExportAPI("weight")>
    Public Function weight(g As NetworkGraph, u As String, v As String, Optional setWeight As Double = 0, Optional directed As Boolean = False) As Double()
        Dim edges As Edge()

        If directed Then
            edges = g.GetEdges(g.GetElementByID(u), g.GetElementByID(v)).SafeQuery.ToArray
        Else
            edges = g.GetEdges(g.GetElementByID(u), g.GetElementByID(v)).SafeQuery.AsList + g.GetEdges(g.GetElementByID(v), g.GetElementByID(u))
        End If

        If setWeight <> 0 Then
            For Each edge In edges
                edge.weight = setWeight
            Next
        End If

        Return edges.Select(Function(e) e.weight).ToArray
    End Function

    ''' <summary>
    ''' Add edges by a given node label tuple list
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="data">a given node label tuple list, 
    ''' this parameter should be a list of edge names, in 
    ''' format looks like ``list(tag = [from, to], ...)``.
    ''' </param>
    ''' <param name="weight">
    ''' the edge weights vector
    ''' </param>
    ''' <param name="type">
    ''' the edge types vector
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("pushEdges")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function addEdges(g As NetworkGraph, data As Object,
                             <RRawVectorArgument>
                             Optional weight As Object = Nothing,
                             <RRawVectorArgument>
                             Optional type As Object = Nothing,
                             Optional ignoreElementNotFound As Boolean = True,
                             Optional env As Environment = Nothing) As Object

        Dim nodeLabels As String()
        Dim edge As Edge
        Dim i As i32 = 1
        Dim weights As Double() = REnv.asVector(Of Double)(weight)
        Dim types As String() = REnv.asVector(Of String)(type)
        Dim w As Double
        Dim err As New Value(Of Message)
        Dim checkNode As Func(Of String, Message) =
            Function(id)
                If g.GetElementByID(id) Is Nothing Then
                    If ignoreElementNotFound Then
                        Call g.CreateNode(id)
                        Call env.AddMessage({
                            "missing target node for create a new edge...",
                            "missing node: " & id
                        }, MSG_TYPES.WRN)
                    Else
                        Return Internal.debug.stop({
                            "missing target node for create a new edge...",
                            "missing node: " & id
                        }, env)
                    End If
                End If

                Return Nothing
            End Function
        Dim tuplesData As IEnumerable(Of NamedValue(Of Object))

        If TypeOf data Is list Then
            tuplesData = list.GetSlots(data).IterateNameValues
        ElseIf TypeOf data Is FormulaExpression Then
            Dim formula As FormulaExpression = DirectCast(data, FormulaExpression)

            data = env.FindSymbol(formula.var)

            If data Is Nothing Then
                Return Message.SymbolNotFound(env, formula.var, TypeCodes.generic)
            Else
                data = DirectCast(data, Symbol).value
            End If

            If data Is Nothing Then
                Return Internal.debug.stop({$"the required object '{formula.var}' can not be nothing!", $"object: {formula.var}"}, env)
            End If

            Dim table As rDataframe = DirectCast(data, rDataframe)
            Dim from As String = DirectCast(DirectCast(formula.formula, BinaryExpression).left, SymbolReference).symbol
            Dim [to] As String = DirectCast(DirectCast(formula.formula, BinaryExpression).right, SymbolReference).symbol

            tuplesData = (Iterator Function() As IEnumerable(Of NamedValue(Of Object))
                              For Each row In DirectCast(data, rDataframe).forEachRow({from, [to]})
                                  Yield New NamedValue(Of Object)(row.name, row.value)
                              Next
                          End Function)()

        ElseIf TypeOf data Is rDataframe Then
            tuplesData = (Iterator Function() As IEnumerable(Of NamedValue(Of Object))
                              For Each row In DirectCast(data, rDataframe).forEachRow({"from", "to"})
                                  Yield New NamedValue(Of Object)(row.name, row.value)
                              Next
                          End Function)()
        Else
            Throw New NotImplementedException
        End If

        ' list(tag = [from, to])
        For Each tuple As NamedValue(Of Object) In tuplesData
            nodeLabels = REnv.asVector(Of String)(tuple.Value)
            w = weights.ElementAtOrDefault(CInt(i) - 1)
            type = types.ElementAtOrDefault(CInt(i) - 1)

            If Not err = checkNode(nodeLabels(Scan0)) Is Nothing Then
                Return CType(err, Message)
            ElseIf Not err = checkNode(nodeLabels(1)) Is Nothing Then
                Return CType(err, Message)
            Else
                edge = g.CreateEdge(nodeLabels(0), nodeLabels(1))
                edge.weight = w

                If Not type Is Nothing Then
                    edge.data(NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE) = type
                End If
            End If

            ' 20191226
            ' 如果使用数字作为边的编号的话
            ' 极有可能会出现重复的边编号
            ' 所以在这里判断一下
            ' 尽量避免使用数字作为编号
            If ++i = tuple.Name.ParseInteger OrElse tuple.Name.IsPattern("(\[\[\d+\]\])|(\[\d+,\s*\])") Then
                ' fix for list name or dataframe row name
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
    <ExportAPI("group")>
    Public Function typeGroupOfNodes(g As NetworkGraph, nodes As String(), <RByRefValueAssign> type$) As NetworkGraph
        Call nodes _
            .Select(AddressOf g.GetElementByID) _
            .DoEach(Sub(n)
                        ' 如果执行了connected_graph函数
                        ' 则有些节点会消失
                        ' GetElementByID返回的是空值
                        ' 跳过这些空值
                        If Not n Is Nothing Then
                            n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = type
                        End If
                    End Sub)
        Return g
    End Function

    <ExportAPI("class")>
    Public Function nodeClass(g As NetworkGraph, <RByRefValueAssign> Optional classList As String() = Nothing) As String()
        If classList.IsNullOrEmpty Then
            Return g.vertex.Select(Function(v) v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)).ToArray
        Else
            Dim nodes = g.vertex.ToArray

            For i As Integer = 0 To nodes.Length - 1
                nodes(i).data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = classList(i)
            Next

            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' get all nodes in the given graph model
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("vertex")>
    Public Function getNodes(g As NetworkGraph, Optional allConnected As Boolean = False) As node()
        If allConnected Then
            Return g.connectedNodes
        Else
            Return g.vertex.ToArray
        End If
    End Function

    ''' <summary>
    ''' get all edges in the given graph model
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("edges")>
    Public Function getEdges(g As NetworkGraph) As Edge()
        Return g.graphEdges.ToArray
    End Function

    <ExportAPI("has.edge")>
    Public Function hasEdge(g As NetworkGraph, u As String, v As String, Optional directed As Boolean = False) As Boolean
        If directed Then
            Return g.GetEdges(g.GetElementByID(u), g.GetElementByID(v)).Any
        Else
            Return g.GetEdges(g.GetElementByID(u), g.GetElementByID(v)).Any OrElse g.GetEdges(g.GetElementByID(v), g.GetElementByID(u)).Any
        End If
    End Function

    <Extension>
    Private Function nodeAttributes(elements As node(), name$, values As Object, env As Environment) As Object
        If values Is Nothing Then
            Return elements.GetNodeAttributes(name)
        ElseIf TypeOf values Is list Then
            Return elements.SetNodeAttributesInList(name, DirectCast(values, list))
        Else
            Return elements.SetNodeAttributeInVector(name, values)
        End If
    End Function

    <Extension>
    Private Function edgeAttributes(elements As Edge(), name$, values As Object, env As Environment) As Object
        If values Is Nothing Then
            ' get edge attribute vector
            Return elements.Select(Function(a) If(a.data(name), "")).ToArray
        ElseIf TypeOf values Is list Then
            Return elements.SetEdgeAttributesInList(name, values)

            ' set edge attribute vector
        ElseIf TypeOf values Is vector Then
            Return Internal.debug.stop(New NotImplementedException, env)
        Else
            Return Internal.debug.stop(New NotImplementedException, env)
        End If
    End Function

    ''' <summary>
    ''' get or set element attribute values
    ''' </summary>
    ''' <param name="elements"></param>
    ''' <param name="name"></param>
    ''' <param name="values"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' dash style of the edge element could be:
    ''' solid, dash, dot, dashdot, dashdotdot
    ''' </remarks>
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
            Return DirectCast(elements, node()).nodeAttributes(name, values, env)

        ElseIf TypeOf elements Is Edge() Then
            Return DirectCast(elements, Edge()).edgeAttributes(name, values, env)

        Else
            Return Message.InCompatibleType(GetType(node), elements.GetType, env,, NameOf(elements))
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

    ''' <summary>
    ''' get subnetwork components directly by test node disconnections
    ''' </summary>
    ''' <param name="graph"></param>
    ''' <returns></returns>
    <ExportAPI("components")>
    Public Function components(graph As NetworkGraph) As NetworkGraph()
        Return graph.IteratesSubNetworks(Of NetworkGraph)(singleNodeAsGraph:=True)
    End Function

    <ExportAPI("louvain_cluster")>
    Public Function LouvainCluster(g As NetworkGraph) As NetworkGraph
        Return Communities.Analysis(g)
    End Function
End Module
