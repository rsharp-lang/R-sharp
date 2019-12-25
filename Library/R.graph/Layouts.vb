
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("igraph.layouts")>
Module Layouts

    <ExportAPI("layout.force_directed")>
    Public Function forceDirect(g As NetworkGraph, Optional iterations% = 1000) As NetworkGraph
        Return g.doForceLayout(
            showProgress:=True,
            iterations:=iterations
        )
    End Function
End Module
