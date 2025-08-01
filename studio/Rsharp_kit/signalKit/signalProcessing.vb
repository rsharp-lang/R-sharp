﻿#Region "Microsoft.VisualBasic::56b17c7a473fc08e73c324b38979cd88, studio\Rsharp_kit\signalKit\signalProcessing.vb"

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

    '   Total Lines: 474
    '    Code Lines: 339 (71.52%)
    ' Comment Lines: 75 (15.82%)
    '    - Xml Docs: 94.67%
    ' 
    '   Blank Lines: 60 (12.66%)
    '     File Size: 19.77 KB


    ' Module signalProcessing
    ' 
    '     Function: asGeneral, asMatrix, FindAllSignalPeaks, Gaussian, gaussian_bin
    '               gaussian_fit, gaussian_peak, gaussPeaks, peakTable, plotPeaksDecomposition
    '               printSignalDf, resampler_f, writeCDF
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Signal
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Math.SignalProcessing.EmGaussian
Imports Microsoft.VisualBasic.Math.SignalProcessing.PeakFinding
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports RDataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports Microsoft.VisualBasic.ComponentModel.TagData


#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

''' <summary>
''' Signal processing is an electrical engineering subfield that focuses on analyzing, 
''' modifying and synthesizing signals, such as sound, images, potential fields, seismic 
''' signals, altimetry processing, and scientific measurements. Signal processing 
''' techniques are used to optimize transmissions, digital storage efficiency, correcting
''' distorted signals, subjective video quality, and to also detect or pinpoint components 
''' of interest in a measured signal.
''' </summary>
<Package("signalProcessing")>
<RTypeExport("signal_peak", GetType(Variable))>
Module signalProcessing

    Friend Sub Main()
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(SignalPeak()), AddressOf peakTable)
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(Variable()), AddressOf gaussPeaks)
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(GeneralSignal), AddressOf printSignalDf)

        Call RInternal.generic.add("plot", GetType(Variable()), AddressOf plotPeaksDecomposition)
    End Sub

    ''' <summary>
    ''' visual of the signal decomposition result
    ''' </summary>
    ''' <param name="decompose"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Private Function plotPeaksDecomposition(decompose As Variable(), args As list, env As Environment) As Object
        Dim x_range As Double() = CLRVector.asNumeric(args.getBySynonyms("x", "x.range"))
        Dim res As Double = CLRVector.asNumeric(args.getBySynonyms("res", "resolution")).DefaultFirst([default]:=1000)
        Dim padding As String = InteropArgumentHelper.getPadding(args.getBySynonyms("padding", "margin"), [default]:="padding: 100px 1200px 200px 200px;", env:=env)
        Dim fill As String = RColorPalette.getColor(args.getBySynonyms("fill", "grid.fill"), [default]:="white", env:=env)
        Dim sine As Boolean = args.getValue(Of Boolean)({"sine", "sin"}, env)

        If x_range.IsNullOrEmpty Then
            x_range = {0, 1}
        End If

        Dim x_axis As Double() = seq(x_range.Min, x_range.Max, by:=(x_range.Max - x_range.Min) / res).ToArray
        Dim y As PointData()() = New PointData(decompose.Length - 1)() {}
        Dim yi As Double
        Dim conv As New List(Of PointData)
        Dim offset As Integer = 0
        Dim colors As Color() = Designer.GetColors(
            term:=RColorPalette.getColorSet(
                colorSet:=args.getBySynonyms("colors", "colorSet"),
                [default]:="paper"
            ),
            n:=decompose.Length + 1
        )

        For i As Integer = 0 To decompose.Length - 1
            y(i) = New PointData(x_axis.Length - 1) {}
        Next

        For Each xi As Double In x_axis
            Dim sum As Double = 0

            For i As Integer = 0 To decompose.Length - 1
                If sine Then
                    yi = decompose(i).sine(xi)
                Else
                    yi = decompose(i).gaussian(xi)
                End If

                sum += yi
                y(i)(offset) = New PointData(xi, yi)
            Next

            conv.Add(New PointData(xi, sum))
            offset += 1
        Next

        Dim signals As New List(Of SerialData)

