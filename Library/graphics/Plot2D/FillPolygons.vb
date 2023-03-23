Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Math2D

Public Class FillPolygons : Inherits Plot

    Dim polygons As Polygon2D()
    Dim dims As SizeF

    Public Sub New(polygons As Polygon2D(), theme As Theme)
        MyBase.New(theme)
        Me.polygons = polygons
        Me.dims = New Polygon2D(polygons).GetRectangle.Size
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Throw New NotImplementedException()
    End Sub
End Class
