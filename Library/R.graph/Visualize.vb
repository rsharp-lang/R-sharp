Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("igraph.render")>
Module Visualize

    <ExportAPI("render.Plot")>
    Public Function renderPlot(g As NetworkGraph) As GraphicsData
        Return g.DrawImage()
    End Function

End Module

