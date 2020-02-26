#Region "Microsoft.VisualBasic::865fe1ab5bcd13121d0403bcf207ec4a, Library\R.plot\plots.vb"

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
'     Function: linearRegression, plot
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
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Histogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime.Internal

<Package("plot.charts")>
Module plots

    <ExportAPI("linear.regression")>
    Public Function linearRegression(lm As IFitted) As GraphicsData
        Return RegressionPlot.Plot(lm)
    End Function

    <RInitialize>
    Sub Main()
        Call REnv.generic.add("plot", GetType(DeclareLambdaFunction), AddressOf plotFormula)
        Call REnv.generic.add("plot", GetType(ODEOutput), AddressOf plotODEResult)
        Call REnv.generic.add("plot", GetType(ODEsOut), AddressOf plot_deSolveResult)
        Call REnv.generic.add("plot", GetType(SerialData()), AddressOf plotSerials)
        Call REnv.generic.add("plot", GetType(SerialData), AddressOf plotSerials)
        Call REnv.generic.add("plot", GetType(DataBinBox(Of Double)()), AddressOf plot_binBox)
    End Sub

    Public Function plot_binBox(data As DataBinBox(Of Double)(), args As list, env As Environment) As Object
        Dim step! = args!steps
        Dim title$ = If(args!title, "Histogram Plot")

        If [step] <= 0 Then
            ' guess step value from binbox width
            [step] = data _
                .Select(Function(bin)
                            Return bin.Raw.Range.Length
                        End Function) _
                .Average
        End If

        Return data.HistogramPlot([step]:=[step])
    End Function

    Public Function plot_deSolveResult(desolve As ODEsOut, args As list, env As Environment) As Object
        Dim vector As list = args!vector
        Dim camera As Camera = args!camera
        Dim color As Color = InteropArgumentHelper.getColor(args!color, "black").TranslateColor
        Dim bg$ = InteropArgumentHelper.getColor(args!bg, "white")
        Dim title As String = Scripting.ToString(args!title, "Plot deSolve")
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
            Return REnv.debug.stop("Missing parameter 'x' for plot function!", env)
        End If

        Dim fx As Func(Of Double, Double) = math.CreateLambda(Of Double, Double)(env)
        Dim x As Double() = vector.asVector(Of Double)(args!x)
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
        Dim size As Size = InteropArgumentHelper.getSize(args!size).SizeParser
        Dim padding = InteropArgumentHelper.getPadding(args!padding)

        Return serials.Plot(
            size:=size, padding:=padding,
            xlabel:=args("x.lab"),
            ylabel:=args("y.lab")
        )
    End Function

    <ExportAPI("serial")>
    Public Function CreateSerial(x As Array, y As Array, Optional name$ = "data serial", Optional color As Object = "black") As SerialData
        Dim px As Double() = vector.asVector(Of Double)(x)
        Dim py As Double() = vector.asVector(Of Double)(y)
        Dim points As PointData() = px _
            .Select(Function(xi, i)
                        Return New PointData With {
                            .pt = New PointF(xi, y(i))
                        }
                    End Function) _
            .ToArray
        Dim serial As New SerialData With {
            .color = InteropArgumentHelper.getColor(color).TranslateColor,
            .lineType = DashStyle.Solid,
            .PointSize = 5,
            .pts = points,
            .Shape = LegendStyles.SolidLine,
            .title = name,
            .width = 5
        }

        Return serial
    End Function
End Module
