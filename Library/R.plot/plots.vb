#Region "Microsoft.VisualBasic::112fe60991e830a8a6f915f584b2fdd3, Library\R.plot\plots.vb"

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
'     Function: linearRegression
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging.Driver
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
        Call REnv.generic.add("plot", GetType(DeclareLambdaFunction), AddressOf plot)
    End Sub

    ''' <summary>
    ''' plot the math function
    ''' </summary>
    ''' <param name="math">y = f(x)</param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Function plot(math As DeclareLambdaFunction, args As list, env As Environment) As Object
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
End Module
