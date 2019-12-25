Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' Rendering png or svg image from a given network graph model.
''' </summary>
<Package("igraph.render")>
Module Visualize

    ''' <summary>
    ''' Rendering png or svg image from a given network graph model.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="canvasSize$"></param>
    ''' <returns></returns>
    <ExportAPI("render.Plot")>
    Public Function renderPlot(g As NetworkGraph,
                               <RRawVectorArgument>
                               Optional canvasSize As Object = "1024,768",
                               Optional labelerIterations% = 500) As GraphicsData

        If canvasSize Is Nothing Then
            canvasSize = "1024,768"
        ElseIf canvasSize.GetType Is GetType(String()) Then
            canvasSize = DirectCast(canvasSize, Array).GetValue(Scan0)
        ElseIf canvasSize.GetType Is GetType(Long()) Then
            With DirectCast(canvasSize, Long())
                canvasSize = $"{ .GetValue(0)},{ .GetValue(1)}"
            End With
        End If

        Return g.DrawImage(
            canvasSize:=canvasSize,
            labelerIterations:=labelerIterations
        )
    End Function

    <ExportAPI("color.type_group")>
    Public Function colorByTypeGroup(g As NetworkGraph, type$, color$) As NetworkGraph
        Dim colorBrush As Brush = color.GetBrush

        g.vertex _
            .Where(Function(n)
                       Return n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = type
                   End Function) _
            .DoEach(Sub(n)
                        n.data.color = colorBrush
                    End Sub)

        Return g
    End Function

End Module

