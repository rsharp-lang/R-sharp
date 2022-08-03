#Region "Microsoft.VisualBasic::2cf830dc3f1efae9e565db3f9124f25e, R-sharp\Library\igraph\Comparison.vb"

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

    '   Total Lines: 43
    '    Code Lines: 20
    ' Comment Lines: 18
    '   Blank Lines: 5
    '     File Size: 1.88 KB


    ' Module Comparison
    ' 
    '     Function: similarity, similarityOfNode
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' Network graph comparison tools
''' </summary>
<Package("igraph.comparison")>
Module Comparison

    ''' <summary>
    ''' calculate node similarity cos score.
    ''' </summary>
    ''' <param name="a">A graph node model</param>
    ''' <param name="b">Another graph node model</param>
    ''' <returns>The node cos similarity value.</returns>
    <ExportAPI("node.cos")>
    Public Function similarityOfNode(a As Node, b As Node,
                                     Optional classEquivalent As Func(Of String, String, Double) = Nothing,
                                     Optional topologyCos As Boolean = False) As Double

        Return Analysis.NodeSimilarity(a, b, classEquivalent, topologyCos)
    End Function

    ''' <summary>
    ''' ### Graph Jaccard Similarity
    ''' 
    ''' calculate graph jaccard similarity based on the nodes' cos score.
    ''' </summary>
    ''' <param name="a">A network graph model</param>
    ''' <param name="b">Another network graph model</param>
    ''' <param name="cutoff">The similarity cutoff value of the node cos silimarity compares.</param>
    ''' <returns>The graph similarity value.</returns>
    <ExportAPI("graph.jaccard")>
    Public Function similarity(a As NetworkGraph, b As NetworkGraph,
                               Optional cutoff# = 0.85,
                               Optional classEquivalent As Func(Of String, String, Double) = Nothing,
                               Optional topologyCos As Boolean = False) As Double

        Return Analysis.GraphSimilarity(a, b, cutoff, classEquivalent, topologyCos)
    End Function
End Module
