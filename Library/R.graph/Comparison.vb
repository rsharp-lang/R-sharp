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
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    <ExportAPI("node.cos")>
    Public Function similarity(a As Node, b As Node) As Double
        Return Analysis.NodeSimilarity(a, b)
    End Function

    ''' <summary>
    ''' calculate graph jaccard similarity based on the nodes' cos score.
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="cutoff#"></param>
    ''' <returns></returns>
    <ExportAPI("graph.jaccard")>
    Public Function similarity(a As NetworkGraph, b As NetworkGraph, Optional cutoff# = 0.85) As Double
        Return Analysis.GraphSimilarity(a, b, cutoff)
    End Function
End Module
