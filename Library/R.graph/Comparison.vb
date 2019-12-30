Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Scripting.MetaData

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

End Module
