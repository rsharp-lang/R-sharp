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

    Dim polygons As (color As Color, regions As Polygon2D())()
    Dim dims As SizeF
    Dim union As Polygon2D

    Public Sub New(polygonGroups As PolygonGroup(), theme As Theme)
        Call MyBase.New(theme)

        Dim colors As New LoopArray(Of Color)(Designer.GetColors(theme.colorSet))

        Me.polygons = polygonGroups.Select(Function(r) (++colors, r.subregions)).ToArray
        Me.union = New Polygon2D(polygons.Select(Function(r) r.regions).IteratesALL.ToArray)
        Me.dims = union.GetRectangle.Size
    End Sub

    Public Sub New(polygons As Polygon2D(), theme As Theme)
        MyBase.New(theme)

        Dim colors As New LoopArray(Of Color)(Designer.GetColors(theme.colorSet))

        Me.polygons = polygons.Select(Function(r) (++colors, {r})).ToArray
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

        For Each tuple As (color As Color, regions As Polygon2D()) In polygons
            For Each polygon In tuple.regions
                Dim fill As Color = tuple.color
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
        Next
    End Sub
End Class
