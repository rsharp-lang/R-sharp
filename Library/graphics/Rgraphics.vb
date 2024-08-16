﻿#Region "Microsoft.VisualBasic::1886e31c01ceb4cf0b745335e1a88a47, Library\graphics\Rgraphics.vb"

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

    '   Total Lines: 389
    '    Code Lines: 247 (63.50%)
    ' Comment Lines: 95 (24.42%)
    '    - Xml Docs: 86.32%
    ' 
    '   Blank Lines: 47 (12.08%)
    '     File Size: 15.52 KB


    ' Module Rgraphics
    ' 
    '     Function: as_raster, as_vector, bytes, colorHeightMap, image
    '               imageFromMatrix, rasetr_matrix, rasetrFromDataframe, raster_convolution, raster_dataframe
    '               rgb_formula
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Scaler
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports vec = SMRUCC.Rsharp.Runtime.Internal.Object.vector

''' <summary>
''' The R Graphics Package
''' 
''' R functions for base graphics.
''' </summary>
<Package("graphics")>
Module Rgraphics

    Sub Main()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(RasterScaler), AddressOf raster_dataframe)
    End Sub

    ''' <summary>
    ''' construct a color heigh map
    ''' </summary>
    ''' <param name="colors">
    ''' the color set for construct the color height map, the colors used
    ''' in this parameter is used as the checkpoint for evaluate the 
    ''' intensity scale value. A raster image object also could be used
    ''' in this parameter.
    ''' </param>
    ''' <param name="levels"></param>
    ''' <param name="desc">
    ''' should this function reverse the color vector order?
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("color.height_map")>
    Public Function colorHeightMap(<RRawVectorArgument>
                                   colors As Object,
                                   Optional levels As Integer = 255,
                                   Optional desc As Boolean = False,
                                   Optional env As Environment = Nothing) As Object

        Dim colorSet = RColorPalette.getColorSet(colors, Nothing)
        Dim colorList As Color()

        If TypeOf colors Is Image Then
            ' get colors from a raster image, based on the grayscale value
            colorList = BitmapBuffer.FromImage(DirectCast(colors, Image)) _
                .GetPixelsAll _
                .Distinct _
                .OrderBy(Function(c) c.GrayScale) _
                .ToArray
        Else
            ' get colors from a color palette value
            If colorSet.StringEmpty Then
                Return Internal.debug.stop("Invalid color set was provided!", env)
            Else
                colorList = Designer.GetColors(colorSet)
            End If
        End If

        If desc Then
            colorList = colorList.Reverse.ToArray
        End If

        Return New ColorHeightMap(colorList).ScaleLevels(levels)
    End Function

    ''' <summary>
    ''' Cast the clr image object as the raster data
    ''' </summary>
    ''' <param name="img"></param>
    ''' <param name="rgb_stack">
    ''' A character vector for tells the raster function that extract the signal via rgb stack, 
    ''' default nothing means just extract the raster data via the image pixel its brightness 
    ''' value, otherwise this parameter should be a character of of value combination of chars: 
    ''' ``r``, ``g`` and ``b``.
    ''' 
    ''' example as: 
    ''' 
    ''' + rgb.stack = ['r'] means just extract the red channel as the raster data
    ''' + rgb.stack = ['g', 'b'] means extract the raster data via green and blue channel, 
    '''     the raster scale value will be evaluated as g * 10 + b
    ''' 
    ''' andalso this parameter value could be a .net clr color height map ruler object, which could be used for
    ''' mapping a color sequence to a scale level.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("as.raster")>
    <RApiReturn(GetType(RasterScaler))>
    Public Function as_raster(img As Object,
                              <RRawVectorArgument>
                              Optional rgb_stack As Object = Nothing,
                              Optional env As Environment = Nothing) As Object

        Dim formula As Func(Of Color, Single) = Nothing

        If TypeOf rgb_stack Is ColorHeightMap Then
            formula = AddressOf DirectCast(rgb_stack, ColorHeightMap).GetScale
        Else
            Dim rgbs As String() = CLRVector.asCharacter(rgb_stack)

            If Not rgbs.IsNullOrEmpty Then
                formula = rgb_formula(rgbs)
            End If
        End If

        If TypeOf img Is Image OrElse TypeOf img Is Bitmap Then
            Return New RasterScaler(New Bitmap(CType(img, Image)), formula)
        ElseIf img.GetType.ImplementInterface(Of GeneralMatrix) Then
            Return rasetr_matrix(img, formula, env)
        Else
            Return Message.InCompatibleType(GetType(Image), img.GetType, env)
        End If
    End Function

    Private Function rasetr_matrix(m As NumericMatrix, formula As Func(Of Color, Single), env As Environment) As RasterScaler
        Return New RasterScaler(m.imageFromMatrix("Gray", env), formula)
    End Function

    Private Function rgb_formula(rgbs As String()) As Func(Of Color, Single)
        Dim get_channels As Func(Of Color, Single()) =
            Function(c)
                Dim s As Single() = New Single(rgbs.Length - 1) {}

                For i As Integer = 0 To rgbs.Length - 1
                    Select Case rgbs(i)
                        Case "r" : s(i) = c.R / 255 * 10
                        Case "g" : s(i) = c.G / 255 * 10
                        Case "b" : s(i) = c.B / 255 * 10
                        Case Else
                            ' do nothing
                    End Select
                Next

                Return s
            End Function

        Return Function(c)
                   Dim v As Single() = get_channels(c)
                   Dim scale As Single = 0

                   Call Array.Reverse(v)

                   For i As Integer = 0 To v.Length - 1
                       scale += v(i) * (10 ^ i)
                   Next

                   Return scale
               End Function
    End Function

    ''' <summary>
    ''' convert raster object to dataframe. [x,y,scale]
    ''' </summary>
    ''' <param name="raster"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Function raster_dataframe(raster As RasterScaler, args As list, env As Environment) As dataframe
        Dim pixels = raster.GetRasterData.ToArray
        Dim df As New dataframe With {.columns = New Dictionary(Of String, Array)}
        Dim rgb As Boolean = args.getValue(Of Boolean)("rgb", env, [default]:=False)

        Call df.add("x", pixels.Select(Function(a) a.X))
        Call df.add("y", pixels.Select(Function(a) a.Y))
        Call df.add("scale", pixels.Select(Function(a) a.Scale))

        If rgb Then
            Dim colors As Color() = pixels _
                .Select(Function(p) raster.GetPixel(p.X - 1, p.Y - 1)) _
                .ToArray

            Call df.add("r", colors.Red)
            Call df.add("g", colors.Green)
            Call df.add("b", colors.Blue)
        End If

        Return df
    End Function

    <ExportAPI("raster_convolution")>
    Public Function raster_convolution(raster As RasterScaler,
                                       Optional size As Integer = 3,
                                       Optional stride As Integer = 1) As dataframe
        Dim xl As New List(Of Integer)
        Dim yl As New List(Of Integer)
        Dim scale As New List(Of Double)
        Dim r As New List(Of Double)
        Dim g As New List(Of Double)
        Dim b As New List(Of Double)

        For xi As Integer = 0 To raster.size.Width - 1 Step stride
            For yj As Integer = 0 To raster.size.Height - 1 Step stride
                Dim samples As New List(Of Color)

                For kx As Integer = 1 To size
                    For ky As Integer = 1 To size
                        If xi + kx < raster.size.Width AndAlso yj + ky < raster.size.Height Then
                            Call samples.Add(raster.GetPixel(xi + kx, yj + ky))
                        End If
                    Next
                Next

                If samples.Count = 0 Then
                    Continue For
                End If

                Call xl.Add(xi)
                Call yl.Add(yj)
                Call scale.Add(Aggregate c As Color In samples Into Sum(c.GetBrightness))
                Call r.Add(Aggregate c As Color In samples Into Average(c.R))
                Call g.Add(Aggregate c As Color In samples Into Average(c.G))
                Call b.Add(Aggregate c As Color In samples Into Average(c.B))
            Next
        Next

        Return New dataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"x", xl.ToArray},
                {"y", yl.ToArray},
                {"scale", scale.ToArray},
                {"r", r.ToArray},
                {"g", g.ToArray},
                {"b", b.ToArray}
            }
        }
    End Function

    ''' <summary>
    ''' Convert a raster image object data as an intensity scale vector
    ''' </summary>
    ''' <param name="raster">A specific raster image object</param>
    ''' <returns></returns>
    <ExportAPI("raster_vec")>
    Public Function as_vector(raster As RasterScaler) As vec
        Return New vec(raster.GetRasterData.Select(Function(p) p.Scale))
    End Function

    ''' <summary>
    ''' ## Display a Color Image
    ''' 
    ''' Creates a grid of colored or gray-scale rectangles with colors 
    ''' corresponding to the values in z. This can be used to display 
    ''' three-dimensional or spatial data aka images. This is a generic
    ''' function.
    ''' 
    ''' NOTE: the grid Is drawn As a Set Of rectangles by Default; see 
    ''' the useRaster argument To draw the grid As a raster image.
    ''' 
    ''' The Function hcl().colors provides a broad range Of sequential 
    ''' color palettes that are suitable For displaying ordered data, 
    ''' With n giving the number Of colors desired.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="col">
    ''' a list Of colors such As that generated by ``hcl.colors``, 
    ''' ``gray.colors`` Or similar functions.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <example>
    ''' bitmap(file = "./plot.png") {
    '''     image(matrix(c(1,2,3,4), nrow = 2, byrow = TRUE));
    ''' }
    ''' </example>
    <ExportAPI("image")>
    Public Function image(x As Object,
                          <RRawVectorArgument>
                          Optional col As Object = "YlOrRd",
                          <RListObjectArgument>
                          Optional args As list = Nothing,
                          Optional env As Environment = Nothing) As Object

        If x Is Nothing Then
            Return Nothing
        End If

        If TypeOf x Is matrix Then
            Throw New NotImplementedException
        ElseIf x.GetType.ImplementInterface(Of GeneralMatrix) Then
            Return DirectCast(x, GeneralMatrix).imageFromMatrix(col, env)
        ElseIf TypeOf x Is dataframe Then
            Return DirectCast(x, dataframe).rasetrFromDataframe(col, args, env)
        Else
            Throw New NotImplementedException
        End If
    End Function

    ''' <summary>
    ''' dataframe object should contains data fields:
    ''' 
    ''' 1. x and y(required)
    ''' 2. r, g, b for color(optional)
    ''' 3. scale, intensity, heatmap for color scale(optional)
    ''' 
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="col"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <Extension>
    Private Function rasetrFromDataframe(df As dataframe, col As Object, args As list, env As Environment) As Object
        Dim px As Integer() = CLRVector.asInteger(df!x)
        Dim py As Integer() = CLRVector.asInteger(df!y)
        Dim poly As New Polygon2D(px, py)
        Dim size = InteropArgumentHelper _
            .getSize(args.getBySynonyms("size", "Size", "dims"), env, [default]:=$"{poly.width},{poly.height}") _
            .SizeParser
        Dim cw As Integer = args.getValue("cw", env, 1)
        Dim ch As Integer = args.getValue("ch", env, 1)

        If df.hasName("r") AndAlso df.hasName("g") AndAlso df.hasName("b") Then
            Dim r As Double() = bytes(CLRVector.asNumeric(df!r))
            Dim g As Double() = bytes(CLRVector.asNumeric(df!g))
            Dim b As Double() = bytes(CLRVector.asNumeric(df!b))
            Dim raster As New Bitmap(size.Width + 1, size.Height + 1)
            Dim buffer As BitmapBuffer = BitmapBuffer.FromBitmap(raster)
            Dim color As Color

            For i As Integer = 0 To px.Length - 1
                color = Color.FromArgb(r(i), g(i), b(i))
                ' raster.SetPixel(px(i) - 1, py(i) - 1, color)
                buffer.SetPixel(px(i) - 1, py(i) - 1, color)
            Next

            Call buffer.Dispose()

            Return raster
        Else
            Dim scale As Double()

            If df.hasName("scale") Then
                scale = CLRVector.asNumeric(df!scale)
            ElseIf df.hasName("intensity") Then
                scale = CLRVector.asNumeric(df!intensity)
            ElseIf df.hasName("heatmap") Then
                scale = CLRVector.asNumeric(df!heatmap)
            Else
                Throw New NotImplementedException
            End If

            Dim colorSet As String = RColorPalette.getColorSet(col, [default]:="jet")
            Dim raster As New PixelRender(colorSet, 255, defaultColor:=Color.Transparent)
            Dim pixels As PixelData() = scale _
                .Select(Function(d, i) New PixelData(px(i), py(i), d)) _
                .ToArray

            Return raster.RenderRasterImage(pixels, size:=size, cw:=cw, ch:=ch)
        End If
    End Function

    Private Function bytes(r As Double()) As Double()
        Return SIMD.Multiply.f64_scalar_op_multiply_f64(255, SIMD.Divide.f64_op_divide_f64_scalar(r, r.Max))
    End Function

    <Extension>
    Private Function imageFromMatrix(m As GeneralMatrix, colors As Object, env As Environment) As Image
        Dim raster As New RasterMatrix(m)
        Dim dims As New Size(m.ColumnDimension, m.RowDimension)
        Dim ms As New MemoryStream

        Call Internal.Invokes.graphics.bitmap(
            file:=ms,
            args:=New list With {.slots = New Dictionary(Of String, Object) From {{"size", $"{dims.Width},{dims.Height}"}}},
            env:=env
        )
        Call graphics2DTools.rasterHeatmap(raster, colorName:=colors, dimSize:=dims, env:=env)
        Call Internal.Invokes.graphics.devOff(env:=env)
        Call ms.Flush()

#Disable Warning
        Return System.Drawing.Image.FromStream(ms)
#Enable Warning
    End Function
End Module
