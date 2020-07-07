#Region "Microsoft.VisualBasic::9f643179228d6ffe84116762eb265e38, Library\R.plot\plots.vb"

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

' Module plots
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: CreateSerial, doVolinPlot, linearRegression, plot_binBox, plot_categoryBars
'               plot_corHeatmap, plot_deSolveResult, plot_hclust, plotFormula, plotODEResult
'               plotSerials
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Histogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.DendrogramVisualize
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports Scatter2D = Microsoft.VisualBasic.Data.ChartPlots.Scatter

''' <summary>
''' chartting plots for R#
''' </summary>
<Package("charts", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
Module plots

    ''' <summary>
    ''' do plot of the linear regression result
    ''' </summary>
    ''' <param name="lm">the linear model</param>
    ''' <returns></returns>
    <ExportAPI("linear.regression")>
    Public Function linearRegression(lm As IFitted) As GraphicsData
        Return RegressionPlot.Plot(lm)
    End Function

    Sub New()
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of SerialData)(Function(line) line.ToString)
    End Sub

    <RInitialize>
    Sub Main()
        Call REnv.Internal.generic.add("plot", GetType(DeclareLambdaFunction), AddressOf plotFormula)
        Call REnv.Internal.generic.add("plot", GetType(ODEOutput), AddressOf plotODEResult)
        Call REnv.Internal.generic.add("plot", GetType(ODEsOut), AddressOf plot_deSolveResult)
        Call REnv.Internal.generic.add("plot", GetType(SerialData()), AddressOf plotSerials)
        Call REnv.Internal.generic.add("plot", GetType(SerialData), AddressOf plotSerials)
        Call REnv.Internal.generic.add("plot", GetType(DataBinBox(Of Double)()), AddressOf plot_binBox)
        Call REnv.Internal.generic.add("plot", GetType(Dictionary(Of String, Double)), AddressOf plot_categoryBars)
        Call REnv.Internal.generic.add("plot", GetType(DistanceMatrix), AddressOf plot_corHeatmap)
        Call REnv.Internal.generic.add("plot", GetType(Cluster), AddressOf plot_hclust)
    End Sub

    Public Function plot_hclust(cluster As Cluster, args As list, env As Environment) As Object
        Dim cls As Dictionary(Of String, String) = Nothing
        Dim size$ = InteropArgumentHelper.getSize(args.getByName("size"))
        Dim padding$ = InteropArgumentHelper.getPadding(args.getByName("padding"))

        If args.hasName("class") Then
            Dim list = args!class

            If TypeOf list Is list Then
                cls = DirectCast(list, list).slots _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return Scripting.ToString(a.Value)
                                  End Function)
            ElseIf TypeOf list Is Dictionary(Of String, String) Then
                cls = list
            ElseIf list.GetType.ImplementInterface(GetType(IDictionary)) Then
                Dim hash = DirectCast(list, IDictionary)

                cls = New Dictionary(Of String, String)

                For Each key As Object In hash.Keys
                    cls.Add(key.ToString, Scripting.ToString(hash(key)))
                Next
            End If
        End If

        Dim dp As New DendrogramPanel With {
           .LineColor = Color.Blue,
           .ScaleValueDecimals = 0,
           .ScaleValueInterval = 1,
           .Model = cluster,
           .ClassTable = cls,
           .ShowDistanceValues = False
       }
        Dim region As New GraphicsRegion(size.SizeParser, padding)

        Using g As Graphics2D = region.Size.CreateGDIDevice(filled:=Color.White)
            Call dp.Paint(g, region.PlotRegion, layout:=Layouts.Vertical)
            Return g.ImageResource
        End Using
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
        Dim bg$ = InteropArgumentHelper.getColor(args!bg, "white")
        Dim size = InteropArgumentHelper.getSize(args!size, "3600,3000")
        Dim padding$ = InteropArgumentHelper.getPadding(args!padding, "padding: 300px 150px 150px 100px;")
        Dim driver As Drivers = args.GetString("driver", "default").DoCall(AddressOf g.ParseDriverEnumValue)
        Dim colorSet = args.GetString("colors", ColorBrewer.DivergingSchemes.RdBu11)
        Dim fixedSize = args.getValue(Of Boolean)("fixed_size", env, False)
        Dim titleFont$ = InteropArgumentHelper.getFontCSS(args!mainCSS, CSSFont.Win7VeryLarge)
        Dim labelFont$ = InteropArgumentHelper.getFontCSS(args!labelCSS, CSSFont.Win7Normal)
        Dim legendTitleFont$ = InteropArgumentHelper.getFontCSS(args!legendTitleCSS, CSSFont.Win7LargeBold)

        Return CorrelationHeatmap.Plot(
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
        Dim padding$ = InteropArgumentHelper.getPadding(args!padding)

        If [step] <= 0 Then
            ' guess step value from binbox width
            [step] = data _
                .Select(Function(bin)
                            Return bin.Raw.Range.Length
                        End Function) _
                .Average
        End If

        Return data.HistogramPlot(
            [step]:=[step],
            serialsTitle:=title,
            xLabel:=xlab,
            yLabel:=ylab,
            padding:=padding
        )
    End Function

    Public Function plot_deSolveResult(desolve As ODEsOut, args As list, env As Environment) As Object
        Dim vector As list = args!vector
        Dim camera As Camera = args!camera
        Dim color As Color = InteropArgumentHelper.getColor(args!color, "black").TranslateColor
        Dim bg$ = InteropArgumentHelper.getColor(args!bg, "white")
        Dim title As String = Scripting.ToString(getFirst(args!title), "Plot deSolve")
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

        Return {data}.Plot(camera, bg:=bg)
    End Function

    Public Function plotODEResult(math As ODEOutput, args As list, env As Environment) As Object
        Return math.Plot(size:=InteropArgumentHelper.getSize(args!size))
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
        Dim x As Double() = asVector(Of Double)(args!x)
        Dim points As PointF() = x.Select(Function(xi) New PointF(xi, fx(xi))).ToArray

        Return points.Plot(
            size:=InteropArgumentHelper.getSize(args!size).SizeParser,
            title:=math.ToString
        )
    End Function

    Public Function plotSerials(data As Object, args As list, env As Environment) As Object
        If TypeOf data Is SerialData Then
            data = {DirectCast(data, SerialData)}
        End If

        Dim serials As SerialData() = DirectCast(data, SerialData())
        Dim size As String = InteropArgumentHelper.getSize(args!size)
        Dim padding = InteropArgumentHelper.getPadding(args!padding)
        Dim title As String = Scripting.ToString(getFirst(args!title), "Scatter Plot")
        Dim showLegend As Boolean
        Dim spline As Splines = args.getValue(Of Splines)("interplot", env, Splines.None)

        If args.hasName("showLegend") Then
            showLegend = getFirst(asLogical(args!showLegend))
        Else
            showLegend = True
        End If

        Return Scatter2D.Plot(
            c:=serials,
            size:=size, padding:=padding,
            Xlabel:=getFirst(REnv.asVector(Of String)(args("x.lab"))),
            Ylabel:=getFirst(REnv.asVector(Of String)(args("y.lab"))),
            drawLine:=getFirst(asLogical(args!line)),
            legendBgFill:=InteropArgumentHelper.getColor(args!legendBgFill, Nothing),
            legendFontCSS:=InteropArgumentHelper.getFontCSS(args("legend.font")),
            showLegend:=showLegend,
            title:=title,
            legendSplit:=args.getValue(Of Integer)("legend.block", env),
            ablines:=args.getValue(Of Line())("abline", env),
            hullConvexList:=args.getValue(Of String())("convexHull", env),
            XtickFormat:=args.getValue(Of String)("x.format", env, "F2"),
            YtickFormat:=args.getValue(Of String)("y.format", env, "F2"),
            interplot:=spline
        )
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

        Dim px As Double() = asVector(Of Double)(x)
        Dim py As Double() = asVector(Of Double)(y)
        Dim points As PointData() = px _
            .Select(Function(xi, i)
                        Return New PointData With {
                            .pt = New PointF(xi, y(i))
                        }
                    End Function) _
            .ToArray
        Dim serial As New SerialData With {
            .color = InteropArgumentHelper.getColor(color).TranslateColor.Alpha(alpha),
            .lineType = DashStyle.Solid,
            .pointSize = ptSize,
            .pts = points,
            .shape = LegendStyles.SolidLine,
            .title = name,
            .width = 5
        }

        Return serial
    End Function

    <ExportAPI("volinPlot")>
    Public Function doVolinPlot(dataset As Array,
                                <RRawVectorArgument> Optional size As Object = Canvas.Resolution2K.Size,
                                <RRawVectorArgument> Optional margin As Object = Canvas.Resolution2K.PaddingWithTopTitle,
                                Optional bg$ = "white",
                                Optional colorSet$ = DesignerTerms.TSFShellColors,
                                Optional ylab$ = "y axis",
                                Optional title$ = "Volin Plot",
                                Optional labelAngle As Double = -45,
                                Optional env As Environment = Nothing) As Object

        If dataset Is Nothing Then
            Return Internal.debug.stop("the required dataset is nothing!", env)
        End If

        Dim type As Type = REnv.MeasureArrayElementType(dataset)

        size = InteropArgumentHelper.getSize(size)
        margin = InteropArgumentHelper.getPadding(margin)

        If type Is GetType(DataSet) Then
            Return VolinPlot.Plot(
                dataset:=DirectCast(REnv.asVector(Of DataSet)(dataset), DataSet()),
                size:=size,
                margin:=margin,
                bg:=bg,
                colorset:=colorSet,
                Ylabel:=ylab,
                title:=title,
                labelAngle:=labelAngle
            )
        Else
            Dim data As New NamedCollection(Of Double) With {
                .name = title,
                .value = REnv.asVector(Of Double)(dataset)
            }

            Return VolinPlot.Plot(
                dataset:={data},
                size:=size,
                margin:=margin,
                bg:=bg,
                colorset:=colorSet,
                Ylabel:=ylab,
                title:=title,
                labelAngle:=labelAngle
            )
        End If
    End Function
End Module
