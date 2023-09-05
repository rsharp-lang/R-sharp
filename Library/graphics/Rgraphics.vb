#Region "Microsoft.VisualBasic::68a8df60b147ec003351f6b4f320200e, D:/GCModeller/src/R-sharp/Library/graphics//Rgraphics.vb"

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

'   Total Lines: 35
'    Code Lines: 11
' Comment Lines: 22
'   Blank Lines: 2
'     File Size: 1.36 KB


' Module Rgraphics
' 
'     Function: image
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
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
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
    ''' + rgb.stack = ['g', 'b'] means extract the raster data via green and blue channel, the raster scale value will be evaluated as g * 10 + b
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("as.raster")>
    <RApiReturn(GetType(RasterScaler))>
    Public Function as_raster(img As Object,
                              <RRawVectorArgument>
                              Optional rgb_stack As Object = Nothing,
                              Optional env As Environment = Nothing) As Object

        Dim rgbs As String() = CLRVector.asCharacter(rgb_stack)
        Dim formula As Func(Of Color, Single) = Nothing

        If Not rgbs.IsNullOrEmpty Then
            formula = rgb_formula(rgbs)
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
                          Optional env As Environment = Nothing) As Object

        If x Is Nothing Then
            Return Nothing
        End If

        If TypeOf x Is matrix Then
            Throw New NotImplementedException
        ElseIf x.GetType.ImplementInterface(Of GeneralMatrix) Then
            Return DirectCast(x, GeneralMatrix).imageFromMatrix(col, env)
        ElseIf TypeOf x Is dataframe Then
            Dim df As dataframe = DirectCast(x, dataframe)
            Dim px As Integer() = CLRVector.asInteger(df!x)
            Dim py As Integer() = CLRVector.asInteger(df!y)
            Dim r As Double() = bytes(CLRVector.asNumeric(df!r))
            Dim g As Double() = bytes(CLRVector.asNumeric(df!g))
            Dim b As Double() = bytes(CLRVector.asNumeric(df!b))
            Dim poly As New Polygon2D(px, py)
            Dim raster As New Bitmap(CInt(poly.width) + 1, CInt(poly.height) + 1)
            Dim buffer As BitmapBuffer = BitmapBuffer.FromBitmap(raster)
            Dim color As Color

            For i As Integer = 0 To px.Length - 1
                color = Color.FromArgb(r(i), g(i), b(i))
                buffer.SetPixel(px(i) - 1, py(i) - 1, color)
            Next

            Call buffer.Dispose()

            Return raster
        Else
            Throw New NotImplementedException
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
        Call graphics2D.rasterHeatmap(raster, colorName:=colors, dimSize:=dims, env:=env)
        Call Internal.Invokes.graphics.devOff(env:=env)
        Call ms.Flush()

#Disable Warning
        Return System.Drawing.Image.FromStream(ms)
#Enable Warning
    End Function
End Module