#Disable Warning
        Call signals.Add(New SerialData With {
            .color = Color.Black,
            .lineType = DashStyle.Solid,
            .pointSize = 5,
            .pts = conv.ToArray,
            .shape = LegendStyles.Square,
            .width = 2,
            .title = "signal"
        })

        For i As Integer = 0 To decompose.Length - 1
            Call signals.Add(New SerialData With {
                .color = colors(i),
                .lineType = DashStyle.Dash,
                .pointSize = 5,
                .pts = y(i),
                .shape = LegendStyles.Triangle,
                .title = decompose(i).ToString,
                .width = 2
            })
        Next
#Enable Warning

        Return Scatter.Plot(signals, padding:=padding,
            drawLine:=True, fill:=False,
            XtickFormat:="F2", YtickFormat:="G3",
            gridFill:=fill)
    End Function

    Private Function gaussPeaks(peaks As Variable(), args As list, env As Environment) As RDataframe
        Dim peak_df As New RDataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        Call peak_df.add("center", peaks.Select(Function(p) p.center)) ' center
        Call peak_df.add("width", peaks.Select(Function(p) p.width))
        Call peak_df.add("height", peaks.Select(Function(p) p.height))
        Call peak_df.add("offset", peaks.Select(Function(p) p.offset))

        Return peak_df
    End Function

    Private Function peakTable(sigs As SignalPeak(), args As list, env As Environment) As RDataframe
        Dim data As New Dictionary(Of String, Array) From {
            {NameOf(SignalPeak.rt), sigs.Select(Function(p) p.rt).ToArray},
            {NameOf(SignalPeak.rtmin), sigs.Select(Function(p) p.rtmin).ToArray},
            {NameOf(SignalPeak.rtmax), sigs.Select(Function(p) p.rtmax).ToArray},
            {NameOf(SignalPeak.signalMax), sigs.Select(Function(p) p.signalMax).ToArray},
            {NameOf(SignalPeak.snratio), sigs.Select(Function(p) p.snratio).ToArray},
            {NameOf(SignalPeak.baseline), sigs.Select(Function(p) p.baseline).ToArray},
            {NameOf(SignalPeak.integration), sigs.Select(Function(p) p.integration).ToArray},
            {"ticks", sigs.Select(Function(p) p.region.TryCount).ToArray}
        }

        Return New RDataframe With {
            .columns = data
        }
    End Function

    Private Function printSignalDf(sig As GeneralSignal, args As list, env As Environment) As RDataframe
        Dim df As New RDataframe With {.columns = New Dictionary(Of String, Array)}

        Call df.add("x", sig.Measures)
        Call df.add("y", sig.Strength)

        Return df
    End Function

    ''' <summary>
    ''' Find all signal peaks from a given time signal data
    ''' </summary>
    ''' <param name="signal"></param>
    ''' <param name="baseline">the quantile threshold value for measure the noise baseline</param>
    ''' <param name="cutoff"></param>
    ''' <returns></returns>
    <ExportAPI("findpeaks")>
    Public Function FindAllSignalPeaks(signal As GeneralSignal,
                                       Optional baseline As Double = 0.65,
                                       Optional cutoff As Double = 3) As SignalPeak()

        Return New ElevationAlgorithm(cutoff, baseline) _
            .FindAllSignalPeaks(signal) _
            .ToArray
    End Function

    ''' <summary>
    ''' Create a new general signal
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="signals"></param>
    ''' <param name="title"></param>
    ''' <param name="meta"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.signal")>
    <RApiReturn(GetType(GeneralSignal))>
    Public Function asGeneral(<RRawVectorArgument> x As Object,
                              <RRawVectorArgument>
                              Optional signals As Object = Nothing,
                              Optional title$ = "general signal",
                              <RListObjectArgument>
                              Optional meta As list = Nothing,
                              Optional env As Environment = Nothing) As Object

        If signals Is Nothing Then
            Dim sig = pipeline.TryCreatePipeline(Of ITimeSignal)(x, env)

            If sig.isError Then
                Return sig.getError
            End If

            Dim sigData As ITimeSignal() = sig _
                .populates(Of ITimeSignal)(env) _
                .ToArray

            Return New GeneralSignal With {
                .description = title,
                .measureUnit = "n/a",
                .Measures = sigData.X,
                .meta = meta.AsGeneric(Of String)(env),
                .reference = App.NextTempName,
                .Strength = sigData.Y
            }
        End If

        Dim peaks As pipeline = pipeline.TryCreatePipeline(Of Variable)(signals, env)

        If peaks.isError Then
            Return New GeneralSignal With {
                .description = title,
                .Measures = CLRVector.asNumeric(x),
                .measureUnit = "n/a",
                .meta = meta.AsGeneric(Of String)(env),
                .reference = App.NextTempName,
                .Strength = CLRVector.asNumeric(signals)
            }
        Else
            Dim gauss As Variable() = peaks.populates(Of Variable)(env).ToArray
            Dim sig As GeneralSignal = gauss.Compose(x)

            Return sig
        End If
    End Function

    <ExportAPI("gaussian_bin")>
    <RApiReturn(TypeCodes.double)>
    Public Function gaussian_bin(sig As GeneralSignal, Optional max As Integer = 100) As Object
        Return sig.TimeBins(max)
    End Function

    ''' <summary>
    ''' generates a set of the gauss peaks
    ''' </summary>
    ''' <param name="center"></param>
    ''' <param name="height"></param>
    ''' <param name="width"></param>
    ''' <param name="offset"></param>
    ''' <returns></returns>
    <ExportAPI("gaussian_peak")>
    <RApiReturn(GetType(Variable))>
    Public Function gaussian_peak(<RRawVectorArgument> center As Object,
                                  <RRawVectorArgument> height As Object,
                                  <RRawVectorArgument> width As Object,
                                  <RRawVectorArgument> Optional offset As Object = 0) As Object

        Dim center_vec = GetVectorElement.Create(Of Double)(CLRVector.asNumeric(center))
        Dim height_vec = GetVectorElement.Create(Of Double)(CLRVector.asNumeric(height))
        Dim width_vec = GetVectorElement.Create(Of Double)(CLRVector.asNumeric(width))
        Dim offset_vec = GetVectorElement.Create(Of Double)(CLRVector.asNumeric(offset))
        Dim npeaks As Integer = Aggregate arg As GetVectorElement
                                In {center_vec, height_vec, width_vec, offset_vec}
                                Into Max(arg.size)

        Return Enumerable.Range(0, npeaks) _
            .Select(Function(i)
                        Return New Variable(
                            center:=center_vec(i),
                            width:=width_vec(i),
                            height:=height_vec(i),
                            offset:=offset_vec(i)
                        )
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' Fit time/spectrum/other sequential data with a set of gaussians
    ''' by expectation-maximization algoritm.
    ''' </summary>
    ''' <param name="sig"></param>
    ''' <param name="max_peaks"></param>
    ''' <param name="max_loops"></param>
    ''' <param name="eps"></param>
    ''' <param name="gauss_clr">
    ''' returns the clr raw object of the gauss peaks
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("gaussian_fit")>
    <RApiReturn(GetType(RDataframe), GetType(Variable))>
    Public Function gaussian_fit(<RRawVectorArgument>
                                 sig As Object,
                                 Optional max_peaks As Integer = 100,
                                 Optional max_loops As Integer = 10000,
                                 Optional eps As Double = 0.00001,
                                 Optional gauss_clr As Boolean = False,
                                 Optional sine_kernel As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

        Dim opts As New Opts With {
            .maxIterations = max_loops,
            .maxNumber = max_peaks,
            .tolerance = eps
        }
        Dim signal As Double()
        Dim x_axis As Double()

        If TypeOf sig Is GeneralSignal Then
            signal = DirectCast(sig, GeneralSignal).Strength
            x_axis = DirectCast(sig, GeneralSignal).Measures
        ElseIf TypeOf sig Is Signal Then
            signal = DirectCast(sig, Signal).intensities.ToArray
            x_axis = DirectCast(sig, Signal).times.ToArray
        ElseIf TypeOf sig Is RDataframe Then
            signal = DirectCast(sig, RDataframe).getVector(Of Double)("signal", "strength", "intensity", "data", "y")
            x_axis = DirectCast(sig, RDataframe).getVector(Of Double)("time", "x")

            If x_axis.IsNullOrEmpty Then
                x_axis = seq2(1, signal.Length, by:=1)
            End If
        Else
            signal = CLRVector.asNumeric(sig)
            x_axis = seq2(1, signal.Length, by:=1)
        End If

        ' signal = SIMD.Divide.f64_op_divide_f64_scalar(signal, signal.Max)

        Dim gauss As New GaussianFit(opts, sine_kernel)
        Dim peaks = gauss.fit(x_axis, signal, max_peaks)

        If gauss_clr Then
            Return peaks
        Else
            Return gaussPeaks(peaks, New list(("x.axis", x_axis)), env)
        End If
    End Function

    <ExportAPI("resampler")>
    Public Function resampler_f(sig As GeneralSignal, Optional max_dx As Double = Double.MaxValue) As LinearFunction
        Return New LinearFunction With {
            .linear = Resampler.CreateSampler(sig, max_dx)
        }
    End Function

    <ExportAPI("writeCDF")>
    <RApiReturn(GetType(Boolean))>
    Public Function writeCDF(<RRawVectorArgument>
                             signals As Object,
                             file As String,
                             Optional description As String = "no description",
                             Optional env As Environment = Nothing) As Object

        Dim signalData As pipeline = pipeline.TryCreatePipeline(Of GeneralSignal)(signals, env)

        If signalData.isError Then
            Return signalData.getError
        End If

        Return signalData.populates(Of GeneralSignal)(env).WriteCDF(file, description)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="signals"></param>
    ''' <param name="signalId">
    ''' if this parameter is not empty, then it means one of the 
    ''' property data in <see cref="GeneralSignal.meta"/> will be 
    ''' used as signal id. the <see cref="GeneralSignal.reference"/>
    ''' is used as signal id by default.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("signal.matrix")>
    <RApiReturn(GetType(DataSet))>
    Public Function asMatrix(signals As Object, Optional signalId As String = Nothing, Optional env As Environment = Nothing) As Object
        Dim signalStream As pipeline = pipeline.TryCreatePipeline(Of GeneralSignal)(signals, env)

        If signalStream.isError Then
            Return signalStream.getError
        End If

        Dim allSignals As GeneralSignal() = signalStream.populates(Of GeneralSignal)(env).ToArray

        If allSignals.Select(Function(sig) sig.measureUnit).Distinct.Count > 1 Then
            Call env.AddMessage("contains multiple measure unit of the signals...", MSG_TYPES.WRN)
        End If

        Dim allMeasures As String() = allSignals _
            .Select(Function(a)
                        Return a.Measures.Select(Function(b) b.ToString)
                    End Function) _
            .IteratesALL _
            .Distinct _
            .OrderBy(AddressOf Val) _
            .ToArray
        Dim matrix As New List(Of DataSet)

        For Each signal As GeneralSignal In allSignals
            Dim raw As Dictionary(Of String, Double) = signal.Measures _
                .SeqIterator _
                .ToDictionary(Function(a) a.value.ToString,
                              Function(i)
                                  Return signal.Strength(i)
                              End Function)
            Dim row = allMeasures _
                .ToDictionary(Function(a) a,
                              Function(a)
                                  Return If(raw.ContainsKey(a), raw(a), 0)
                              End Function)
            Dim rowObj As New DataSet With {
                .ID = If(signalId.StringEmpty, signal.reference, signal.meta(signalId)),
                .Properties = row
            }

            Call matrix.Add(rowObj)
        Next

        Return matrix.ToArray
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="A">is the height of the curve's peak</param>
    ''' <param name="mu">is the position of the center of the peak</param>
    ''' <param name="sigma">(the standard deviation, sometimes called the Gaussian RMS width) 
    ''' controls the width of the "bell"</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("gaussian")>
    <RApiReturn(TypeCodes.double)>
    Public Function Gaussian(<RRawVectorArgument> x As Object, A#, mu#, sigma#) As Object
        Return Distributions.Gaussian.Gaussian(CLRVector.asNumeric(x), A, mu, sigma)
    End Function
End Module
