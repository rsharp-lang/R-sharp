#Region "Microsoft.VisualBasic::3c0338b6d8ebf889f23bdea245303fc1, Library\graphics\Plot2D\plots.vb"

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

    '   Total Lines: 1343
    '    Code Lines: 1056 (78.63%)
    ' Comment Lines: 149 (11.09%)
    '    - Xml Docs: 83.89%
    ' 
    '   Blank Lines: 138 (10.28%)
    '     File Size: 61.50 KB


    ' Module plots
    ' 
    '     Function: barplot, charPie, ContourPlot, CreateSerial, doViolinPlot
    '               findNumberVector, measureDataTable, modelWithClass, modelWithoutClass, plot_binBox
    '               plot_categoryBars, plot_corHeatmap, plot_deSolveResult, plot_hclust, plot_heatmap
    '               plotArray, plotContourLayers, plotFormula, plotLinearYFit, plotLmCall
    '               plotODEResult, plotPieChart, PlotPolygon, plotSerials, plotVector
    '               printImage, UpSetPlot
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
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Data.Plots.Plot3D
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Legend
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
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of Series)(Function(line) line.ToString)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of GraphicsData)(AddressOf printImage)

        Call REnv.Internal.generic.add("plot", GetType(DeclareLambdaFunction), AddressOf plotFormula)
        Call REnv.Internal.generic.add("plot", GetType(ODEOutput), AddressOf plotODEResult)
        Call REnv.Internal.generic.add("plot", GetType(ODEsOut), AddressOf plot_deSolveResult)
        Call REnv.Internal.generic.add("plot", GetType(Series()), AddressOf plotSerials)
        Call REnv.Internal.generic.add("plot", GetType(Series), AddressOf plotSerials)
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
        Dim rowNames As String() = dataset.Select(Function(a) a.ID).ToArray
        Dim mat(rowNames.Length - 1, colNames.Length - 1) As Double

        For i As Integer = 0 To rowNames.Length - 1
            For j As Integer = 0 To colNames.Length - 1
                mat(i, j) = dataset(i).Properties(colNames(j))
            Next
        Next

        Using plt As New HeatmapPlot(size.Width, size.Height, PlotTheme.Light(), driver) With {
            .Title = mainTitle,
            .Matrix = mat,
            .RowLabels = rowNames,
            .ColLabels = colNames,
            .ColorMap = parseColorMap(colors, ColorScale.ColorMapType.Viridis)
        }
            Call plt.Plot()
            Return plt.AsGraphicsData()
        End Using
    End Function

    Private Function parseColorMap(name As String, [default] As ColorScale.ColorMapType) As ColorScale.ColorMapType
        Dim cmap As ColorScale.ColorMapType = Nothing

        If [Enum].TryParse(name, ignoreCase:=True, result:=cmap) Then
            Return cmap
        Else
            Return [default]
        End If
    End Function

    ''' <summary>
    ''' 将 R# 的图例形状参数映射为新引擎的 <see cref="MarkerShape"/>
    ''' </summary>
    Private Function parseMarkerShape(style As LegendStyles) As MarkerShape
        Select Case style
            Case LegendStyles.Rectangle, LegendStyles.RoundRectangle : Return MarkerShape.Square
            Case LegendStyles.Diamond : Return MarkerShape.Diamond
            Case LegendStyles.Triangle : Return MarkerShape.Triangle
            Case LegendStyles.Hexagon : Return MarkerShape.Hexagon
            Case LegendStyles.Pentacle : Return MarkerShape.Star
            Case LegendStyles.SolidLine, LegendStyles.DashLine : Return MarkerShape.None
            Case Else : Return MarkerShape.Circle
        End Select
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
        Dim showLegend As Boolean = args.getValue("show.legend", env, True)
        Dim showYFit As Boolean = args.getValue("show.yfit", env, True)
        Dim padding As String = InteropArgumentHelper.getPadding(args!padding, "padding: 150px 100px 150px 200px")
        Dim xlab As String = args.getValue("xlab", env, "X")
        Dim ylab As String = args.getValue("ylab", env, "Y")
        Dim driver As Drivers = env.getDriver
        Dim dpi As Integer = graphicsPipeline.getDpi(args.slots, env, [default]:=100)

        ' 将 R# 的线性拟合结果转换为新引擎的回归拟合数据模型
        Dim testPoints = fit.ErrorTest _
            .Select(Function(t, i)
                        Dim pt As TestPoint = TryCast(t, TestPoint)

                        If pt Is Nothing Then
                            pt = New TestPoint With {
                                .X = i,
                                .Y = t.Y,
                                .Yfit = t.Yfit
                            }
                        End If

                        Return pt
                    End Function) _
            .ToArray
        Dim regression As New RegressionFit With {
            .X = testPoints.Select(Function(p) p.X).ToArray,
            .Y = testPoints.Select(Function(p) p.Y).ToArray,
            .Yfit = testPoints.Select(Function(p) p.Yfit).ToArray,
            .EquationText = fit.Polynomial.ToString,
            .Name = "fit"
        }

        Using plt As New RegressionPlot(size.SizeParser.Width, size.SizeParser.Height, New PlotTheme(padding), driver) With {
            .Fit = regression,
            .Title = args.getValue(Of String)("main", env, "Regression Plot"),
            .XLabel = xlab,
            .YLabel = ylab,
            .ShowPoints = showYFit,
            .ShowConfidenceBand = False,
            .ShowLegend = showLegend
        }
            Call plt.Plot()
            Return plt.AsGraphicsData()
        End Using
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
        Dim classSerials As New Dictionary(Of String, List(Of PointF))

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
            classSerials(label) = New List(Of PointF)
        Next

        If y Is Nothing Then
            For i As Integer = 0 To x.Length - 1
                classSerials(classList(i)).Add(New PointF(classSerials(classList(i)).Count + 1, x(i)))
            Next
        Else
            Dim maxy As Double = y.Max

            For i As Integer = 0 To x.Length - 1
                classSerials(classList(i)).Add(New PointF(x(i), If(reverse, maxy - y(i), y(i))))
            Next
        End If

        Dim lines As Series() = classSerials _
            .Where(Function(list)
                       Return list.Value.Count > 0
                   End Function) _
            .Select(Function(tuple)
                        Return New Series With {
                            .X = tuple.Value.Select(Function(p) CDbl(p.X)).ToArray,
                            .Y = tuple.Value.Select(Function(p) CDbl(p.Y)).ToArray,
                            .Color = colors(tuple.Key),
                            .PointSize = ptSize,
                            .MarkerShape = parseMarkerShape(shape),
                            .Name = tuple.Key,
                            .LineStyle = DashStyle.Custom
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

        ' 逐点着色（colorSet / scaler 映射）：由于新引擎的 Series 只支持系列级颜色，
        ' 这里按照点颜色将数据点分组为多个相同标题的 Series
        Dim pointColors As Dictionary(Of Integer, Color) = Nothing
        Dim maxy As Double = If(y.IsNullOrEmpty, x.Max, y.Max)

        If Not y Is Nothing Then
            Dim mapScaler As Double() = CLRVector.asNumeric(args.getBySynonyms("scaler", "heatmap"))

            If mapScaler.IsNullOrEmpty Then
                If args.hasName("colorSet") AndAlso Not args!colorSet Is Nothing Then
                    Dim colorsMap As String() = RColorPalette.getColors(args!colorSet, x.Length, Nothing)

                    pointColors = New Dictionary(Of Integer, Color)
                    For i As Integer = 0 To x.Length - 1
                        pointColors(i) = colorsMap(i).TranslateColor
                    Next
                End If
            Else
                Dim colorsMap As String() = Nothing
                Dim scaler As New DoubleRange(mapScaler)
                Dim levels As New DoubleRange(0, 30)

                If scaler.Length = 0 Then
                    Return RInternal.debug.stop("scatter point color heatmap mapping range should be greater than zero!", env)
                End If

                If args.hasName("colorSet") AndAlso Not args!colorSet Is Nothing Then
                    colorsMap = RColorPalette.getColors(args!colorSet, levels.Max + 1, Nothing)
                Else
                    colorsMap = RColorPalette.getColors("viridis", levels.Max + 1, Nothing)
                End If

                pointColors = New Dictionary(Of Integer, Color)

                For i As Integer = 0 To x.Length - 1
                    Dim value As Double = mapScaler(i)
                    Dim offset As Integer = scaler.ScaleMapping(value, levels)

                    pointColors(i) = colorsMap(offset).TranslateColor
                Next
            End If
        End If

        Dim title As String = args.getValue("title", env, If(y Is Nothing, "data", "x ~ y"))
        Dim color As Color = args.getValue("color", env, "black").TranslateColor
        Dim defaultColor As Color = color
        Dim line As New Series With {
            .PointSize = ptSize,
            .Name = title,
            .MarkerShape = parseMarkerShape(shape),
            .Color = defaultColor
        }

        If y Is Nothing Then
            line.X = x.SeqIterator.Select(Function(i) CDbl(i.i)).ToArray
            line.Y = x
        Else
            line.X = x
            line.Y = y.Select(Function(yi, i) If(reverse, maxy - yi, yi)).ToArray
        End If

        If args.hasName("fit") Then
            Dim fit As Object = args!fit
            Dim lines As New List(Of Series) From {line}
            Dim x_axis As Double() = line.X
            Dim color = defaultColor

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

                    lines.Add(New Series With {
                        .PointSize = ptSize,
                        .Name = peak.ToString,
                        .MarkerShape = parseMarkerShape(shape),
                        .Color = color,
                        .X = x_axis,
                        .Y = x_axis _
                            .Select(Function(xi)
                                        Return If(reverse, maxy - peak.gaussian(xi), peak.gaussian(xi))
                                    End Function) _
                            .ToArray
                    })
                Next
            End If

            Return plotSerials(lines, args, env)
        Else
            Dim fit_names As String() = args.getNames _
                .Where(Function(s)
                           Return TypeOf args(s) Is list AndAlso DirectCast(args(s), list).hasNames("x", "y")
                       End Function) _
                .ToArray
            Dim lines As New List(Of Series) From {line}

            For Each name As String In fit_names
                Dim serial_data As list = args(name)
                Dim x_axis As Double() = CLRVector.asNumeric(serial_data!x)
                Dim y_axis As Double() = CLRVector.asNumeric(serial_data!y)
                Dim color_line As Color = color

                If serial_data.hasName("color") Then
                    color_line = serial_data.getValue("color", env, "black").TranslateColor
                End If

                lines.Add(New Series With {
                    .PointSize = ptSize,
                    .Name = name,
                    .MarkerShape = parseMarkerShape(shape),
                    .Color = color_line,
                    .X = x_axis,
                    .Y = y_axis.Select(Function(yi, i) If(reverse, maxy - yi, yi)).ToArray
                })
            Next

            If pointColors.IsNullOrEmpty Then
                Return plotSerials(lines, args, env)
            Else
                ' 逐点着色：按颜色分组后输出
                Dim grouped As New List(Of Series)

                For Each base As Series In lines
                    If base.Color Is Nothing OrElse Not base Is line Then
                        grouped.Add(base)
                        Continue For
                    End If

                    Dim byColor = base.Y _
                        .SeqIterator _
                        .GroupBy(Function(i) pointColors(i.i).ToHtmlColor) _
                        .ToArray

                    For gi As Integer = 0 To byColor.Length - 1
                        Dim grp = byColor(gi)

                        grouped.Add(New Series With {
                            .PointSize = ptSize,
                            .Name = If(gi = 0, base.Name, ""),
                            .MarkerShape = base.MarkerShape,
                            .Color = pointColors(grp.First.i),
                            .X = grp.Select(Function(i) base.X(i.i)).ToArray,
                            .Y = grp.Select(Function(i) base.Y(i.i)).ToArray,
                            .LineStyle = DashStyle.Custom
                        })
                    Next
                Next

                Return plotSerials(grouped, args, env)
            End If
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

        If TypeOf array Is Series() Then
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
        Dim showLabels As Boolean = False

        If args.hasName("class") Then
            Call TryGetClassData(args!class, classes, classinfo)
        End If

        Dim theme As New DendrogramTheme With {
            .padding = padding,
            .tagCSS = labelStyle,
            .gridStrokeX = linkStroke,
            .axisTickCSS = tickStyle,
            .axisStroke = axisStroke,
            .pointSize = CSng(ptSize),
            .background = bg,
            .XaxisTickFormat = axisFormat
        }

        Return New DendrogramPlot(
            hist:=cluster,
            theme:=theme,
            classes:=classes,
            classinfo:=classinfo,
            pointColor:=pointColor,
            showLeafLabels:=showLabels
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

        Dim n As Integer = dist.size
        Dim names As String() = dist.keys
        Dim m(n - 1, n - 1) As Double

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                m(i, j) = dist(i, j)
            Next
        Next

        Using plt As New CorrelationTrianglePlot(size.SizeParser.Width, size.SizeParser.Height, New PlotTheme(padding), driver) With {
            .Title = title,
            .Correlation = New Microsoft.VisualBasic.Data.Plots.CorrelationMatrix With {
                .Names = names,
                .Matrix = m
            },
            .ShowValues = fixedSize
        }
            Call plt.Plot()
            Return plt.AsGraphicsData()
        End Using
    End Function

    Public Function plot_categoryBars(data As Dictionary(Of String, Double), args As list, env As Environment) As Object
        Dim title$ = args.GetString("title", "Histogram Plot")
        Dim xlab$ = args.GetString("x.lab", "X")
        Dim ylab$ = args.GetString("y.lab", "Y")
        Dim padding$ = InteropArgumentHelper.getPadding(args!padding)
        Dim driver As Drivers = env.getDriver
        Dim serials As BarSerial() = data _
            .Select(Function(bar)
                        Return New BarSerial With {
                            .Label = bar.Key,
                            .Value = bar.Value,
                            .Color = Color.SkyBlue
                        }
                    End Function) _
            .ToArray

        Using plt As New BarPlot(2400, 1800, New PlotTheme(padding), driver) With {
            .Title = title,
            .XLabel = xlab,
            .YLabel = ylab,
            .Serials = serials
        }
            Call plt.Plot()
            Return plt.AsGraphicsData()
        End Using
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

        ' DataBinBox 分箱数据 -> 显式分箱边界的直方图
        Dim binEdges As New List(Of Double)

        If data.Length > 0 Then
            binEdges.Add(data(0).Raw.Min)
        End If

        For Each bin As DataBinBox(Of Double) In data
            binEdges.Add(bin.Raw.Max)
        Next

        Using plt As New HistogramPlot(size.Width, size.Height, New PlotTheme(padding), env.getDriver) With {
            .Title = title,
            .XLabel = xlab,
            .YLabel = ylab,
            .Data = data.SelectMany(Function(bin) bin.Raw).ToArray,
            .BinEdges = binEdges.ToArray
        }
            Call plt.Plot()
            Return plt.AsGraphicsData()
        End Using
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

        Dim names As New List(Of String)
        Dim values As New List(Of Double)
        Dim colorSet As String = RColorPalette.getColorSet(schema, "Paired:c12")
        Dim colors As LoopArray(Of Color) = Designer.GetColors(colorSet)

        If x Is Nothing Then
            Return RInternal.debug.stop("the requred x data object can not be nothing!", env)
        ElseIf TypeOf x Is list Then
            ' a collection of [name => number]
            For Each tag As NamedValue(Of Object) In DirectCast(x, list).namedValues
                names.Add(tag.Name)
                values.Add(CLRVector.asNumeric(tag.Value).GetValue(Scan0))
            Next
        ElseIf TypeOf x Is vector Then
            Dim v As vector = DirectCast(x, vector)

            If v.elementType.is_numeric Then
                Dim vecNames As String() = DirectCast(x, vector).getNames
                Dim vec As Double() = CLRVector.asNumeric(x)

                For i As Integer = 0 To vecNames.Length - 1
                    names.Add(vecNames(i))
                    values.Add(vec(i))
                Next
            Else
                For Each factor As IGrouping(Of String, String) In CLRVector.asCharacter(x).GroupBy(Function(s) s)
                    names.Add(factor.Key)
                    values.Add(factor.Count)
                Next
            End If
        ElseIf TypeOf x Is String() Then
            For Each factor As IGrouping(Of String, String) In CLRVector.asCharacter(x).GroupBy(Function(s) s)
                names.Add(factor.Key)
                values.Add(factor.Count)
            Next
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
            Dim slices As PieSlice() = names _
                .SeqIterator _
                .Select(Function(a)
                            Return New PieSlice With {
                                .Name = a.value,
                                .Value = values(a.i),
                                .Color = ++colors
                            }
                        End Function) _
                .ToArray

            Return PieChart3D.Plot3D(slices, camera, driver:=env.getDriver)
        Else
            ' 2D
            Dim sz As Size = InteropArgumentHelper.getSize(size, env).SizeParser
            Dim colorArr(values.Count - 1) As Color

            For i As Integer = 0 To values.Count - 1
                colorArr(i) = ++colors
            Next

            Using plt As New PiePlot(sz.Width, sz.Height, PlotTheme.Light(), env.getDriver) With {
                .Title = "Pie Chart",
                .Labels = names.ToArray,
                .Values = values.ToArray,
                .Colors = colorArr
            }
                Call plt.Plot()
                Return plt.AsGraphicsData()
            End Using
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
        Dim bars As VariableBarData() = items _
            .SeqIterator _
            .Select(Function(i)
                        ' 旧版本的 barplot 带有 min/max 变宽柱语义，
                        ' 这里映射为新引擎的 VariableWidthBarPlot
                        Return New VariableBarData With {
                            .Name = i.value,
                            .Value = values(i.i),
                            .Width = maxX(i.i) - minX(i.i)
                        }
                    End Function) _
            .ToArray

        Dim sz As Size = InteropArgumentHelper.getSize(size, env).SizeParser
        Dim pad As String = InteropArgumentHelper.getPadding(padding)

        Using plt As New VariableWidthBarPlot(sz.Width, sz.Height, New PlotTheme(pad), env.getDriver) With {
            .Title = title,
            .XLabel = xlab,
            .YLabel = ylab,
            .Bars = New List(Of VariableBarData)(bars)
        }
            Call plt.Plot()
            Return plt.AsGraphicsData()
        End Using
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
        If camera Is Nothing Then
            camera = New Camera With {
                .screen = New Size(2400, 1800),
                .viewDistance = 10000
            }
        End If

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

        ' 3D 散点：使用迁移到 DataPlot 的 Plot3D 引擎（支持 driver 输出）
        Return Scatter3DPlot.Plot(
            {data}, camera,
            bg:=bg,
            showLegend:=False,
            driver:=env.getDriver
        )
    End Function

    Public Function plotODEResult(math As ODEOutput, args As list, env As Environment) As Object
        Dim size As String = InteropArgumentHelper.getSize(args!size, env, [default]:="1600,1200")
        Dim padding As String = InteropArgumentHelper.getPadding(args!padding, [default]:=g.DefaultPadding, env)
        Dim pts As PointF() = math.GetPointsData.Select(Function(p) p.PointF).ToArray
        Dim sz As Size = size.SizeParser
        Dim driver As Drivers = env.getDriver

        Using plt As New LinePlot(sz.Width, sz.Height, New PlotTheme(padding), driver) With {
            .Title = math.ID
        }
            Call plt.Plot({New Series With {
                .Name = math.ID,
                .Color = Color.Cyan,
                .LineStyle = DashStyle.Dash,
                .X = pts.Select(Function(p) CDbl(p.X)).ToArray,
                .Y = pts.Select(Function(p) CDbl(p.Y)).ToArray
            }})
            Return plt.AsGraphicsData()
        End Using
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
        Dim sz As Size = InteropArgumentHelper.getSize(args!size, env).SizeParser
        Dim padding As String = InteropArgumentHelper.getPadding(args!padding)
        Dim driver As Drivers = imageDriverHandler.getDriver(env)

        Using plt As New LinePlot(sz.Width, sz.Height, New PlotTheme(padding), driver) With {
            .Title = math.ToString
        }
            Call plt.Plot({New Series With {
                .Name = math.ToString,
                .Color = Color.Black,
                .LineStyle = DashStyle.Solid,
                .X = x,
                .Y = x.Select(Function(xi) fx(xi)).ToArray
            }})
            Return plt.AsGraphicsData()
        End Using
    End Function

    ''' <summary>
    ''' plot scatter 2d serial data
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Function plotSerials(data As Object, args As list, env As Environment) As Object
        If TypeOf data Is Series Then
            data = {DirectCast(data, Series)}
        End If

        Dim serials As Series() = DirectCast(data, IEnumerable(Of Series)).ToArray

        If drawLine Then
            ' plot(x, y, line = TRUE) 绘制连线
            For Each s As Series In serials
                s.LineStyle = DashStyle.Solid
            Next
        End If

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
        Dim drawLine As Boolean = getFirst(CLRVector.asLogical(args!line))
        Dim convexHull As Object = args.getBySynonyms("convexHull")
        Dim convexHullList = CLRVector.asCharacter(convexHull)
        Dim drawHull As Boolean = False

        If Not convexHullList.IsNullOrEmpty Then
            If convexHullList.Length = 1 AndAlso (convexHullList(0).ToLower = "true" OrElse convexHullList(0).ToLower = "false") Then
                If convexHullList(0).ParseBoolean Then
                    ' use all serials as convex hull
                    drawHull = True
                Else
                    ' no convex hull
                    drawHull = False
                End If
            Else
                drawHull = True
            End If
        End If

        ' 将 Line 列表 (斜率-截距参考线) 转换为新引擎的 abline 模型: (b, a) => y = a + b * x
        Dim ablines As New List(Of (b As Double, a As Double))

        For Each line As Line In args.getValue(Of Line())("abline", env).SafeQuery
            If line.B.X <> line.A.X Then
                Dim b As Double = (line.B.Y - line.A.Y) / (line.B.X - line.A.X)

                ablines.Add((b, line.A.Y - b * line.A.X))
            End If
        Next

        Dim theme As New PlotTheme(margin) With {
            .ShowGrid = showAxis
        }

        If args.CheckGraphicsDeviceExists Then
            ' draw on current graphics context
            Dim dev As graphicsDevice = R_graphics.Common.Runtime.graphics.curDev

            Using plt As New ScatterPlot(dev.g, theme) With {
                .Title = title,
                .XLabel = args.getValue("x.lab", env, "X"),
                .YLabel = args.getValue("y.lab", env, "Y"),
                .ShowLegend = showLegend,
                .ShowConvexHull = drawHull,
                .AbLines = ablines,
                .Smooth = spline <> Splines.None
            }
                If Not xlim.IsNullOrEmpty Then
                    plt.XMin = xlim(0)
                    plt.XMax = xlim(xlim.Length - 1)
                End If
                If Not ylim.IsNullOrEmpty Then
                    plt.YMin = ylim(0)
                    plt.YMax = ylim(ylim.Length - 1)
                End If

                Call plt.Plot(serials)
            End Using

            Return Nothing
        Else
            Dim sz As Size = size.SizeParser

            Using plt As New ScatterPlot(sz.Width, sz.Height, theme, driver) With {
                .Title = title,
                .XLabel = args.getValue("x.lab", env, "X"),
                .YLabel = args.getValue("y.lab", env, "Y"),
                .ShowLegend = showLegend,
                .ShowConvexHull = drawHull,
                .AbLines = ablines,
                .Smooth = spline <> Splines.None
            }
                If Not xlim.IsNullOrEmpty Then
                    plt.XMin = xlim(0)
                    plt.XMax = xlim(xlim.Length - 1)
                End If
                If Not ylim.IsNullOrEmpty Then
                    plt.YMin = ylim(0)
                    plt.YMax = ylim(ylim.Length - 1)
                End If

                Call plt.Plot(serials)
                Return plt.AsGraphicsData()
            End Using
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
                                 Optional ptSize As Integer = 5) As Series

        Dim px As Double() = CLRVector.asNumeric(x)
        Dim py As Double() = CLRVector.asNumeric(y)
        Dim serial As New Series With {
            .Color = RColorPalette _
                .getColor(color) _
                .TranslateColor _
                .Alpha(alpha),
            .LineStyle = DashStyle.Solid,
            .PointSize = ptSize,
            .X = px,
            .Y = py,
            .Name = name
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
        Dim driver As Drivers = env.getDriver
        Dim groups As New List(Of BoxGroup)

        If type Is GetType(DataSet) Then
            For Each entity As DataSet In DirectCast(data, DataSet())
                groups.Add(New BoxGroup With {
                    .Name = entity.ID,
                    .Data = entity.Vector
                })
            Next
        Else
            groups.Add(New BoxGroup With {
                .Name = title,
                .Data = CLRVector.asNumeric(data)
            })
        End If

        Using plt As New ViolinPlot(size.SizeParser.Width, size.SizeParser.Height, New PlotTheme(margin), driver) With {
            .Title = title,
            .YLabel = ylab,
            .Groups = groups
        }
            Call plt.Plot()
            Return plt.AsGraphicsData()
        End Using
    End Function

    <ExportAPI("fillPolygon")>
    Public Function PlotPolygon(<RRawVectorArgument>
                                polygon As Object,
                                <RRawVectorArgument>
                                Optional padding As Object = g.DefaultUltraLargePadding,
                                Optional grid_fill As Object = "white",
                                Optional reverse As Boolean = False,
                                Optional env As Environment = Nothing) As Object

        Dim pad As String = InteropArgumentHelper.getPadding(padding, g.DefaultUltraLargePadding)
        Dim driver As Drivers = env.getDriver

        If polygon Is Nothing Then
            Return RInternal.debug.stop("polygon data can not be nothing!", env)
        ElseIf TypeOf polygon Is list Then
            Dim names As String() = DirectCast(polygon, list).getNames
            Dim pathData = names.Select(AddressOf DirectCast(polygon, list).getByName).ToArray
            Dim poly As pipeline = pipeline.TryCreatePipeline(Of GeneralPath)(pathData, env, suppress:=True)

            If Not poly.isError Then
                Return renderPolygons(poly.populates(Of GeneralPath)(env), names, pad, driver)
            End If

            poly = pipeline.TryCreatePipeline(Of Math2D.Polygon2D)(pathData, env)

            If Not poly.isError Then
                Return renderPolygons(poly.populates(Of Math2D.Polygon2D)(env).Select(Function(p) New GeneralPath(p)), names, pad, driver)
            End If

            Return poly.getError
        Else
            Dim poly As pipeline = pipeline.TryCreatePipeline(Of GeneralPath)(polygon, env, suppress:=True)

            If Not poly.isError Then
                Return renderPolygons(poly.populates(Of GeneralPath)(env), Nothing, pad, driver)
            End If

            poly = pipeline.TryCreatePipeline(Of Math2D.Polygon2D)(polygon, env)

            If Not poly.isError Then
                Return renderPolygons(poly.populates(Of Math2D.Polygon2D)(env).Select(Function(p) New GeneralPath(p)), Nothing, pad, driver)
            End If

            Return poly.getError
        End If
    End Function

    ''' <summary>
    ''' render polygon groups via the new DataPlot FillPolygons plot engine
    ''' </summary>
    Private Function renderPolygons(paths As IEnumerable(Of GeneralPath),
                                    names As String(),
                                    padding As String,
                                    driver As Drivers) As GraphicsData

        Dim pathList As GeneralPath() = paths.ToArray
        Dim groups As New List(Of PolygonGroup)

        For i As Integer = 0 To pathList.Length - 1
            Dim name As String = If(names.IsNullOrEmpty, pathList(i).ToString, names(i))

            groups.Add(New PolygonGroup With {
                .Label = name,
                .SubRegions = pathList(i).GetPolygons.ToArray
            })
        Next

        Using plt As New FillPolygons(2400, 1800, New PlotTheme(padding), driver) With {
            .Groups = groups
        }
            Call plt.Plot()
            Return plt.AsGraphicsData()
        End Using
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
            Dim layers As ContourLayer() = ContourLayer.GetContours(measures).ToArray

            Return renderContourLayers(layers, RColorPalette.getColorSet(colorSet), env.getDriver)
        ElseIf TypeOf data Is DeclareLambdaFunction Then
            Dim lambda As Func(Of (Double, Double), Double) = DirectCast(data, DeclareLambdaFunction).CreateLambda(Of (Double, Double), Double)(env)
            Dim rx As DoubleRange = args.getValue(Of Double())("x", env)
            Dim ry As DoubleRange = args.getValue(Of Double())("y", env)

            Using plt As New ContourPlot(2400, 1800, PlotTheme.Light(), env.getDriver) With {
                .Title = "Contour Plot",
                .Surface = Function(x, y) lambda((x, y)),
                .XMin = rx.Min, .XMax = rx.Max,
                .YMin = ry.Min, .YMax = ry.Max,
                .Mode = ContourPlot.ContourMode.Filled,
                .Levels = 10
            }
                Call plt.Plot()
                Return plt.AsGraphicsData()
            End Using
        Else
            Dim layers As pipeline = pipeline.TryCreatePipeline(Of ContourLayer)(data, env)

            If layers.isError Then
                Return Message.InCompatibleType(GetType(FormulaExpression), data.GetType, env)
            End If

            Return renderContourLayers(layers.populates(Of ContourLayer)(env).ToArray, RColorPalette.getColorSet(colorSet), env.getDriver)
        End If
    End Function

    ''' <summary>
    ''' render the marching squares contour layers
    ''' (从旧 Plots 项目的 Contour.ContourPlot 渲染逻辑迁移而来)
    ''' </summary>
    Private Function renderContourLayers(layers As ContourLayer(), colorSet As String, driver As Drivers) As GraphicsData
        Dim contours As GeneralPath() = layers _
            .OrderBy(Function(layer) layer.threshold) _
            .Select(Function(layer) New GeneralPath(layer)) _
            .ToArray
        Dim level_cutoff As Double() = contours.Select(Function(c) c.level).ToArray
        Dim colors As Brush() = Designer _
            .GetColors(colorSet, level_cutoff.Length) _
            .Select(Function(c) New SolidBrush(c)) _
            .ToArray
        Dim i As i32 = Scan0
        Dim plotInternal =
            Sub(ByRef g As IGraphics, canvas As GraphicsRegion)
                Dim css As CSSEnvirnment = g.LoadEnvironment
                Dim polygons = contours _
                    .Select(Function(layer) layer.GetContour.shapes) _
                    .IteratesALL _
                    .ToArray
                Dim dims As Size

                If polygons.Length = 0 Then
                    dims = New Size
                Else
                    dims = New Size(polygons.Select(Function(p) p.x.Max).Max, polygons.Select(Function(p) p.y.Max).Max)
                End If

                Dim rect As Rectangle = canvas.PlotRegion(css)

                If dims.Width * dims.Height > 0 Then
                    Dim scaleX = d3js.scale.linear.domain(values:=New Double() {0, dims.Width}).range(values:=New Double() {rect.Left, rect.Right})
                    Dim scaleY = d3js.scale.linear.domain(values:=New Double() {0, dims.Height}).range(values:=New Double() {rect.Top, rect.Bottom})

                    For Each polygon As GeneralPath In contours
                        Dim color As Brush = colors(++i)

                        Call polygon.Fill(g, color, scaleX, scaleY)
                        Call polygon.Draw(g, Pens.Black, scaleX, scaleY)
                    Next
                End If

                Dim paddingLayout As PaddingLayout = PaddingLayout.EvaluateFromCSS(css, canvas.Padding)
                Dim legendLayout As New Rectangle(rect.Right + 10, rect.Top, paddingLayout.Right / 3 * 2, rect.Height / 3 * 2)
                Dim legendTitleFont As Font = css.GetFont(CSSFont.Win7LargeBold)
                Dim tickFont As Font = css.GetFont(CSSFont.Win7Normal)

                Call g.ColorMapLegend(legendLayout, colors, level_cutoff, legendTitleFont, title:="Levels", tickFont, Pens.Gray)
            End Sub

        Return g.GraphicsPlots(New Size(2400, 1800), "padding: 100px 150px 100px 150px;", "white", plotInternal, driver:=driver)
    End Function
End Module
