#Region "Microsoft.VisualBasic::e82346187276f99f773149fe7e549efe, studio\Rsharp_kit\MLkit\dataset\bitmap.vb"

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

'   Total Lines: 224
'    Code Lines: 162 (72.32%)
' Comment Lines: 30 (13.39%)
'    - Xml Docs: 96.67%
' 
'   Blank Lines: 32 (14.29%)
'     File Size: 8.83 KB


' Module bitmap_func
' 
'     Function: corp_rectangle, intensity_vec, open, scan_rowpeaks, summary_region
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.DensityQuery
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Math.SignalProcessing.PeakFinding
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' bitmap image dataset helper function
''' </summary>
''' 
<Package("bitmap")>
Module bitmap_func

    Sub Main()
        Call RInternal.generic.add("summary", GetType(BitmapReader), AddressOf summary_region)
    End Sub

    <RGenericOverloads("summary")>
    Private Function summary_region(bmp As BitmapReader, args As list, env As Environment) As Object
        Dim offset_x As Integer = args.getValue("x", env, [default]:=1)
        Dim offset_y As Integer = args.getValue("y", env, [default]:=1)
        Dim w As Integer = args.getValue("w", env, [default]:=5)
        Dim h As Integer = args.getValue("h", env, [default]:=5)
        Dim q As Double = args.getValue("q", env, [default]:=0.65)
        Dim intensity As Double() = bmp.intensity_vec(offset_x, offset_y, w, h)
        Dim total As Double = intensity.Sum
        Dim median As Double = intensity.Median
        Dim min As Double = intensity.Min
        Dim max As Double = intensity.Max
        Dim TrIQ90 As Double = TrIQ.FindThreshold(intensity, q:=0.9, eps:=0.1)
        Dim TrIQCut As Double = TrIQ.FindThreshold(intensity, q, eps:=0.1)
        Dim area As Double = (Aggregate ti As Double
                              In intensity
                              Where ti >= TrIQCut
                              Into Count) / intensity.Count

        Return New list(
            slot("total") = total,
            slot("median") = median,
            slot("min") = min,
            slot("max") = max,
            slot("TrIQ_90") = TrIQ90,
            slot("density") = area
        )
    End Function

    ''' <summary>
    ''' extract the raster intensity value from a specific region
    ''' </summary>
    ''' <param name="bmp"></param>
    ''' <param name="w"></param>
    ''' <param name="h"></param>
    ''' <returns></returns>
    <ExportAPI("raster_intensity")>
    <Extension>
    <RApiReturn(TypeCodes.double)>
    Public Function intensity_vec(bmp As BitmapReader, offset_x As Integer, offset_y As Integer, w As Integer, h As Integer, Optional progress As Boolean = True) As Object
        Dim intensity As New List(Of Double)
        Dim c As Color
        Dim y_iterator As IEnumerable(Of Integer)

        If progress Then
            y_iterator = Tqdm.Range(offset_y, h)
        Else
            y_iterator = Enumerable.Range(offset_y, h)
        End If

        For Each y As Integer In y_iterator
            For x As Integer = offset_x To offset_x + w - 1
                c = bmp.GetPixelColor(y, x)
                intensity.Add(BitmapScale.GrayScaleF(255 - c.R, 255 - c.G, 255 - c.B))
            Next
        Next

        Return intensity.ToArray
    End Function

    <ExportAPI("open")>
    <RApiReturn(GetType(BitmapReader))>
    Public Function open(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If buf Like GetType(Message) Then
            Return buf.TryCast(Of Message)
        End If

        Return New BitmapReader(buf.TryCast(Of Stream))
    End Function

    ''' <summary>
    ''' reader test for the pixels
    ''' </summary>
    ''' <param name="bmp"></param>
    ''' <param name="pos"></param>
    ''' <param name="size"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("corp_rectangle")>
    <RApiReturn(GetType(Bitmap))>
    Public Function corp_rectangle(bmp As BitmapReader,
                                   <RRawVectorArgument> pos As Object,
                                   <RRawVectorArgument> size As Object,
                                   Optional env As Environment = Nothing) As Object

        Dim loc_str = InteropArgumentHelper.getSize(pos, env, Nothing)
        Dim size_str = InteropArgumentHelper.getSize(size, env, Nothing)

        If loc_str.StringEmpty(, True) Then
            Return RInternal.debug.stop("the required location should not be empty!", env)
        End If
        If size_str.StringEmpty(, True) OrElse size_str = "0,0" Then
            Return RInternal.debug.stop("the required rectangle size for corp should not be empty!", env)
        End If

        Dim loc As Point = Casting.PointParser(loc_str)
        Dim sizeVal As Size = size_str.SizeParser
        Dim copy As New Bitmap(sizeVal.Width, sizeVal.Height)
        Dim c As Color
        Dim px, py As Integer
        Dim bar As Tqdm.ProgressBar = Nothing

        For Each x As Integer In Tqdm.Range(loc.X, sizeVal.Width, bar:=bar)
            bar.SetLabel($"processing (x={x})...")

            For y As Integer = loc.Y To loc.Y + sizeVal.Height - 1
                c = bmp.GetPixelColor(y, x)
                px = x - loc.X
                py = y - loc.Y
                copy.SetPixel(px, py, c)
            Next
        Next

        Return copy
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="bmp"></param>
    ''' <returns></returns>
    <ExportAPI("slic")>
    <RApiReturn(GetType(SLICPixel))>
    Public Function slic(bmp As Object,
                         Optional region_size As Double = 0.2,
                         Optional iterations As Integer = 1000,
                         Optional env As Environment = Nothing) As Object

        Dim buf As BitmapBuffer

        If bmp Is Nothing Then
            Return RInternal.debug.stop("the given bitmap data object should not be nothing!", env)
        End If

        If TypeOf bmp Is Bitmap Then
            buf = BitmapBuffer.FromBitmap(DirectCast(bmp, Bitmap))
        ElseIf TypeOf bmp Is Image Then
            buf = BitmapBuffer.FromImage(DirectCast(bmp, Image))
        ElseIf TypeOf bmp Is BitmapBuffer Then
            buf = bmp
        Else
            Return Message.InCompatibleType(GetType(BitmapBuffer), bmp.GetType, env)
        End If

        If region_size < 1 Then
            ' convert ration to pixel size
            region_size = {buf.Width * region_size, buf.Height * region_size}.Average
        End If

        Dim method As New SLIC(buf)
        Dim pixels = method.MeasureSegments(region_size, iterations)

        Return pixels
    End Function

    ''' <summary>
    ''' scan the peak signal inside the bitmap image data
    ''' </summary>
    ''' <param name="bmp"></param>
    ''' <param name="pos">[x,y] position integer vector</param>
    ''' <param name="size">[w,h] scan size integer vector</param>
    ''' <param name="threshold">
    ''' angle threshold value, value in range (0,90).
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("scan_peaks")>
    Public Function scan_rowpeaks(bmp As BitmapReader,
                                  <RRawVectorArgument> pos As Object,
                                  <RRawVectorArgument> size As Object,
                                  Optional noise As Double = 0.65,
                                  Optional threshold As Double = 30,
                                  Optional env As Environment = Nothing) As Object

        Dim loc_str = InteropArgumentHelper.getSize(pos, env, Nothing)
        Dim size_str = InteropArgumentHelper.getSize(size, env, Nothing)

        If loc_str.StringEmpty(, True) Then
            Return RInternal.debug.stop("the required location should not be empty!", env)
        End If
        If size_str.StringEmpty(, True) OrElse size_str = "0,0" Then
            Return RInternal.debug.stop("the required rectangle size for corp should not be empty!", env)
        End If

        Dim loc As Point = Casting.PointParser(loc_str)
        Dim sizeVal As Size = size_str.SizeParser
        Dim bar As Tqdm.ProgressBar = Nothing
        Dim c As Color
        Dim rows As New List(Of GeneralSignal)

        For Each y As Integer In Tqdm.Range(loc.Y, sizeVal.Height, bar:=bar)
            Dim tx As New List(Of Double)
            Dim data As New List(Of Double)

            Call bar.SetLabel($"processing row (y={y})...")

            For x As Integer = loc.X To loc.X + sizeVal.Width - 1
                c = bmp.GetPixelColor(y, x)
                tx.Add(x - loc.X)
                data.Add(BitmapScale.GrayScaleF(255 - c.R, 255 - c.G, 255 - c.B))
            Next

            Call rows.Add(New GeneralSignal(tx.ToArray, data.ToArray))
        Next

        Dim peak_detection As New ElevationAlgorithm(threshold, noise)
        Dim boundaries = (x:=New List(Of Integer), y:=New List(Of Integer))
        Dim yi As Integer = 0

        For Each row As GeneralSignal In rows
            Dim peaks = peak_detection.FindAllSignalPeaks(row).ToArray

            For Each peak As SignalPeak In peaks
                Call boundaries.x.Add(peak.rt)
                Call boundaries.y.Add(yi)
            Next

            yi += 1
        Next

        Dim scan_out As New dataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"x", boundaries.x.ToArray},
                {"y", boundaries.y.ToArray}
            }
        }

        Return scan_out
    End Function
End Module
