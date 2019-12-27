
Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports R.graphics
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("igraph.layouts")>
Module Layouts

    ''' <summary>
    ''' Do force directed layout
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="iterations"></param>
    ''' <param name="clearScreen">
    ''' Clear of the console screen when display the progress bar.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("layout.force_directed")>
    Public Function forceDirect(g As NetworkGraph,
                                Optional iterations% = 1000,
                                Optional clearScreen As Boolean = False) As NetworkGraph

        Return g.doForceLayout(
            showProgress:=True,
            iterations:=iterations,
            clearScreen:=clearScreen
        )
    End Function

    <ExportAPI("layout.orthogonal")>
    Public Function orthogonalLayout(g As NetworkGraph,
                                     <RRawVectorArgument>
                                     Optional gridSize As Object = "10,10",
                                     Optional delta# = 1) As NetworkGraph

        Dim size As Size = InteropArgumentHelper _
            .getSize(gridSize) _
            .SizeParser

        Call Orthogonal.DoLayout(g, size, delta)

        Return g
    End Function
End Module
