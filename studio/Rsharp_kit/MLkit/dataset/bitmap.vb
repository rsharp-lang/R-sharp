﻿
Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Math.SignalProcessing.PeakFinding
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' bitmap image dataset helper function
''' </summary>
''' 
<Package("bitmap")>
Module bitmap_func

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
            Return Internal.debug.stop("the required location should not be empty!", env)
        End If
        If size_str.StringEmpty(, True) OrElse size_str = "0,0" Then
            Return Internal.debug.stop("the required rectangle size for corp should not be empty!", env)
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
    ''' scan the peak signal inside the bitmap image data
    ''' </summary>
    ''' <param name="bmp"></param>
    ''' <param name="pos"></param>
    ''' <param name="size"></param>
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
            Return Internal.debug.stop("the required location should not be empty!", env)
        End If
        If size_str.StringEmpty(, True) OrElse size_str = "0,0" Then
            Return Internal.debug.stop("the required rectangle size for corp should not be empty!", env)
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