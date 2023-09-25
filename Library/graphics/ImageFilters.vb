#Region "Microsoft.VisualBasic::9d517633ae64eab44811621c24deb2a3, D:/GCModeller/src/R-sharp/Library/graphics//ImageFilters.vb"

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

'   Total Lines: 101
'    Code Lines: 85
' Comment Lines: 0
'   Blank Lines: 16
'     File Size: 3.98 KB


' Module ImageFilters
' 
'     Function: Diffusion, Emboss, gaussBlurEffect, HqxScalesF, Pencil
'               Sharp, Soften, WoodCarving
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap.hqx
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Filters
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("filter")>
Module ImageFilters

    <ExportAPI("pencil")>
    Public Function Pencil(image As Image, Optional sensitivity As Single = 25) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Pencil(sensitivity).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("wood_carving")>
    Public Function WoodCarving(image As Image, Optional sensitivity As Single = 25) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Pencil(sensitivity, woodCarving:=True).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("emboss")>
    Public Function Emboss(image As Image,
                           <RRawVectorArgument(GetType(Integer))>
                           Optional direction As Object = "1,1",
                           Optional lighteness As Integer = 127,
                           Optional env As Environment = Nothing) As Image

        Dim dir As Size = InteropArgumentHelper _
            .getSize(direction, env, "1,1") _
            .SizeParser

        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Emboss(dir.Width, dir.Height, lighteness).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("diffusion")>
    Public Function Diffusion(image As Image) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Diffusion.GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("soft")>
    Public Function Soften(image As Image, Optional max As Double = 255) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Soften(max).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("sharp")>
    Public Function Sharp(image As Image, Optional sharpDgree As Single = 0.3, Optional max As Double = 255) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Sharp(sharpDgree, max).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("gauss_blur")>
    <RApiReturn(GetType(Bitmap))>
    Public Function gaussBlurEffect(image As Object,
                                    Optional levels As Integer = 100,
                                    Optional env As Environment = Nothing) As Object

        If image Is Nothing Then
            Return Nothing
        End If

        If TypeOf image Is ImageData Then
            image = DirectCast(image, ImageData).Image
        End If
        If TypeOf image Is Image OrElse TypeOf image Is Bitmap Then
            image = CType(image, Image)
        Else
            Return Internal.debug.stop({$"required of the gdi+ image data! (given {image.GetType.FullName})"}, env)
        End If

        Dim bitmap As New Bitmap(DirectCast(image, Image))

        For i As Integer = 0 To levels
            bitmap = GaussBlur.GaussBlur(bitmap)
        Next

        Return bitmap
    End Function

    <ExportAPI("hqx_scales")>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function HqxScalesF(image As Image, Optional scale As HqxScales = HqxScales.Hqx_2x) As Object
        Return New RasterScaler(New Bitmap(image)).Scale(scale)
    End Function

    ''' <summary>
    ''' .NET Implement of Real-time Contrast Preserving Decolorization
    ''' </summary>
    ''' <param name="img"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' https://github.com/IntPtrZero/RTCPRGB2Gray/tree/master
    ''' </remarks>
    <ExportAPI("RTCP_gray")>
    Public Function RTCPGray_func(img As Image) As Bitmap
        Return RTCP.RTCPGray(New Bitmap(img))
    End Function

    <ExportAPI("RTCP_weight")>
    <RApiReturn("r", "g", "b")>
    Public Function RTCPWeight(img As Image) As list
        Dim w = RTCP.MeasureGlobalWeight(New Bitmap(img))
        Dim c As New list With {.slots = New Dictionary(Of String, Object) From {
            {"r", w.r},
            {"g", w.g},
            {"b", w.b}
        }}

        Return c
    End Function
End Module
