Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Public Class FillPolygons : Inherits Plot

    Dim polygons As Polygon2D()
    Dim dims As SizeF
    Dim union As Polygon2D

    Public Sub New(polygons As Polygon2D(), theme As Theme)
        MyBase.New(theme)
        Me.polygons = polygons
        Me.union = New Polygon2D(polygons)
        Me.dims = union.GetRectangle.Size
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim xTicks = union.xpoints.CreateAxisTicks
        Dim yTicks = union.ypoints.CreateAxisTicks
        Dim x = d3js.scale.linear.domain(values:=xTicks).range(canvas.GetXLinearScaleRange)
        Dim y = d3js.scale.linear.domain(values:=yTicks).range(canvas.GetYLinearScaleRange)
        Dim scaler As New DataScaler(rev:=True) With {
            .AxisTicks = (xTicks.AsVector, yTicks.AsVector),
            .region = canvas.PlotRegion,
            .X = x,
            .Y = y
        }

        If theme.drawAxis Then
            Call Axis.DrawAxis(g, canvas, scaler, xlabel, ylabel, theme)
        End If

        Dim colors As New LoopArray(Of Color)(Designer.GetColors(theme.colorSet))

        For Each polygon As Polygon2D In polygons
            Dim fill As Color = ++colors
            Dim path As New GraphicsPath
            Dim start = scaler.Translate(polygon.AsEnumerable.First)

            For Each pt As PointF In polygon.AsEnumerable.Skip(1).Select(AddressOf scaler.Translate)
                path.AddLine(start, pt)
                start = pt
            Next

            Call path.AddLine(start, scaler.Translate(polygon.AsEnumerable.First))
            Call path.CloseFigure()
            Call g.FillPath(New SolidBrush(fill), path)
        Next
    End Sub
End Class
