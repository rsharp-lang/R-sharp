
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("igraph.layouts")>
Module Layouts

    <ExportAPI("layout.force_directed")>
    Public Function forceDirect(g As NetworkGraph) As NetworkGraph
        Return g.doForceLayout(showProgress:=True)
    End Function
End Module
