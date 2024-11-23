#Region "Microsoft.VisualBasic::79cb9c89fe5d6687a645cdcba9b4c9eb, Library\igraph\builder.vb"

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

'   Total Lines: 131
'    Code Lines: 83 (63.36%)
' Comment Lines: 29 (22.14%)
'    - Xml Docs: 86.21%
' 
'   Blank Lines: 19 (14.50%)
'     File Size: 5.19 KB


' Module builder
' 
'     Function: fromCorDataframe, FromCorrelations, SimilarityGraph
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports DataFrame = Microsoft.VisualBasic.Math.DataFrame.DataFrame
Imports rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports std = System.Math

''' <summary>
''' helper module for convert datasets to network graph object
''' </summary>
<Package("builder")>
Module builder

    ''' <summary>
    ''' Create a network graph based on the item correlations
    ''' </summary>
    ''' <param name="x">a correlation matrix or 
    ''' a correlation matrix represents in dataframe object.
    ''' </param>
    ''' <param name="threshold">the absolute threshold value of the correlation value.</param>
    ''' <param name="pvalue"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("correlation.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function FromCorrelations(x As Object,
                                     Optional threshold As Double = 0.65,
                                     Optional pvalue As Double = 1,
                                     Optional group As list = Nothing,
                                     Optional top_edges As Integer = Integer.MaxValue,
                                     Optional env As Environment = Nothing) As Object

        Dim cor As CorrelationMatrix = TryCast(x, CorrelationMatrix)

        If x Is Nothing Then
            Call "The given correlation matrix data is nothing!".Warning
            Return Nothing
        End If

        If cor Is Nothing Then
            If TypeOf x Is rdataframe Then
                Dim df As rdataframe = DirectCast(x, rdataframe)
                Dim names As String() = df.colnames
                Dim corDbl As Double()() = New Double(names.Length - 1)() {}
                Dim pval As Double()() = New Double(names.Length - 1)() {}
                Dim i As i32 = 0

                For Each name As String In names
                    corDbl(i) = CLRVector.asNumeric(df(name))
                    pval(++i) = New Double(names.Length - 1) {}
                Next

                ' 20241008
                ' construct a new correlation matrix
                cor = New CorrelationMatrix(names.Indexing, corDbl, pval)
            Else
                Return Message.InCompatibleType(GetType(CorrelationMatrix), x.GetType, env)
            End If
        End If

        Dim g As NetworkGraph = cor.BuildNetwork(threshold, pvalue).Item1

        If g.graphEdges.Count > top_edges Then
            ' sort edges by value and get top edges
            Dim filters = g.graphEdges _
                .OrderByDescending(Function(e) std.Abs(e.weight)) _
                .Skip(top_edges) _
                .ToArray

            For Each edge As Edge In filters
                Call g.RemoveEdge(edge)
            Next
        End If

        If Not group Is Nothing Then
            Dim class_labels As Dictionary(Of String, String) = group.AsGeneric(env, "no_class")

            For Each v As Node In g.vertex
                v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = class_labels _
                    .TryGetValue(v.label, [default]:="no_class")
            Next
        End If

        Call VBDebugger.EchoLine(g.ToString)

        Return g
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="cutoff"></param>
    ''' <param name="sample_label">
    ''' the node type label of the rows in the given dataframe
    ''' </param>
    ''' <param name="feature_label">
    ''' the node type label of the columns in the given dataframe
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("dataframe.to_graph")>
    Public Function fromCorDataframe(x As rdataframe, Optional cutoff As Double = 0.65,
                                     Optional sample_label As String = "Sample",
                                     Optional feature_label As String = "Feature",
                                     Optional env As Environment = Nothing) As Object

        Dim df = MathDataSet.toFeatureSet(x, env)

        If TypeOf df Is Message Then
            Return df
        End If

        Return CorrelationNetwork.BuildNetwork(DirectCast(df, DataFrame), cutoff, sample_label, feature_label)
    End Function

    <ExportAPI("similarity_graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function SimilarityGraph(x As DistanceMatrix, Optional cutoff As Double = 0.6) As Object
        Return x.BuildNetwork(cutoff)
    End Function

    ''' <summary>
    ''' Create sparse graph matrix
    ''' </summary>
    ''' <param name="g">
    ''' should be a graph object that implements of the interface <see cref="SparseGraph.ISparseGraph"/>
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("spare_graph")>
    <RApiReturn(GetType(SparseGraph))>
    Public Function clone_sparegraph(g As Object, Optional env As Environment = Nothing) As Object
        If g Is Nothing Then
            Return Nothing
        End If

        If TypeOf g Is list AndAlso DirectCast(g, list).hasName(NameOf(SparseGraph.Edges)) Then
            ' from json list?
            Dim edges = DirectCast(g, list).getByName(NameOf(SparseGraph.Edges))
            Dim edgeSet As New List(Of SparseGraph.Edge)

            For Each obj In ObjectSet.GetObjectSet(edges, env)
                If Not TypeOf obj Is list Then
                    Return RInternal.debug.stop("the required edge data should be a tuple list object!", env)
                End If

                Dim u As String = DirectCast(obj, list).getValue(Of String)("u", env)
                Dim v As String = DirectCast(obj, list).getValue(Of String)("v", env)

                ' missing data
                If u Is Nothing OrElse v Is Nothing Then
                    Return RInternal.debug.stop("there are some missing data inside your molecule graph!", env)
                End If

                Call edgeSet.Add(New SparseGraph.Edge(u, v))
            Next

            Return New SparseGraph() With {
                .Edges = edgeSet.ToArray
            }
        End If

        If g.GetType.ImplementInterface(Of SparseGraph.ISparseGraph) Then
            Return SparseGraph.Copy(DirectCast(g, SparseGraph.ISparseGraph))
        Else
            Return Message.InCompatibleType(GetType(SparseGraph), g.GetType, env)
        End If
    End Function
End Module
