﻿#Region "Microsoft.VisualBasic::730e6018b9edd57cc612038c470404a5, Library\graphics\Plot2D\plots.vb"

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

    '   Total Lines: 1264
    '    Code Lines: 992 (78.48%)
    ' Comment Lines: 146 (11.55%)
    '    - Xml Docs: 85.62%
    ' 
    '   Blank Lines: 126 (9.97%)
    '     File Size: 57.89 KB


    ' Module plots
    ' 
    '     Function: barplot, ContourPlot, CreateSerial, doViolinPlot, findNumberVector
    '               measureDataTable, modelWithClass, modelWithoutClass, plot_binBox, plot_categoryBars
    '               plot_corHeatmap, plot_deSolveResult, plot_hclust, plot_heatmap, plotArray
    '               plotContourLayers, plotFormula, plotLinearYFit, plotLmCall, plotODEResult
    '               plotPieChart, PlotPolygon, plotSerials, plotVector, printImage
    '               UpSetPlot
    ' 
    '     Sub: Main, TryGetClassData
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Histogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Contour
Imports Microsoft.VisualBasic.Data.ChartPlots.Fractions
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Data.ChartPlots.Plots
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Scaler
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports R_graphics.Common.Runtime
Imports Rlapack
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports gaussVariable = Microsoft.VisualBasic.Math.SignalProcessing.EmGaussian.Variable
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports Scatter2D = Microsoft.VisualBasic.Data.ChartPlots.Scatter

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
''' chartting plots for R#
''' </summary>
<Package("charts", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
<RTypeExport("contours", GetType(ContourLayer()))>
Module plots

    <RInitialize>
    Sub Main()
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of SerialData)(Function(line) line.ToString)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of GraphicsData)(AddressOf printImage)

        Call REnv.Internal.generic.add("plot", GetType(DeclareLambdaFunction), AddressOf plotFormula)
        Call REnv.Internal.generic.add("plot", GetType(ODEOutput), AddressOf plotODEResult)
        Call REnv.Internal.generic.add("plot", GetType(ODEsOut), AddressOf plot_deSolveResult)
        Call REnv.Internal.generic.add("plot", GetType(SerialData()), AddressOf plotSerials)
        Call REnv.Internal.generic.add("plot", GetType(SerialData), AddressOf plotSerials)
        Call REnv.Internal.generic.add("plot", GetType(DataBinBox(Of Double)()), AddressOf plot_binBox)
        Call REnv.Internal.generic.add("plot", GetType(Dictionary(Of String, Double)), AddressOf plot_categoryBars)
        Call REnv.Internal.generic.add("plot", GetType(DistanceMatrix), AddressOf plot_corHeatmap)
        Call REnv.Internal.generic.add("plot", GetType(Cluster), AddressOf plot_hclust)
        Call REnv.Internal.generic.add("plot", GetType(vector), AddressOf plotVector)
        Call REnv.Internal.generic.add("plot", GetType(Double()), AddressOf plotArray)
        Call REnv.Internal.generic.add("plot", GetType(Single()), AddressOf plotArray)
        Call REnv.Internal.generic.add("plot", GetType(Integer()), AddressOf plotArray)
        Call REnv.Internal.generic.add("plot", GetType(Long()), AddressOf plotArray)
        Call REnv.Internal.generic.add("plot", GetType(ContourLayer()), AddressOf plotContourLayers)
        Call REnv.Internal.generic.add("plot", GetType(Rdataframe), AddressOf plot_heatmap)

        Call REnv.Internal.generic.add("plot", GetType(WeightedFit), AddressOf plotLinearYFit)
        Call REnv.Internal.generic.add("plot", GetType(IFitted), AddressOf plotLinearYFit)
        Call REnv.Internal.generic.add("plot", GetType(lmCall), AddressOf plotLmCall)

        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(MeasureData()), AddressOf measureDataTable)
    End Sub

    <RGenericOverloads("plot")>
    Private Function plot_heatmap(m As Rdataframe, args As list, env As Environment) As Object
        Dim cols = m.colnames
        Dim row_scale As Boolean = CLRVector.asLogical(args.getBySynonyms("row_scale", "row.scale")).ElementAtOrDefault(0, [default]:=False)
        Dim mainTitle As String = CLRVector.asCharacter(args.getBySynonyms("title")).DefaultFirst("heatmap")
        Dim dataset As DataSet() = m.forEachRow _
            .Select(Function(a)
                        Dim fields As New Dictionary(Of String, Double)
                        Dim vec As Double() = CLRVector.asNumeric(a.value)

                        If row_scale Then
                            vec = vec.Z
                        End If

                        For i As Integer = 0 To cols.Length - 1
                            Call fields.Add(cols(i), vec(i))
                        Next

                        Return New DataSet With {
                            .ID = a.name,
                            .Properties = fields
                        }
                    End Function) _
            .ToArray
        Dim driver As Drivers = env.getDriver
        Dim dpi As Integer = graphicsPipeline.getDpi(args.slots, env, [default]:=100)
        Dim colors As String = CLRVector.safeCharacters(args.getBySynonyms("colors", "colorset")).ElementAtOrDefault(0, ColorBrewer.DivergingSchemes.RdYlBu11)
        Dim size = graphicsPipeline.getSize(args.slots, env, New SizeF(3000, 3000))

        Return HeatMap.Plot(dataset,
                            mapName:=colors,
                            size:=$"{size.Width},{size.Height}",
                            dendrogramLayout:="600,300",
                            mainTitle:=mainTitle,
                            ppi:=dpi,
                            driver:=driver)
    End Function

    Private Function printImage(img As GraphicsData) As String
        Dim sb As New StringBuilder

        If img.Driver = Drivers.GDI Then
            Call sb.AppendLine("Raster image data:")
        Else
            Call sb.AppendLine("Vector image data:")
        End If

        Call sb.AppendLine($" -> driver: {img.Driver.Description}")
        Call sb.AppendLine($" -> size: [{img.Width}x{img.Height}]")

        Return sb.ToString
    End Function

    Public Function plotLmCall(lm As lmCall, args As list, env As Environment) As Object
        Return plotLinearYFit(lm.lm, args, env)
    End Function

    Public Function plotLinearYFit(fit As IFitted, args As list, env As Environment) As Object
        Dim size As String = InteropArgumentHelper.getSize(args!size, env, "1600,1100")
        Dim gridFill As String = RColorPalette.getColor(args("grid.fill"), "rgb(245,245,245)")
        Dim showLegend As Boolean = args.getValue("show.legend", env, True)
        Dim showYFit As Boolean = args.getValue("show.yfit", env, True)
        Dim padding As String = InteropArgumentHelper.getPadding(args!padding, "padding: 150px 100px 150px 200px")
        Dim xlab As String = args.getValue("xlab", env, "X")
        Dim ylab As String = args.getValue("ylab", env, "Y")
        Dim PredictsLabel As String = args.getValue("predicts_lab", env, "Predicts")
        Dim ReferenceLabel As String = args.getValue("reference_lab", env, "Standard Reference")
        Dim LinearLabel As String = args.getValue("linear_lab", env, "Linear")
        Dim SamplesLabel As String = args.getValue("samples_lab", env, "Samples")
        Dim tickX As String = args.getValue("xtick", env, "F0")
        Dim tickY As String = args.getValue("ytick", env, "G2")
        Dim absolute_positive As Boolean = args.getValue("absolute_positive", env, False)

        Return RegressionPlot.Plot(fit, size:=size,
            gridFill:=gridFill,
            showLegend:=showLegend,
            showYFitPoints:=showYFit,
            showErrorBand:=False,
            title:=args.getValue(Of String)("main", env, Nothing),
            margin:=padding,
            factorFormat:="G4",
            xAxisTickFormat:=tickX,
            yAxisTickFormat:=tickY,
            xLabel:=xlab,
            yLabel:=ylab,
            pointLabelFontCSS:=CSSFont.Win10Normal,
            PredictsLabel:=PredictsLabel,
            ReferenceLabel:=ReferenceLabel,
            LinearLabel:=LinearLabel,
            SamplesLabel:=SamplesLabel,
            absolute_positive:=absolute_positive
        )
    End Function

    Private Function measureDataTable(data As MeasureData(), args As list, env As Environment) As Rdataframe
        Dim x = data.Select(Function(p) p.X).ToArray
        Dim y = data.Select(Function(p) p.Y).ToArray
        Dim Z = data.Select(Function(p) p.Z).ToArray

        Return New Rdataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"x", x},
                {"y", y},
                {"data", Z}
            }
        }
    End Function

    Public Function plotContourLayers(contours As ContourLayer(), args As list, env As Environment) As Object
        Return ContourPlot(contours, colorSet:=args!colorSet, args:=args, env:=env)
    End Function

    Public Function plotArray(vec As Array, args As list, env As Environment) As Object
        Dim x As Double() = CLRVector.asNumeric(vec)
        Dim findY = args.findNumberVector(size:=x.Length, env)

        If findY Like GetType(Message) Then
            Return findY.TryCast(Of Message)
        End If

        Dim y As Double() = findY?.TryCast(Of Double())
        Dim ptSize As Single = args.getValue({"point_size", "point.size"}, env, 15)
        Dim classList As String() = args.getValue(Of String())("class", env, Nothing)
        Dim reverse As Boolean = args.getValue("reverse", env, False)
        Dim drawLine As Boolean = (y Is Nothing) OrElse CLRVector.asLogical(args!line).DefaultFirst(False)
        Dim shape As LegendStyles = args.getValue("shape", env, "Circle").ParseLegendStyle

        ' only x, no y(y is nothing): draw line
        ' x,y: draw line for scatter points when set line parameter to TRUE
        ' plot(x,y, line = TRUE);
        ' plot(x,y)  # default is draw scatter plot

        args.slots!line = drawLine

        If Not classList.IsNullOrEmpty Then
            Return modelWithClass(x, y, ptSize, classList, args, reverse, shape, env)
        Else
            Return modelWithoutClass(x, y, ptSize, args, reverse, shape, env)
        End If
    End Function

    Private Function modelWithClass(x As Double(), y As Double(),
                                    ptSize As Single,
                                    classList As String(),
                                    args As list,
                                    reverse As Boolean,
                                    shape As LegendStyles,
                                    env As Environment) As Object

        Dim uniqClass As String() = classList.Distinct.ToArray
        ' make the category colorset shuffle?
        ' this may be usefull when too much category class to assign
        Dim shuffles As Boolean = CLRVector.asLogical(args.getBySynonyms("shuffles", "colorset.shuffles")).DefaultFirst([default]:=False)
        Dim colors As Dictionary(Of String, Color) = uniqClass.CreateColorMaps(
            args.getBySynonyms("colorSet", "colors", "color_set"), env,
            shuffles:=shuffles)
        Dim classSerials As New Dictionary(Of String, List(Of PointData))

        If classList.Length <> x.Length Then
            If env.globalEnvironment.Rscript.strict Then
                Return RInternal.debug.stop({
                    $"the size of the point class ({classList.Length}) is not equals to the size of the given data point ({x.Length})!",
                    $"class_size: {classList.Length}",
                    $"point_size: {x.Length}"
                }, env)
            Else
                env.AddMessage($"the size of the point class ({classList.Length}) is not equals to the size of the given data point ({x.Length})!", MSG_TYPES.WRN)
            End If
        End If

        For Each label As String In uniqClass
            classSerials(label) = New List(Of PointData)
        Next

        Dim point As PointData

        If y Is Nothing Then
            For i As Integer = 0 To x.Length - 1
                point = New PointData(classSerials(classList(i)).Count + 1, x(i))
                classSerials(classList(i)).Add(point)
            Next
        Else
            Dim maxy As Double = y.Max

            For i As Integer = 0 To x.Length - 1
                point = New PointData(x(i), If(reverse, maxy - y(i), y(i)))
                classSerials(classList(i)).Add(point)
            Next
        End If

        Dim lines As SerialData() = classSerials _
            .Where(Function(list)
                       Return list.Value.Count > 0
                   End Function) _
            .Select(Function(tuple)
                        Return New SerialData With {
                            .pts = tuple.Value.ToArray,
                            .color = colors(tuple.Key),
                            .pointSize = ptSize,
                            .shape = shape,
                            .title = tuple.Key,
                            .width = 5,
                            .lineType = DashStyle.Custom
                        }
                    End Function) _
            .ToArray

        Return plotSerials(lines, args, env)
    End Function

    Private Function modelWithoutClass(x As Double(), y As Double(),
                                       ptSize As Single,
                                       args As list,
                                       reverse As Boolean,
                                       shape As LegendStyles,
                                       env As Environment) As Object
        Dim line As SerialData

        If y Is Nothing Then
            line = New SerialData() With {
                .pts = x _
                    .SeqIterator _
                    .Select(Function(i)
                                Return New PointData(i.i, i.value)
                            End Function) _
                    .ToArray,
                .pointSize = ptSize,
                .title = args.getValue("title", env, "data"),
                .shape = shape,
                .color = args.getValue("color", env, "black").TranslateColor
            }
        Else
            Dim colorSet As Func(Of Integer, String)
            Dim maxy As Double = y.Max

            If args.hasName("colorSet") AndAlso Not args!colorSet Is Nothing Then
                Dim colorsMap As String() = RColorPalette.getColors(args!colorSet, x.Length, Nothing)

                colorSet = Function(i) colorsMap(i)
            Else
                colorSet = Function() Nothing
            End If

            line = New SerialData() With {
                .pts = x _
                    .Select(Function(xi, i)
                                Return New PointData(xi, If(reverse, maxy - y(i), y(i))) With {
                                    .color = colorSet(i)
                                }
                            End Function) _
                    .ToArray,
                .pointSize = ptSize,
                .title = args.getValue("title", env, "x ~ y"),
                .shape = shape,
                .color = args.getValue("color", env, "black").TranslateColor
            }
        End If

        If args.hasName("fit") Then
            Dim fit As Object = args!fit
            Dim lines As New List(Of SerialData) From {line}
            Dim x_axis As Double() = line.x
            Dim color = args.getValue("color", env, "black").TranslateColor
            Dim maxy As Double = y.Max

            If TypeOf fit Is gaussVariable() Then
                For Each peak As gaussVariable In DirectCast(fit, gaussVariable())
                    If peak.width = 0 OrElse
                        peak.height = 0.0 OrElse
                        peak.center.IsNaNImaginary OrElse
                        peak.height.IsNaNImaginary OrElse
                        peak.width.IsNaNImaginary OrElse
                        peak.offset.IsNaNImaginary Then

                        Continue For
                    End If

                    line = New SerialData With {
                        .pointSize = ptSize,
                        .title = peak.ToString,
                        .shape = shape,
                        .color = color,
                        .pts = x_axis _
                            .Select(Function(xi)
                                        Return New PointData(xi, If(reverse, maxy - peak.gaussian(xi), peak.gaussian(xi)))
                                    End Function) _
                            .ToArray
                    }
                    lines.Add(line)
                Next
            End If

            Return plotSerials(lines, args, env)
        Else
            Dim fit_names As String() = args.getNames _
                .Where(Function(s)
                           Return TypeOf args(s) Is list AndAlso DirectCast(args(s), list).hasNames("x", "y")
                       End Function) _
                .ToArray
            Dim lines As New List(Of SerialData) From {line}
            Dim color = args.getValue("color", env, "black").TranslateColor

            ' plot(data)  only x, no y(means y is nothing)
            Dim maxy As Double = If(y.IsNullOrEmpty, x.Max, y.Max)

            For Each name As String In fit_names
                Dim serial_data As list = args(name)
                Dim x_axis As Double() = CLRVector.asNumeric(serial_data!x)
                Dim y_axis As Double() = CLRVector.asNumeric(serial_data!y)
                Dim color_line As Color = color

                If serial_data.hasName("color") Then
                    color_line = serial_data.getValue("color", env, "black").TranslateColor
                End If

                line = New SerialData With {
                    .pointSize = ptSize,
                    .title = name,
                    .shape = shape,
                    .color = color_line,
                    .pts = x_axis _
                        .Select(Function(xi, i)
                                    Return New PointData(xi, If(reverse, maxy - y_axis(i), y_axis(i)))
                                End Function) _
                        .ToArray
                }
                lines.Add(line)
            Next

            Return plotSerials(lines, args, env)
        End If
    End Function

    ''' <summary>
    ''' A helper function for find the y vector
    ''' </summary>
    ''' <param name="args"></param>
    ''' <param name="size"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <Extension>
    Private Function findNumberVector(args As list, size As Integer, env As Environment) As [Variant](Of Double(), Message)
        For Each value As Object In From obj As Object
                                    In args.data
                                    Where Not TypeOf obj Is String

            If TypeOf value Is ValueAssignExpression Then
                Dim assign As ValueAssignExpression = value

                If assign.isByRef Then
                    Continue For
                End If
                If assign.symbolSize > 1 Then
                    Continue For
                End If

                If TypeOf assign.targetSymbols(0) Is SymbolReference Then
                    Dim symbol As SymbolReference = assign.targetSymbols(0)

                    If Not symbol.symbol.TextEquals("y") Then
                        Continue For
                    Else
                        value = assign.value
                    End If
                Else
                    Continue For
                End If
            End If

            If TypeOf value Is Expression Then
                value = DirectCast(value, Expression).Evaluate(env)
            End If

            If Program.isException(value) Then
                Return DirectCast(value, Message)
            End If

            value = CLRVector.asNumeric(value)

            If DirectCast(value, Double()).Length = size Then
                Return DirectCast(value, Double())
            End If
        Next

        Return Nothing
    End Function

    Public Function plotVector(x As vector, args As list, env As Environment) As Object
        Dim array As Array = REnv.TryCastGenericArray(x.data, env)

        If TypeOf array Is SerialData() Then
            Return plotSerials(array, args, env)
        Else
            Return plotArray(CLRVector.asNumeric(array), args, env)
        End If
    End Function

    Private Sub TryGetClassData(list As Object, ByRef classes As ColorClass(), ByRef classinfo As Dictionary(Of String, String))
        If list Is Nothing Then
            classes = Nothing '
            classinfo = Nothing
        End If

        If TypeOf list Is list Then
            classinfo = DirectCast(list, list).slots _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return any.ToString(a.Value)
                              End Function)
        ElseIf TypeOf list Is Dictionary(Of String, String) Then
            classinfo = list
        ElseIf list.GetType.ImplementInterface(GetType(IDictionary)) Then
            Dim hash = DirectCast(list, IDictionary)

            classinfo = New Dictionary(Of String, String)

            For Each key As Object In hash.Keys
                classinfo(key.ToString) = any.ToString(hash(key))
            Next
        End If

        If classinfo.Values.All(Function(str)
                                    Return str.IsPattern("\d+") OrElse str.IsPattern("((class)|(cluster)|(group)).*\d+")
                                End Function) Then
            ' is class tag
            ' assign color by the cluster tag
            Dim colors As New CategoryColorProfile(classinfo.Values.Distinct, "paper")

            For Each key As String In classinfo.Keys.ToArray
                classinfo(key) = colors.GetColor(classinfo(key)).ToHtmlColor
            Next
        Else
            classinfo = classinfo _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return a.Value.TranslateColor.ToHtmlColor
                              End Function)
        End If

        classes = classinfo.Values _
            .Distinct _
            .Select(Function(colorName, i)
                        Return New ColorClass With {
                            .color = colorName,
                            .factor = i,
                            .name = colorName
                        }
                    End Function) _
            .ToArray
    End Sub

    Public Function plot_hclust(cluster As Cluster, args As list, env As Environment) As Object
        Dim size$ = InteropArgumentHelper.getSize(args.getByName("size"), env)
        Dim padding$ = InteropArgumentHelper.getPadding(args.getByName("padding"))
        Dim labelStyle$ = InteropArgumentHelper.getFontCSS(args.getByName("label"), CSSFont.PlotLabelNormal)
        Dim linkStroke$ = InteropArgumentHelper.getStrokePenCSS(args.getByName("links"), Stroke.AxisGridStroke)
        Dim tickStyle$ = InteropArgumentHelper.getFontCSS(args.getByName("ticks"), CSSFont.PlotLabelNormal)
        Dim axisStroke$ = InteropArgumentHelper.getStrokePenCSS(args.getByName("axis"), Stroke.AxisStroke)
        Dim axisFormat$ = args.getValue("axis.format", env, "F1")
        Dim ptSize As Double = args.getValue(Of Double)("pt.size", env, 10)
        Dim bg$ = RColorPalette.getColor(args.getByName("background"), "white")
        Dim pointColor$ = RColorPalette.getColor(args.getByName("pt.color"), "black")
        Dim driver As Drivers = env.getDriver
        Dim dpi As Integer = graphicsPipeline.getDpi(args.slots, env, 300)
        Dim classes As ColorClass() = Nothing
        Dim classinfo As Dictionary(Of String, String) = Nothing

        If args.hasName("class") Then
            Call TryGetClassData(args!class, classes, classinfo)
        End If

        Dim theme As New Theme With {
            .padding = padding,
            .tagCSS = labelStyle,
            .gridStrokeX = linkStroke,
            .gridStrokeY = linkStroke,
            .axisTickCSS = tickStyle,
            .axisStroke = axisStroke,
            .pointSize = ptSize,
            .background = bg,
            .XaxisTickFormat = axisFormat
        }

        Return New DendrogramPanelV2(
            hist:=cluster,
            theme:=theme,
            classes:=classes,
            classinfo:=classinfo,
            pointColor:=pointColor
        ).Plot(size, dpi, driver:=driver)
    End Function

    ''' <summary>
    ''' 绘制关联热图
    ''' </summary>
    ''' <param name="dist"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Function plot_corHeatmap(dist As DistanceMatrix, args As list, env As Environment) As Object
        Dim title$ = args.GetString("title", "Correlations")
        Dim bg$ = RColorPalette.getColor(args!bg, "white")
        Dim size = InteropArgumentHelper.getSize(args!size, env, "3600,3000")
        Dim padding$ = InteropArgumentHelper.getPadding(args!padding, "padding: 300px 150px 150px 100px;")
        Dim driver As Drivers = args.GetString("driver", "default").DoCall(AddressOf g.ParseDriverEnumValue)
        Dim colorSet = args.GetString("colors", ColorBrewer.DivergingSchemes.RdBu11)
        Dim fixedSize = args.getValue(Of Boolean)("fixed_size", env, False)
        Dim titleFont$ = InteropArgumentHelper.getFontCSS(args!mainCSS, CSSFont.Win7VeryLarge)
        Dim labelFont$ = InteropArgumentHelper.getFontCSS(args!labelCSS, CSSFont.Win7Normal)
        Dim legendTitleFont$ = InteropArgumentHelper.getFontCSS(args!legendTitleCSS, CSSFont.Win7LargeBold)

        Return CorrelationTriangle.Plot(
            data:=dist,
            size:=size,
            bg:=bg,
            padding:=padding,
            mainTitle:=title,
            drawGrid:=True,
            driver:=driver,
            mapName:=colorSet,
            variantSize:=Not fixedSize,
            titleFont:=titleFont,
            rowLabelFontStyle:=labelFont,
            legendFont:=legendTitleFont
        )
    End Function

    Public Function plot_categoryBars(data As Dictionary(Of String, Double), args As list, env As Environment) As Object
        Dim title$ = args.GetString("title", "Histogram Plot")
        Dim xlab$ = args.GetString("x.lab", "X")
        Dim ylab$ = args.GetString("y.lab", "Y")
        Dim padding$ = InteropArgumentHelper.getPadding(args!padding)
        Dim serials As BarDataSample() = data _
            .Select(Function(bar)
                        Return New BarDataSample With {
                            .data = {bar.Value},
                            .tag = bar.Key
                        }
                    End Function) _
            .ToArray
        Dim plotData As New BarDataGroup With {
            .Samples = serials,
            .Serials = {New NamedValue(Of Color)(title, Color.SkyBlue)}
        }

        Return plotData.Plot(padding:=padding)
    End Function

    Public Function plot_binBox(data As DataBinBox(Of Double)(), args As list, env As Environment) As Object
        Dim step! = CSng(REnv.getFirst(args!steps))
        Dim title$ = args.GetString("title", "Histogram Plot")
        Dim xlab$ = args.GetString("x.lab", "X")
        Dim ylab$ = args.GetString("y.lab", "Y")
        Dim padding$ = InteropArgumentHelper.getPadding(args!padding, [default]:="padding: 10% 15% 15% 10%;", env)
        Dim dpi As Integer = graphicsPipeline.getDpi(args.slots, env, [default]:=100)
        Dim highlightRange As Double() = CLRVector.asNumeric(args.getBySynonyms("highlights", "highlight.range"))
        Dim highlightColor = graphicsPipeline.GetRawColor(args.getBySynonyms("highlight.color", "highlights.color"), [default]:="red")
        Dim highlightTitle As String = args.getValue("highlights.title", env, "highlights " & highlightColor.ToHtmlColor)
        Dim highlights As NamedValue(Of DoubleRange)() = Nothing
        Dim size = graphicsPipeline.getSize(args.slots, env, New Size(1600, 1200))

        If highlightRange.TryCount >= 2 Then
            highlights = {
                New NamedValue(Of DoubleRange)(highlightTitle, New DoubleRange(highlightRange), highlightColor.ToHtmlColor)
            }
        End If
        If [step] <= 0 Then
            ' guess step value from binbox width
            [step] = data _
                .Select(Function(bin)
                            Return bin.Raw.Range.Length
                        End Function) _
                .Average
        End If

        Return data.HistogramPlot(
            serialsTitle:=title,
            xLabel:=xlab,
            yLabel:=ylab,
            padding:=padding,
            dpi:=dpi,
            driver:=env.getDriver,
            size:=$"{size.Width},{size.Height}",
            highlights:=highlights
        )
    End Function

    ''' <summary>
    ''' ### Pie Charts
    ''' 
    ''' Draw a pie chart.
    ''' </summary>
    ''' <param name="x">a vector Of non-negative numerical quantities. The values In x are displayed As the areas Of pie slices.</param>
    ''' <param name="d3"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Pie charts are a very bad way of displaying information. The eye is good at judging linear measures and 
    ''' bad at judging relative areas. A bar chart or dot chart is a preferable way of displaying this type of 
    ''' data.
    ''' 
    ''' Cleveland (1985), page 264 “Data that can be shown by pie charts always can be shown by a dot chart. 
    ''' This means that judgements of position along a common scale can be made instead of the less accurate angle 
    ''' judgements.” This statement Is based on the empirical investigations of Cleveland And McGill as well 
    ''' as investigations by perceptual psychologists.
    ''' </remarks>
    <ExportAPI("pie")>
    Public Function plotPieChart(<RRawVectorArgument> x As Object,
                                 Optional schema As Object = "Paired:c12",
                                 Optional d3 As Boolean = False,
                                 Optional camera As Camera = Nothing,
                                 <RRawVectorArgument>
                                 Optional size As Object = "1600,1200",
                                 Optional env As Environment = Nothing) As Object

        Dim data As New List(Of FractionData)
        Dim colorSet As String = RColorPalette.getColorSet(schema, "Paired:c12")
        Dim colors As LoopArray(Of Color) = Designer.GetColors(colorSet)

        If x Is Nothing Then
            Return RInternal.debug.stop("the requred x data object can not be nothing!", env)
        ElseIf TypeOf x Is list Then
            ' a collection of [name => number]
            For Each tag As NamedValue(Of Object) In DirectCast(x, list).namedValues
                data += New FractionData With {
                    .Name = tag.Name,
                    .Value = CLRVector.asNumeric(tag.Value).GetValue(Scan0),
                    .Color = ++colors
                }
            Next
        ElseIf TypeOf x Is vector Then
            Dim v As vector = DirectCast(x, vector)

            If v.elementType.is_numeric Then
                Dim names As String() = DirectCast(x, vector).getNames
                Dim vec As Double() = CLRVector.asNumeric(x)

                For i As Integer = 0 To names.Length - 1
                    data += New FractionData With {
                        .Name = names(i),
                        .Color = ++colors,
                        .Value = vec(i)
                    }
                Next
            Else
                Dim chars As String() = CLRVector.asCharacter(x)
                Dim factors = chars.GroupBy(Function(s) s)

                For Each factor As IGrouping(Of String, String) In factors
                    data += New FractionData With {
                        .Name = factor.Key,
                        .Color = ++colors,
                        .Value = factor.Count
                    }
                Next
            End If
        Else
            Return Message.InCompatibleType(GetType(vector), x.GetType, env)
        End If

        If d3 Then
            If camera Is Nothing Then
                camera = New Camera With {
                    .screen = InteropArgumentHelper.getSize(size, env).SizeParser,
                    .viewDistance = 10000
                }
            End If

            ' 3D
            Return data.Plot3D(camera)
        Else
            ' 2D
            Return PieChart.Plot(
                data:=data,
                size:=InteropArgumentHelper.getSize(size, env)
            )
        End If
    End Function

    ''' <summary>
    ''' ### Bar Plots
    ''' 
    ''' Creates a bar plot with vertical or horizontal bars.
    ''' </summary>
    ''' <param name="height">
    ''' either a vector or matrix of values describing the bars which make up the plot. 
    ''' If height is a vector, the plot consists of a sequence of rectangular bars with 
    ''' heights given by the values in the vector. If height is a matrix and beside is 
    ''' FALSE then each bar of the plot corresponds to a column of height, with the 
    ''' values in the column giving the heights of stacked sub-bars making up the bar. 
    ''' If height is a matrix and beside is TRUE, then the values in each column are 
    ''' juxtaposed rather than stacked.
    ''' </param>
    ''' <param name="category$"></param>
    ''' <param name="value$"></param>
    ''' <param name="color$"></param>
    ''' <param name="min$"></param>
    ''' <param name="max$"></param>
    ''' <param name="title">overall And sub title for the plot.</param>
    ''' <param name="xlab">a label for the x axis.</param>
    ''' <param name="ylab">a label For the y axis.</param>
    ''' <param name="bg"></param>
    ''' <param name="size"></param>
    ''' <param name="padding"></param>
    ''' <param name="show_grid"></param>
    ''' <param name="show_legend"></param>
    ''' <returns>
    ''' the plot image
    ''' </returns>
    <ExportAPI("barplot")>
    Public Function barplot(height As Rdataframe,
                            Optional category$ = "item",
                            Optional value$ = "value",
                            Optional color$ = "color",
                            Optional min$ = "min",
                            Optional max$ = "max",
                            Optional title$ = "Histogram Plot",
                            Optional xlab$ = "X",
                            Optional ylab$ = "Y",
                            Optional bg As Object = "white",
                            <RRawVectorArgument> Optional size As Object = "1920,1080",
                            <RRawVectorArgument> Optional padding As Object = g.DefaultPadding,
                            Optional show_grid As Boolean = True,
                            Optional show_legend As Boolean = True,
                            Optional env As Environment = Nothing) As Object

        Dim items As String() = height.columns(category)
        Dim values As Double() = CLRVector.asNumeric(height.columns(value))
        Dim minX As Double() = CLRVector.asNumeric(height.columns(min))
        Dim maxX As Double() = CLRVector.asNumeric(height.columns(max))
        Dim colors As String() = height.columns(color) _
            .AsObjectEnumerator _
            .Select(AddressOf RColorPalette.getColor) _
            .ToArray
        Dim s As HistProfile() = items _
            .SeqIterator _
            .Select(Function(i)
                        Dim histLegend As New LegendObject With {
                            .color = colors(i),
                            .fontstyle = CSSFont.Win7LargerBold,
                            .style = LegendStyles.Rectangle,
                            .title = i.value
                        }
                        Dim x As Double = values(i)
                        Dim bar As New HistogramData With {
                            .pointY = x,
                            .y = x,
                            .x1 = minX(i.i),
                            .x2 = maxX(i.i)
                        }

                        Return New HistProfile(histLegend, {bar})
                    End Function) _
            .ToArray
        Dim group As New HistogramGroup With {
            .Samples = s,
            .Serials = s _
                .Select(Function(a) a.SerialData) _
                .ToArray
        }
        Dim bgColor As String = RColorPalette.getColor(bg, "white")

        Return group.Plot(
            bg:=bgColor,
            size:=InteropArgumentHelper.getSize(size, env),
            padding:=InteropArgumentHelper.getPadding(padding),
            showGrid:=show_grid,
            xlabel:=xlab,
            Ylabel:=ylab,
            title:=title,
            showLegend:=show_legend
        )
    End Function

    Public Function plot_deSolveResult(desolve As ODEsOut, args As list, env As Environment) As Object
        Dim vector As list = args!vector
        Dim camera As Camera = args!camera
        Dim color As Color = RColorPalette.getColor(args!color, "black").TranslateColor
        Dim bg$ = RColorPalette.getColor(args!bg, "white")
        Dim title As String = any.ToString(getFirst(args!title), "Plot deSolve")
        Dim x As Double() = desolve.y(CStr(vector!x)).value
        Dim y As Double() = desolve.y(CStr(vector!y)).value
        Dim z As Double() = desolve.y(CStr(vector!z)).value
        Dim data As New Serial3D With {
            .Color = color,
            .PointSize = 5,
            .Shape = LegendStyles.Circle,
            .Title = title,
            .Points = x _
                .Select(Function(xi, i)
                            Return New Point3D(xi, y(i), z(i))
                        End Function) _
                .Select(Function(pt3d)
                            Return New NamedValue(Of Point3D) With {
                                .Name = Nothing,
                                .Value = pt3d
                            }
                        End Function) _
                .ToArray
        }

        Return {data}.Plot(camera, bg:=bg, showLegend:=False)
    End Function

    Public Function plotODEResult(math As ODEOutput, args As list, env As Environment) As Object
        Return math.Plot(size:=InteropArgumentHelper.getSize(args!size, env))
    End Function

    ''' <summary>
    ''' plot the math function
    ''' </summary>
    ''' <param name="math">y = f(x)</param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Function plotFormula(math As DeclareLambdaFunction, args As list, env As Environment) As Object
        If Not args.hasName("x") Then
            Return REnv.Internal.debug.stop("Missing parameter 'x' for plot function!", env)
        End If

        Dim fx As Func(Of Double, Double) = math.CreateLambda(Of Double, Double)(env)
        Dim x As Double() = CLRVector.asNumeric(args!x)
        Dim points As PointF() = x.Select(Function(xi) New PointF(xi, fx(xi))).ToArray
        Dim driver As Drivers = imageDriverHandler.getDriver(env)

        Return points.Plot(
            size:=InteropArgumentHelper.getSize(args!size, env).SizeParser,
            title:=math.ToString,
            padding:=InteropArgumentHelper.getPadding(args!padding),
            gridFill:=RColorPalette.getColor(args("grid.fill"), "rgb(250,250,250)"),
            driver:=driver
        )
    End Function

    ''' <summary>
    ''' plot scatter 2d serial data
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Function plotSerials(data As Object, args As list, env As Environment) As Object
        If TypeOf data Is SerialData Then
            data = {DirectCast(data, SerialData)}
        End If

        Dim serials As SerialData() = DirectCast(data, IEnumerable(Of SerialData)).ToArray
        Dim size As String = InteropArgumentHelper.getSize(args!size, env, [default]:="2100,1600")
        Dim margin = InteropArgumentHelper.getPadding(args!padding, [default]:="padding: 5% 10% 10% 15%;", env:=env)
        Dim title As String = any.ToString(getFirst(args!title), "Scatter Plot")
        Dim spline As Splines = args.getValue(Of Splines)("interplot", env, Splines.None)
        Dim xlim As Double() = CLRVector.asNumeric(args("xlim"))
        Dim ylim As Double() = CLRVector.asNumeric(args("ylim"))
        Dim absoluteScale As Boolean = args.getValue("absolute_scale", env, False)
        Dim driver As Drivers = imageDriverHandler.getDriver(env)
        Dim dpi As Integer = graphicsPipeline.getDpi(args.slots, env, [default]:=100)
        Dim showLegend As Boolean = args.getValue(Of Boolean)({"showLegend", "legend", "legend.show"}, env, [default]:=True)
        Dim showAxis As Boolean = args.getValue(Of Boolean)({"show.axis", "axis.show"}, env, [default]:=True)

        If args.CheckGraphicsDeviceExists Then
            ' draw on current graphics context
            Dim dev As graphicsDevice = R_graphics.Common.Runtime.graphics.curDev
            Dim padding As Padding = InteropArgumentHelper.getPadding(dev.getArgumentValue("padding", args))
            Dim canvas As New GraphicsRegion(dev.g.Size, padding)

            Call Scatter2D.Plot(
                serials, dev.g, canvas,
                Xlabel:=args.getValue("x.lab", env, "X"),
                Ylabel:=args.getValue("y.lab", env, "Y"),
                drawLine:=getFirst(CLRVector.asLogical(args!line)),
                legendBgFill:=RColorPalette.getColor(args!legendBgFill, Nothing),
                legendFontCSS:=InteropArgumentHelper.getFontCSS(args("legend.font")),
                showLegend:=showLegend,
                title:=title,
                legendSplit:=args.getValue(Of Integer)("legend.block", env),
                ablines:=args.getValue(Of Line())("abline", env),
                hullConvexList:=args.getValue(Of String())("convexHull", env),
                XtickFormat:=args.getValue("x.format", env, "F2"),
                YtickFormat:=args.getValue("y.format", env, "F2"),
                interplot:=spline,
                axisLabelCSS:=args.getValue("axis.cex", env, CSSFont.Win7VeryLarge),
                gridFill:=RColorPalette.getColor(If(args("grid.fill"), args("fill")), "lightgray", env),
                xlim:=xlim,
                ylim:=ylim,
                XaxisAbsoluteScalling:=absoluteScale,
                YaxisAbsoluteScalling:=absoluteScale,
                drawAxis:=showAxis
            )

            Return Nothing
        Else
            Return Scatter2D.Plot(
                c:=serials,
                size:=size, padding:=margin,
                Xlabel:=args.getValue("x.lab", env, "X"),
                Ylabel:=args.getValue("y.lab", env, "Y"),
                drawLine:=getFirst(CLRVector.asLogical(args!line)),
                legendBgFill:=RColorPalette.getColor(args!legendBgFill, Nothing),
                legendFontCSS:=InteropArgumentHelper.getFontCSS(args("legend.font")),
                showLegend:=showLegend,
                title:=title,
                legendSplit:=args.getValue(Of Integer)("legend.block", env),
                ablines:=args.getValue(Of Line())("abline", env),
                hullConvexList:=args.getValue(Of String())("convexHull", env),
                XtickFormat:=args.getValue("x.format", env, "F2"),
                YtickFormat:=args.getValue("y.format", env, "F2"),
                interplot:=spline,
                axisLabelCSS:=args.getValue("axis.cex", env, CSSFont.Win7VeryLarge),
                gridFill:=RColorPalette.getColor(If(args("grid.fill"), args("fill")), "lightgray", env),
                xlim:=xlim,
                ylim:=ylim,
                XaxisAbsoluteScalling:=absoluteScale,
                YaxisAbsoluteScalling:=absoluteScale,
                dpi:=dpi,
                driver:=env.getDriver,
                drawAxis:=showAxis
            )
        End If
    End Function

    <ExportAPI("upset")>
    Public Function UpSetPlot(upset As list, Optional env As Environment = Nothing) As Object
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' create a new serial for scatter plot
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="name$"></param>
    ''' <param name="color"></param>
    ''' <returns></returns>
    <ExportAPI("serial")>
    Public Function CreateSerial(x As Array, y As Array,
                                 Optional name$ = "data serial",
                                 Optional color As Object = "black",
                                 Optional alpha As Integer = 255,
                                 Optional ptSize As Integer = 5) As SerialData

        Dim px As Double() = CLRVector.asNumeric(x)
        Dim py As Double() = CLRVector.asNumeric(y)
        Dim points As PointData() = px _
            .Select(Function(xi, i)
                        Return New PointData With {
                            .pt = New PointF(xi, y(i))
                        }
                    End Function) _
            .ToArray
        Dim serial As New SerialData With {
            .color = RColorPalette _
                .getColor(color) _
                .TranslateColor _
                .Alpha(alpha),
            .lineType = DashStyle.Solid,
            .pointSize = ptSize,
            .pts = points,
            .shape = LegendStyles.SolidLine,
            .title = name,
            .width = 5
        }

        Return serial
    End Function

    ''' <summary>
    ''' ### Violin plot
    ''' 
    ''' A violin plot is a compact display of a continuous distribution. It is a blend of boxplot and density: 
    ''' a violin plot is a mirrored density plot displayed in the same way as a boxplot.
    ''' </summary>
    ''' <param name="data">
    ''' The data To be displayed In this layer. There are three options
    ''' 
    ''' If NULL, the Default, the data Is inherited from the plot data As specified In the Call To ggplot().
    ''' A data.frame, Or other Object, will override the plot data. All objects will be fortified To produce 
    ''' a data frame. See fortify() For which variables will be created.
    ''' 
    ''' A Function will be called With a Single argument, the plot data. The Return value must be a data.frame, 
    ''' And will be used As the layer data. A Function can be created from a formula (e.g. ~ head(.x, 10)).
    ''' </param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg$"></param>
    ''' <param name="colorSet$"></param>
    ''' <param name="ylab$"></param>
    ''' <param name="title$"></param>
    ''' <param name="labelAngle"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Computed variables
    ''' 
    ''' + ``density`` density estimate
    ''' + ``scaled`` density estimate, scaled To maximum Of 1
    ''' + ``count`` density * number of points - probably useless for violin plots
    ''' + ``violinwidth`` density scaled For the violin plot, according To area, counts Or To a constant maximum width
    ''' + ``n`` number of points
    ''' + ``width`` width of violin bounding box
    ''' </remarks>
    <ExportAPI("violin")>
    Public Function doViolinPlot(data As Array,
                                 <RRawVectorArgument> Optional size As Object = Canvas.Resolution2K.Size,
                                 <RRawVectorArgument> Optional margin As Object = Canvas.Resolution2K.PaddingWithTopTitle,
                                 Optional bg$ = "white",
                                 Optional colorSet$ = DesignerTerms.TSFShellColors,
                                 Optional ylab$ = "y axis",
                                 Optional title$ = "Volin Plot",
                                 Optional labelAngle As Double = -45,
                                 Optional showStats As Boolean = True,
                                 Optional env As Environment = Nothing) As Object

        If data Is Nothing Then
            Return RInternal.debug.stop("the required dataset is nothing!", env)
        End If

        Dim type As Type = REnv.MeasureArrayElementType(data)

        size = InteropArgumentHelper.getSize(size, env)
        margin = InteropArgumentHelper.getPadding(margin)

        If type Is GetType(DataSet) Then
