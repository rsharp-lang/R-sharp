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
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
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
        Else
            Throw New NotImplementedException
        End If
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
