#Region "Microsoft.VisualBasic::684ce8b0ff610d96330d6148cedc1d27, E:/GCModeller/src/R-sharp/Library/graphics//Plot2D/graphics2D.vb"

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


    ' Code Statistics:

    '   Total Lines: 688
    '    Code Lines: 498
    ' Comment Lines: 109
    '   Blank Lines: 81
    '     File Size: 28.24 KB


    ' Module graphics2DTools
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: asciiArt, axisTicks, colorMapLegend, contourPolygon, contourTracing
    '               DrawCircle, drawLegends, DrawRectangle, DrawTriangle, layout_grid
    '               legend, line2D, measureString, offset2D, paddingString
    '               paddingVector, plotColorMap, point2D, pointsVector, rasterHeatmap
    '               (+2 Overloads) rectangle, scale, size, sizeVector
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Scaler
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text.ASCIIArt
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Canvas = Microsoft.VisualBasic.Imaging.Graphics2D

''' <summary>
''' 2D graphics
''' </summary>
<Package("graphics2D")>
<RTypeExport("colormap", GetType(ColorMapLegend))>
Module graphics2DTools

    Sub New()
        Call Internal.generic.add("plot", GetType(ColorMapLegend), AddressOf plotColorMap)
    End Sub

    ''' <summary>
    ''' evaluate location automatically
    ''' </summary>
    ''' <param name="legend"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Private Function plotColorMap(legend As ColorMapLegend, args As list, env As Environment) As Object
        Dim driver As Drivers = imageDriverHandler.getDriver(env)
        Dim size As String = InteropArgumentHelper.getSize(args!size, env, [default]:="0,0")
        Dim margin As String = InteropArgumentHelper.getPadding(args!padding)
        Dim barmap As Boolean = CLRVector.asLogical(args.getBySynonyms("bar")).DefaultFirst([default]:=False)

        If barmap Then
            Dim g As Canvas = New Size(256, 1).CreateGDIDevice
            Dim colors As Brush() = legend.ScaleColors(n:=256) _
                .Select(Function(c) DirectCast(New SolidBrush(c), Brush)) _
                .ToArray

            For i As Integer = 0 To colors.Length - 1
                Call g.FillRectangle(colors(i), New Rectangle(i, 0, 1, 1))
            Next

            Call g.Flush()

            Return g.ImageResource
        ElseIf Not size.SizeParser.IsValidGDIParameter Then
            ' draw on current graphics context
            Dim dev As graphicsDevice = curDev
            Dim padding As Padding = InteropArgumentHelper.getPadding(dev!padding)
            Dim canvas As New GraphicsRegion(dev.g.Size, padding)
            Dim layout As Rectangle = canvas.PlotRegion

            layout = New Rectangle(
                x:=layout.Right + padding.Right / 4,
                y:=layout.Top,
                width:=padding.Right * 2 / 3,
                height:=layout.Height
            )
            legend.Draw(dev.g, layout)
        Else
            Return g.GraphicsPlots(
                size.SizeParser,
                Padding.TryParse(margin),
                "transparent",
                driver:=driver,
                dpi:="300,300",
                plotAPI:=Sub(ByRef g, region)
                             Call legend.Draw(g, region.PlotRegion)
                         End Sub)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="layout">
    ''' should be two integer element which is indicate that 
    ''' the element count on x and the element count on y.
    ''' </param>
    ''' <param name="margin">
    ''' the css internal padding data for each cell in the 
    ''' generated grid layout.
    ''' </param>
    ''' <param name="env"></param>
    ''' <remarks>
    ''' the canvas size and the entire padding is comes from the 
    ''' current graphics device.
    ''' </remarks>
    ''' <returns></returns>
    <ExportAPI("layout.grid")>
    Public Function layout_grid(layout As Integer(),
                                <RRawVectorArgument>
                                Optional margin As Object = 0,
                                Optional env As Environment = Nothing) As Rectangle()

        Dim dev As graphicsDevice = curDev
        Dim size As Size = InteropArgumentHelper.getSize(dev!size, env).SizeParser
        Dim padding As Padding = InteropArgumentHelper.getPadding(dev!padding)
        Dim innerPadding As Padding = InteropArgumentHelper.getPadding(margin)
        Dim region As New GraphicsRegion(size, padding)
        Dim rect As Rectangle = region.PlotRegion
        Dim layouts As New List(Of Rectangle)
        Dim x As Integer = rect.Left
        Dim y As Integer = rect.Top
        Dim w As Integer = rect.Width / layout(0)
        Dim h As Integer = rect.Height / layout(1)
        Dim cell As Rectangle

        For i As Integer = 1 To layout(1)
            x = rect.Left

            For j As Integer = 1 To layout(0)
                cell = New Rectangle With {
                    .X = x + innerPadding.Left,
                    .Y = y + innerPadding.Top,
                    .Width = w - innerPadding.Horizontal,
                    .Height = h - innerPadding.Vertical
                }
                layouts.Add(cell)

                x += w
            Next

            y += h
        Next

        Return layouts.ToArray
    End Function

    ''' <summary>
    ''' create a valid css padding string
    ''' </summary>
    ''' <param name="padding"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("paddingString")>
    Public Function paddingString(<RRawVectorArgument> padding As Object, Optional env As Environment = Nothing) As String
        Return InteropArgumentHelper.getPadding(padding, "padding: 0px 0px 0px 0px;")
    End Function

    <ExportAPI("paddingVector")>
    Public Function paddingVector(<RRawVectorArgument> margin As Object, Optional env As Environment = Nothing) As Double()
        Dim padding As Padding = Padding.TryParse(InteropArgumentHelper.getPadding(margin, "padding: 0px 0px 0px 0px;"))
        Dim v As Double() = New Double() {
            padding.Top,
            padding.Right,
            padding.Bottom,
            padding.Left
        }

        Return v
    End Function

    ''' <summary>
    ''' create a new color map legend
    ''' </summary>
    ''' <param name="colors"></param>
    ''' <param name="ticks"></param>
    ''' <param name="title"></param>
    ''' <param name="mapLevels"></param>
    ''' <param name="format"></param>
    ''' <param name="tickAxisStroke"></param>
    ''' <param name="tickFont"></param>
    ''' <param name="titleFont"></param>
    ''' <param name="unmapColor"></param>
    ''' <param name="foreColor"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("colorMap.legend")>
    Public Function colorMapLegend(<RRawVectorArgument> colors As Object,
                                   <RRawVectorArgument>
                                   Optional ticks As Object = "0,1",
                                   Optional title As String = "Color Map",
                                   Optional mapLevels As Integer = 60,
                                   Optional format As String = "G3",
                                   Optional tickAxisStroke As Object = Stroke.AxisStroke,
                                   Optional tickFont As Object = CSSFont.PlotLabelNormal,
                                   Optional titleFont As Object = CSSFont.PlotSmallTitle,
                                   Optional unmapColor As Object = Nothing,
                                   Optional foreColor As Object = "black",
                                   Optional env As Environment = Nothing) As ColorMapLegend

        Dim colorName As String = RColorPalette.getColorSet(colors)
        Dim foreColorEx As Color = RColorPalette.getColor(foreColor, [default]:="black").TranslateColor
        Dim ticks_vec As Double() = CLRVector.asNumeric(ticks)

        Return New ColorMapLegend(colorName, mapLevels) With {
            .format = format,
            .legendOffsetLeft = 0,
            .noblank = False,
            .ruleOffset = 5,
            .tickAxisStroke = Stroke.TryParse(tickAxisStroke).GDIObject,
            .tickFont = CSSFont.TryParse(tickFont).GDIObject(300),
            .ticks = ticks_vec,
            .title = title,
            .titleFont = CSSFont.TryParse(titleFont).GDIObject(300),
            .unmapColor = RColorPalette.getColor(unmapColor, [default]:=Nothing),
            .foreColor = foreColorEx
        }
    End Function

    ''' <summary>
    ''' create a legend element
    ''' </summary>
    ''' <param name="title$"></param>
    ''' <param name="color"></param>
    ''' <param name="font_style"></param>
    ''' <param name="shape"></param>
    ''' <returns></returns>
    <ExportAPI("legend")>
    Public Function legend(title$, color As Object,
                           Optional font_style As Object = CSSFont.Win10Normal,
                           Optional shape As LegendStyles = LegendStyles.Circle) As LegendObject

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
            canvas = curDev.g
        End If
        If canvas Is Nothing Then
            canvas = New Bitmap(1, 1).CreateCanvas2D
        End If

        Dim fontStyle As Font

        If TypeOf font Is Font Then
            fontStyle = font
        Else
            fontStyle = CSSFont.TryParse(InteropArgumentHelper.getFontCSS(font)).GDIObject(canvas.Dpi)
        End If

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

    <ExportAPI("pointVector")>
    Public Function pointsVector(x As Array,
                                 Optional y As Array = Nothing,
                                 Optional env As Environment = Nothing) As PointF()

        If y Is Nothing OrElse y.Length = 0 Then
            Return (From t As String
                    In CLRVector.asCharacter(x)
                    Let p As Double() = t _
                        .Split(","c) _
                        .Select(AddressOf Val) _
                        .ToArray
                    Select New PointF(p(0), p(1))).ToArray
        Else
            Dim px As Double() = CLRVector.asNumeric(x)
            Dim py As Double() = CLRVector.asNumeric(y)

            Return px _
                .Select(Function(xi, i)
                            Return New PointF(xi, py(i))
                        End Function) _
                .ToArray
        End If
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

    <ExportAPI("sizeVector")>
    Public Function sizeVector(<RRawVectorArgument> size As Object, Optional env As Environment = Nothing) As Double()
        Dim sizeVal = InteropArgumentHelper.getSize(size, env, [default]:="0,0")
        Dim sz As Size = sizeVal.SizeParser

        Return New Double() {sz.Width, sz.Height}
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
        ElseIf TypeOf offset Is Long() OrElse
            TypeOf offset Is Integer() OrElse
            TypeOf offset Is Single() OrElse
            TypeOf offset Is Double() Then

            With CLRVector.asNumeric(offset)
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

        Return New Shapes.Line(p1, p2, CSS.Stroke.TryParse(penCSS).GDIObject)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="region"></param>
    ''' <param name="dimSize"></param>
    ''' <param name="colorName"></param>
    ''' <param name="gauss"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function will returns nothing, not returns the generated image result
    ''' target heatmap image always rendering onto current opened graphics device
    ''' </remarks>
    <ExportAPI("rasterHeatmap")>
    Public Function rasterHeatmap(<RRawVectorArgument>
                                  x As Object,
                                  Optional region As Rectangle = Nothing,
                                  <RRawVectorArgument>
                                  Optional dimSize As Object = Nothing,
                                  <RRawVectorArgument>
                                  Optional colorName As Object = "jet",
                                  Optional gauss As Integer = 0,
                                  Optional colorLevels As Integer = 255,
                                  Optional rasterBitmap As Boolean = False,
                                  Optional fillRect As Boolean = True,
                                  Optional strict As Boolean = True,
                                  Optional env As Environment = Nothing) As Object

        Dim dev As graphicsDevice = curDev
        Dim canvas As Size = InteropArgumentHelper.getSize(dev!size, env).SizeParser
        Dim pixels As pipeline = pipeline.TryCreatePipeline(Of Pixel)(x, env, suppress:=True)
        Dim dimensionStr As String = InteropArgumentHelper.getSize(dimSize, env, Nothing)
        Dim dimension As Size

        If pixels.isError Then
            If TypeOf x Is RasterScaler Then
                pixels = pipeline.CreateFromPopulator(DirectCast(x, RasterScaler).GetRasterData)
            ElseIf x IsNot Nothing AndAlso x.GetType.ImplementInterface(Of IRasterGrayscaleHeatmap) Then
                pixels = pipeline.CreateFromPopulator(DirectCast(x, IRasterGrayscaleHeatmap).GetRasterPixels)
            Else
                Return pixels.getError
            End If
        End If

        If region.IsEmpty Then
            region = New Rectangle(New Point, canvas)
        End If

        Dim allPixels As Pixel() = pixels.populates(Of Pixel)(env).ToArray

        If dimensionStr.StringEmpty Then
            If allPixels.Length > 0 OrElse strict Then
                dimension = New Size With {
                    .Width = allPixels.Select(Function(p) p.X).Max,
                    .Height = allPixels.Select(Function(p) p.Y).Max
                }
            ElseIf Not strict Then
                Return Nothing
            End If
        Else
            dimension = dimensionStr.SizeParser
        End If

        If dev.g Is Nothing Then
            Return Internal.debug.stop({
                "the graphics device has not been opened yet, you should use the bitmap function for create a new at first!",
                "(the acceptor closure syntax of the bitmap function is not working at here!)"
            }, env)
        End If

        ' create base
        Dim colorSet As String = RColorPalette.getColorSet(colorName, [default]:="jet")
        Dim bitmap As Bitmap = New PixelRender(
            colorSet:=colorSet,
            mapLevels:=colorLevels,
            defaultColor:=dev.g.Background
        ).RenderRasterImage(
            pixels:=allPixels,
            size:=dimension,
            fillRect:=fillRect
        )

        If gauss > 0 Then
            bitmap = gaussBlurEffect(bitmap, levels:=gauss, env)
        End If

        If rasterBitmap Then
            Call dev.g.DrawImage(bitmap, region)
        Else
            ' rendering onto current graphics device
            Using scaler As New RasterScaler(bitmap)
                Call scaler.ScaleTo(dev.g, region)
            End Using
        End If

        Return Nothing
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
    Public Function DrawCircle(g As IGraphics, center As PointF, r As Single,
                               <RRawVectorArgument>
                               Optional color As Object = "black",
                               Optional border As Object = Stroke.AxisGridStroke) As IGraphics

        Call Circle.Draw(g, center, r, gdi.getBrush(color), Stroke.TryParse(InteropArgumentHelper.getStrokePenCSS(border, Nothing)))
        Return g
    End Function

    <ExportAPI("draw.rectangle")>
    Public Function DrawRectangle(color As Object, rect As Object, Optional g As IGraphics = Nothing)
        Dim colorVal As Color = RColorPalette.GetRawColor(color)

        If g Is Nothing Then
            g = Invokes.graphics.curDev.g
        End If

        If TypeOf rect Is Rectangle Then
            Call g.FillRectangle(New SolidBrush(colorVal), DirectCast(rect, Rectangle))
        Else
            Call g.FillRectangle(New SolidBrush(colorVal), DirectCast(rect, RectangleF))
        End If

        Return g
    End Function

    ''' <summary>
    ''' create axis tick vector data based on a given data range value
    ''' </summary>
    ''' <param name="x">
    ''' a numeric vector data for specific the data range.
    ''' </param>
    ''' <param name="ticks">
    ''' the number of the ticks in the generated output axis tick vector
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("axis.ticks")>
    Public Function axisTicks(<RRawVectorArgument> x As Object, Optional ticks As Integer = 10) As Double()
        Return CLRVector.asNumeric(x) _
            .Range _
            .CreateAxisTicks(ticks)
    End Function

    ''' <summary>
    ''' scale a numeric vector as the color value vector
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="colorSet"></param>
    ''' <param name="levels"></param>
    ''' <param name="TrIQ">
    ''' set value negative or greater than 1 will be 
    ''' disable the TrIQ scaler
    ''' </param>
    ''' <returns>
    ''' A color character vector in html code format
    ''' </returns>
    <ExportAPI("scale")>
    Public Function scale(x As Double(),
                          <RRawVectorArgument>
                          colorSet As Object,
                          Optional levels As Integer = 25,
                          Optional TrIQ As Double = 0.65,
                          Optional zero As String = Nothing) As String()

        Dim colors As String() = Designer _
            .GetColors(RColorPalette.getColorSet(colorSet), n:=levels) _
            .Select(Function(c) c.ToHtmlColor) _
            .ToArray
        Dim xfilter As Double() = If(zero.StringEmpty, x, x.Where(Function(xi) xi > 0).ToArray)
        Dim hasUnmapped As Boolean = Not zero.StringEmpty
        Dim valueRange As DoubleRange = If(TrIQ <= 0 OrElse TrIQ >= 1, New DoubleRange(xfilter), xfilter.GetTrIQRange(q:=TrIQ, N:=levels))
        Dim levelRange As New IntRange(0, levels - 1)
        Dim i As Integer() = x _
            .Select(Function(xi)
                        If xi >= valueRange.Max Then
                            Return levelRange.Max
                        ElseIf hasUnmapped AndAlso xi <= 0.0 Then
                            Return -1
                        Else
                            Return valueRange.ScaleMapping(xi, levelRange)
                        End If
                    End Function) _
            .ToArray

        Return i _
            .Select(Function(index)
                        If valueRange.Length = 0 Then
                            Return colors(0)
                        ElseIf index < 0 Then
                            Return zero
                        Else
                            Return colors(index)
                        End If
                    End Function) _
            .ToArray
    End Function

    <ExportAPI("contour")>
    Public Function contourPolygon(data As MeasureData(), Optional interpolateFill As Boolean = True) As ContourLayer()
        Dim contours As GeneralPath() = ContourLayer.GetContours(data, interpolateFill:=interpolateFill).ToArray
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
    Public Function contourTracing(<RRawVectorArgument> x As Object,
                                   <RRawVectorArgument> y As Object,
                                   Optional fillDots As Integer = 1,
                                   Optional env As Environment = Nothing) As GeneralPath

        Dim xi As Double() = CLRVector.asNumeric(x)
        Dim yi As Double() = CLRVector.asNumeric(y)

        Return ContourLayer.GetOutline(xi, yi, fillDots)
    End Function

    ''' <summary>
    ''' convert bitmap to ascii characters
    ''' </summary>
    ''' <param name="image"></param>
    ''' <returns></returns>
    <ExportAPI("asciiArt")>
    <RApiReturn(GetType(String))>
    Public Function asciiArt(image As Object,
                             Optional charSet As String = "+-*.",
                             Optional env As Environment = Nothing) As Object
        Dim bitmap As Image

        If image Is Nothing Then
            Return Internal.debug.stop("the required bitmap data can not be nothing!", env)
        ElseIf TypeOf image Is Image Then
            bitmap = DirectCast(image, Image)
        ElseIf TypeOf image Is Bitmap Then
            bitmap = CType(DirectCast(image, Bitmap), Image)
        Else
            Return Message.InCompatibleType(GetType(Bitmap), image.GetType, env)
        End If

        Dim font As New Font(FontFace.Consolas, 10)
        Dim pixels As WeightedChar() = charSet.GenerateFontWeights(font)

        Return bitmap _
            .GetBinaryBitmap() _
            .Convert2ASCII(pixels)
    End Function
End Module