#Disable Warning
            Return ViolinPlot.Plot(
                dataset:=DirectCast(REnv.asVector(Of DataSet)(data), DataSet()),
                size:=size,
                margin:=margin,
                bg:=bg,
                colorset:=colorSet,
                Ylabel:=ylab,
                title:=title,
                labelAngle:=labelAngle,
                showStats:=showStats
            )
#Enable Warning
        Else
            Dim dataSet As New NamedCollection(Of Double) With {
                .name = title,
                .value = CLRVector.asNumeric(data)
            }

            Return ViolinPlot.Plot(
                dataset:={dataSet},
                size:=size,
                margin:=margin,
                bg:=bg,
                colorset:=colorSet,
                Ylabel:=ylab,
                title:=title,
                labelAngle:=labelAngle,
                showStats:=showStats
            )
        End If
    End Function

    <ExportAPI("fillPolygon")>
    Public Function PlotPolygon(<RRawVectorArgument>
                                polygon As Object,
                                <RRawVectorArgument>
                                Optional padding As Object = g.DefaultUltraLargePadding,
                                Optional grid_fill As Object = "white",
                                Optional reverse As Boolean = False,
                                Optional env As Environment = Nothing) As Object

        Dim theme As New Theme With {
            .gridFill = RColorPalette.getColor(grid_fill, "white"),
            .padding = InteropArgumentHelper.getPadding(padding, g.DefaultUltraLargePadding)
        }

        If polygon Is Nothing Then
            Return RInternal.debug.stop("polygon data can not be nothing!", env)
        ElseIf TypeOf polygon Is list Then
            Dim names As String() = DirectCast(polygon, list).getNames
            Dim pathData = names.Select(AddressOf DirectCast(polygon, list).getByName).ToArray
            Dim poly As pipeline = pipeline.TryCreatePipeline(Of GeneralPath)(pathData, env)

            If Not poly.isError Then
                Return New PolygonPlot2D(poly.populates(Of GeneralPath)(env), theme, names, reverse).Plot
            End If

            Return poly.getError
        Else
            Dim poly As pipeline = pipeline.TryCreatePipeline(Of GeneralPath)(polygon, env)

            If Not poly.isError Then
                Return New PolygonPlot2D(poly.populates(Of GeneralPath)(env), theme, reverse:=reverse).Plot
            End If

            Return poly.getError
        End If
    End Function

    ''' <summary>
    ''' A contour plot is a graphical technique for representing a 3-dimensional 
    ''' surface by plotting constant z slices, called contours, on a 2-dimensional 
    ''' format. That is, given a value for z, lines are drawn for connecting the 
    ''' ``(x,y)`` coordinates where that z value occurs.
    ''' 
    ''' The contour plot Is an alternative To a 3-D surface plot.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("contourPlot")>
    <RApiReturn(GetType(GraphicsData))>
    Public Function ContourPlot(<RRawVectorArgument> data As Object,
                                <RRawVectorArgument>
                                Optional colorSet As Object = "Spectral:c10",
                                Optional xlim As Double = Double.NaN,
                                Optional ylim As Double = Double.NaN,
                                <RListObjectArgument>
                                Optional args As list = Nothing,
                                Optional env As Environment = Nothing) As Object


        If data Is Nothing Then
            Return RInternal.debug.stop("object 'data' can not be nothing!", env)
        ElseIf TypeOf data Is Rdataframe Then
            Dim x As Double() = CLRVector.asNumeric(DirectCast(data, Rdataframe).columns("x"))
            Dim y As Double() = CLRVector.asNumeric(DirectCast(data, Rdataframe).columns("y"))
            Dim vals As Double() = CLRVector.asNumeric(DirectCast(data, Rdataframe).columns("data"))
            Dim measures As MeasureData() = x.Select(Function(xi, i) New MeasureData(xi, y(i), vals(i))).ToArray

            Return PlotContour.Plot(measures, colorSet:=RColorPalette.getColorSet(colorSet))
        ElseIf TypeOf data Is DeclareLambdaFunction Then
            Dim lambda As Func(Of (Double, Double), Double) = DirectCast(data, DeclareLambdaFunction).CreateLambda(Of (Double, Double), Double)(env)
            Dim rx As DoubleRange = args.getValue(Of Double())("x", env)
            Dim ry As DoubleRange = args.getValue(Of Double())("y", env)

            Return Contour.HeatMap.Plot(
                fun:=Function(x, y) lambda((x, y)),
                xrange:=rx,
                yrange:=ry,
                xsteps:=rx.Length / 200,
                ysteps:=ry.Length / 200
            )
        Else
            Dim layers As pipeline = pipeline.TryCreatePipeline(Of ContourLayer)(data, env)

            If layers.isError Then
                Return Message.InCompatibleType(GetType(FormulaExpression), data.GetType, env)
            End If

            Return layers _
                .populates(Of ContourLayer)(env) _
                .Plot(
                    colorSet:=RColorPalette.getColorSet(colorSet),
                    xlim:=xlim,
                    ylim:=ylim
                )
        End If
    End Function
End Module
