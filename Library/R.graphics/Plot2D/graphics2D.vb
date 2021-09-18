#Region "Microsoft.VisualBasic::b0b584795688025193ef106c1af5211f, Library\R.graphics\Plot2D\graphics2D.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Module graphics2D
    ' 
    '     Function: axisTicks, contourPolygon, DrawCircle, drawLegends, DrawTriangle
    '               legend, line2D, measureString, offset2D, point2D
    '               (+2 Overloads) rectangle, scale, size
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Canvas = Microsoft.VisualBasic.Imaging.Graphics2D
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("graphics2D")>
Module graphics2D

    <ExportAPI("legend")>
    Public Function legend(title$, color As Object, Optional font_style As Object = CSSFont.Win10Normal, Optional shape As LegendStyles = LegendStyles.Circle) As LegendObject
        Return New LegendObject With {
            .color = RColorPalette.getColor(color),
            .fontstyle = InteropArgumentHelper.getFontCSS(font_style),
            .style = shape,
            .title = title
        }
    End Function

    <ExportAPI("measureString")>
    Public Function measureString(str As String, font As Object, Optional canvas As IGraphics = Nothing) As Double()
        If canvas Is Nothing Then
            canvas = New Bitmap(1, 1).CreateCanvas2D
        End If

        Dim fontStyle As Font = CSSFont.TryParse(InteropArgumentHelper.getFontCSS(font)).GDIObject(canvas.Dpi)
        Dim size As SizeF = canvas.MeasureString(str, fontStyle)

        Return New Double() {size.Width, size.Height}
    End Function

    <ExportAPI("draw.legend")>
    Public Function drawLegends(canvas As Object, legends As LegendObject(), location As PointF,
                                Optional border As Object = Stroke.AxisStroke,
                                <RRawVectorArgument>
                                Optional gSize As Object = "120,45",
                                Optional env As Environment = Nothing) As Object
        Dim g As IGraphics
        Dim stroke As Stroke = Nothing

        If Not border Is Nothing Then
            stroke = InteropArgumentHelper _
                .getStrokePenCSS(border) _
                .DoCall(AddressOf Stroke.TryParse)
        End If

        If TypeOf canvas Is ImageData Then
            g = New Canvas(DirectCast(canvas, ImageData).AsGDIImage)
        ElseIf TypeOf canvas Is Bitmap OrElse TypeOf canvas Is Image Then
            g = New Canvas(DirectCast(canvas, Image))
        ElseIf TypeOf canvas Is IGraphics Then
            g = canvas
        Else
            Return Message.InCompatibleType(GetType(IGraphics), canvas.GetType, env)
        End If

        Call g.DrawLegends(location, legends, gSize:=InteropArgumentHelper.getSize(gSize, env), regionBorder:=stroke)

        Select Case g.GetType
            Case GetType(Canvas) : Return DirectCast(g, Canvas).ImageResource
            Case Else
                Throw New NotImplementedException
        End Select
    End Function

    <ExportAPI("rect")>
    <RApiReturn(GetType(Rectangle), GetType(RectangleF))>
    Public Function rectangle(x As Double, y As Double, w As Double, h As Double, Optional float As Boolean = True) As Object
        If float Then
            Return New RectangleF(x, y, w, h)
        Else
            Return New Rectangle(x, y, w, h)
        End If
    End Function

    <ExportAPI("rectangle")>
    Public Function rectangle(location As PointF, size As SizeF) As RectangleF
        Return New RectangleF(location, size)
    End Function

    <ExportAPI("point")>
    <RApiReturn(GetType(Point), GetType(PointF))>
    Public Function point2D(x As Double, y As Double, Optional float As Boolean = True) As Object
        If float Then
            Return New PointF(x, y)
        Else
            Return New Point(x, y)
        End If
    End Function

    <ExportAPI("size")>
    <RApiReturn(GetType(Size), GetType(SizeF))>
    Public Function size(w As Double, h As Double, Optional float As Boolean = True) As Object
        If float Then
            Return New SizeF(w, h)
        Else
            Return New Size(w, h)
        End If
    End Function

    <ExportAPI("offset2D")>
    Public Function offset2D(layout As Object, <RRawVectorArgument> offset As Object, Optional env As Environment = Nothing) As Object
        Dim offsetPt As PointF

        If offset Is Nothing Then
            Return layout
        ElseIf TypeOf offset Is Point Then
            offsetPt = DirectCast(offset, Point).PointF
        ElseIf TypeOf offset Is PointF Then
            offsetPt = DirectCast(offset, PointF)
        ElseIf TypeOf offset Is Long() OrElse TypeOf offset Is Integer() OrElse TypeOf offset Is Single() OrElse TypeOf offset Is Double() Then
            With REnv.asVector(Of Double)(offset)
                offsetPt = New PointF(.GetValue(Scan0), .GetValue(1))
            End With
        ElseIf TypeOf offset Is list Then
            With DirectCast(offset, list)
                offsetPt = New PointF(.getValue(Of Double)("x", env), .getValue(Of Double)("y", env))
            End With
        ElseIf TypeOf offset Is vector Then
            With DirectCast(offset, vector).data
                offsetPt = New PointF(CDbl(.GetValue(Scan0)), CDbl(.GetValue(1)))
            End With
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(PointF), offset.GetType, env,, NameOf(offset)), env)
        End If

        If TypeOf layout Is Point Then
            Return DirectCast(layout, Point).OffSet2D(offsetPt)
        ElseIf TypeOf layout Is PointF Then
            Return DirectCast(layout, PointF).OffSet2D(offsetPt)
        ElseIf TypeOf layout Is Rectangle Then
            Return DirectCast(layout, Rectangle).OffSet2D(offsetPt)
        ElseIf TypeOf layout Is RectangleF Then
            Return DirectCast(layout, RectangleF).OffSet2D(offsetPt)
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(PointF), layout.GetType, env,, NameOf(layout)), env)
        End If
    End Function

    <ExportAPI("line")>
    Public Function line2D(<RRawVectorArgument> a As Object, <RRawVectorArgument> b As Object, Optional stroke As Object = Stroke.AxisStroke) As Shapes.Line
        Dim p1 As PointF = InteropArgumentHelper.getVector2D(a)
        Dim p2 As PointF = InteropArgumentHelper.getVector2D(b)
        Dim penCSS As String = InteropArgumentHelper.getStrokePenCSS(stroke)

        Return New Shapes.Line(p1, p2, CSS.Stroke.TryParse(penCSS))
    End Function

    <ExportAPI("draw.triangle")>
    Public Function DrawTriangle(g As IGraphics, topleft As PointF, size As SizeF,
                                 <RRawVectorArgument>
                                 Optional color As Object = "black",
                                 Optional border As Object = Stroke.AxisStroke) As IGraphics
        Call Triangle.Draw(g, topleft.ToPoint, size.ToSize, gdi.getBrush(color), InteropArgumentHelper.getStrokePenCSS(border).DoCall(AddressOf Stroke.TryParse))
        Return g
    End Function

    <ExportAPI("draw.circle")>
    Public Function DrawCircle(g As IGraphics, center As PointF, r As Single, <RRawVectorArgument> Optional color As Object = "black", Optional border As Object = Stroke.AxisGridStroke) As IGraphics
        Call Circle.Draw(g, center, r, gdi.getBrush(color), Stroke.TryParse(InteropArgumentHelper.getStrokePenCSS(border, Nothing)))
        Return g
    End Function

    <ExportAPI("axis.ticks")>
    Public Function axisTicks(<RRawVectorArgument> x As Object) As Double()
        Return DirectCast(REnv.asVector(Of Double)(x), Double()).Range.CreateAxisTicks
    End Function

    ''' <summary>
    ''' scale a numeric vector as the color value vector
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="colorSet"></param>
    ''' <param name="levels"></param>
    ''' <returns></returns>
    <ExportAPI("scale")>
    Public Function scale(x As Double(), <RRawVectorArgument> colorSet As Object, Optional levels As Integer = 25) As String()
        Dim colors As String() = Designer _
            .GetColors(RColorPalette.getColorSet(colorSet), n:=levels) _
            .Select(Function(c) c.ToHtmlColor) _
            .ToArray
        Dim valueRange As New DoubleRange(x)
        Dim levelRange As New DoubleRange({0, levels - 1})
        Dim i As Integer() = x _
            .Select(Function(xi)
                        Return CInt(valueRange.ScaleMapping(xi, levelRange))
                    End Function) _
            .ToArray

        Return i _
            .Select(Function(index)
                        If valueRange.Length = 0 Then
                            Return colors(0)
                        Else
                            Return colors(index)
                        End If
                    End Function) _
            .ToArray
    End Function

    <ExportAPI("contour")>
    Public Function contourPolygon(data As MeasureData()) As ContourLayer()
        Dim contours As GeneralPath() = ContourLayer.GetContours(data).ToArray
        Dim result = contours.Select(Function(c) c.GetContour).ToArray

        Return result
    End Function

    ''' <summary>
    ''' Measure object outline via run contour tracing algorithm.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="fillDots"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("contour_tracing")>
    Public Function contourTracing(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object,
                                   Optional fillDots As Integer = 1,
                                   Optional env As Environment = Nothing) As GeneralPath

        Dim xi As Double() = REnv.asVector(Of Double)(x)
        Dim yi As Double() = REnv.asVector(Of Double)(y)

        Return ContourLayer.GetOutline(xi, yi, fillDots)
    End Function
End Module
