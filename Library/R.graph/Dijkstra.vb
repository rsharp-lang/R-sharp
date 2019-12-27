
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
