﻿#Region "Microsoft.VisualBasic::24a0cd932c25c9155214506344c992f7, studio\Rsharp_kit\MLkit\validation.vb"

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

    '   Total Lines: 208
    '    Code Lines: 162 (77.88%)
    ' Comment Lines: 27 (12.98%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (9.13%)
    '     File Size: 9.73 KB


    ' Module validation
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ANN_ROC, AUC, createSampleValidation, fake, PlotROC
    '               prediction, roc_auc_score, ROC_tabular, Tabular
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

<Package("validation", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
Module validation

    Sub New()
        RInternal.Object.Converts.makeDataframe.addHandler(GetType(Evaluation.Validation()), AddressOf Tabular)
        RInternal.Object.Converts.makeDataframe.addHandler(GetType(ROC), AddressOf ROC_tabular)

        RInternal.generic.add("plot", GetType(ROC), AddressOf PlotROC)
    End Sub

    <RGenericOverloads("plot")>
    Public Function PlotROC(roc As ROC, args As list, env As Environment) As Object
        Dim line As SerialData = ROCPlot.CreateSerial(roc)
        Dim size As String = InteropArgumentHelper.getSize(args!size, env, "2700,2400")
        Dim type As Drivers = env.getDriver
        Dim dpi As Integer = graphicsPipeline.getDpi(args.slots, env, [default]:=100)

        line.color = args.getValue("line_color", env, "steelblue").TranslateColor
        line.lineType = DashStyle.Dash
        line.pointSize = args.getValue("point_size", env, 5)
        line.shape = LegendStyles.Circle
        line.width = args.getValue("line_width", env, 1)
        line.title = roc.AUC.ToString("F3")

        Return ROCPlot.Plot(line, size:=size, dpi:=dpi, driver:=type)
    End Function

    <RGenericOverloads("as.data.frame")>
    Public Function ROC_tabular(x As ROC, args As list, env As Environment) As Rdataframe
        Return New Rdataframe With {
            .rownames = CLRVector.asCharacter(x.threshold),
            .columns = New Dictionary(Of String, Array) From {
                {"threshold", x.threshold},
                {"specificity", x.specificity},
                {"sensibility", x.sensibility},
                {"accuracy", x.accuracy},
                {"precision", x.precision},
                {"BER", x.BER},
                {"FPR", x.FPR},
                {"NPV", x.NPV},
                {"F1Score", x.F1Score},
                {"F2Score", x.F2Score},
                {"All", x.All},
                {"TP", x.TP},
                {"FP", x.FP},
                {"TN", x.TN},
                {"FN", x.FN}
            }
        }
    End Function

    <RGenericOverloads("as.data.frame")>
    Public Function Tabular(x As Object, args As list, env As Environment) As Rdataframe
        Dim input As Evaluation.Validation() = DirectCast(x, Evaluation.Validation())
        Dim dataframe As New Rdataframe With {
            .rownames = input.Select(Function(r) CStr(r.Threshold)).ToArray,
            .columns = New Dictionary(Of String, Array) From {
                {"threshold", .rownames.Select(AddressOf Val).ToArray},
                {"specificity", input.Select(Function(r) r.Specificity).ToArray},
                {"sensibility", input.Select(Function(r) r.Sensibility).ToArray},
                {"accuracy", input.Select(Function(r) r.Accuracy).ToArray},
                {"precision", input.Select(Function(r) r.Precision).ToArray},
                {"BER", input.Select(Function(r) r.BER).ToArray},
                {"FPR", input.Select(Function(r) r.FPR).ToArray},
                {"NPV", input.Select(Function(r) r.NPV).ToArray},
                {"F1Score", input.Select(Function(r) r.F1Score).ToArray},
                {"F2Score", input.Select(Function(r) r.FbetaScore(2)).ToArray},
                {"All", input.Select(Function(r) r.All).ToArray},
                {"TP", input.Select(Function(r) r.TP).ToArray},
                {"FP", input.Select(Function(r) r.FP).ToArray},
                {"TN", input.Select(Function(r) r.TN).ToArray},
                {"FN", input.Select(Function(r) r.FN).ToArray}
            }
        }

        Return dataframe
    End Function

    <ExportAPI("AUC")>
    <RApiReturn(GetType(Double))>
    Public Function AUC(<RRawVectorArgument> ROC As Object, Optional env As Environment = Nothing) As Object
        If TypeOf ROC Is ROC Then
            Return DirectCast(ROC, ROC).AUC
        Else
            Dim validates As pipeline = pipeline.TryCreatePipeline(Of Evaluation.Validation)(ROC, env)

            If validates.isError Then
                Return validates.getError
            End If

            Return validates.populates(Of Evaluation.Validation)(env).AUC
        End If
    End Function

    ''' <summary>
    ''' Compute Area Under the Receiver Operating Characteristic Curve (ROC AUC) from prediction scores.
    ''' </summary>
    ''' <param name="y_true">
    ''' array-like of shape (n_samples,) or (n_samples, n_classes)
    ''' True binary labels Or binary label indicators. The multiclass case
    ''' expects shape(n_samples,) And labels With values In 0, 1, ..., n_classes-1.
    ''' </param>
    ''' <param name="pred">array-like of shape (n_samples,) or (n_samples, n_classes)
    ''' Target scores, can either be probability estimates Of the positive
    ''' Class, confidence values, Or non-thresholded measure of decisions
    ''' (as returned by "decision_function" on some classifiers).</param>
    ''' <returns></returns>
    <ExportAPI("roc_auc_score")>
    Public Function roc_auc_score(y_true As Double(), pred As Double()) As Double
        Return Evaluation.AUC(pred, y_true)
    End Function

    ''' <summary>
    ''' Make a fake result vector for run test
    ''' </summary>
    ''' <param name="labels"></param>
    ''' <param name="auc"></param>
    ''' <param name="cutoff"></param>
    ''' <returns></returns>
    <ExportAPI("fake_result")>
    Public Function fake(labels As Double(), auc As Double, Optional cutoff As Double = 0.5) As Object
        Return FakeAUCGenerator.BuildOutput2(labels, auc, cutoff)
    End Function

    ''' <summary>
    ''' construct a ROC validation result object
    ''' </summary>
    ''' <param name="predicts"></param>
    ''' <param name="labels"></param>
    ''' <param name="resolution"></param>
    ''' <returns></returns>
    <ExportAPI("prediction")>
    Public Function prediction(predicts As Double(), labels As Double(), Optional resolution As Integer = 1000) As ROC
        Dim ROCthreshold As New Sequence(labels.Min, labels.Max, resolution)
        Dim result = Evaluation.Validation _
            .ROC(Of Integer)(
                entity:=predicts.Sequence,
                getValidate:=Function(i, d) labels(i) >= d,
                getPredict:=Function(i, cut) predicts(i) >= cut,
                threshold:=ROCthreshold
            ) _
            .OrderBy(Function(t) t.Sensibility) _
            .ToArray
        Dim ROC As New ROC With {
            .accuracy = result.Select(Function(x) x.Accuracy).ToArray,
            .All = result.Select(Function(x) x.All).ToArray,
            .BER = result.Select(Function(x) x.BER).ToArray,
            .F1Score = result.Select(Function(x) x.F1Score).ToArray,
            .F2Score = result.Select(Function(x) x.FbetaScore(2)).ToArray,
            .FN = result.Select(Function(x) x.FN).ToArray,
            .FP = result.Select(Function(x) x.FP).ToArray,
            .FPR = result.Select(Function(x) x.FPR).ToArray,
            .NPV = result.Select(Function(x) x.NPV).ToArray,
            .precision = result.Select(Function(x) x.Precision).ToArray,
            .sensibility = result.Select(Function(x) x.Sensibility).ToArray,
            .specificity = result.Select(Function(x) x.Specificity).ToArray,
            .threshold = result.Select(Function(x) x.Threshold).ToArray,
            .TN = result.Select(Function(x) x.TN).ToArray,
            .TP = result.Select(Function(x) x.TP).ToArray
        }

        Return ROC
    End Function

    <ExportAPI("ANN.ROC")>
    Public Function ANN_ROC(ANN As Network,
                            validateSet As TrainingSample(),
                            range As Double(),
                            attribute%,
                            Optional n% = 20) As Evaluation.Validation()

        Return ANN.CreateValidateResult(validateSet).ROC(range, attribute, n)
    End Function

    <ExportAPI("as.validation")>
    Public Function createSampleValidation(trainingSet As NormalizeMatrix,
                                           input As Double(),
                                           validate As Double(),
                                           Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler) As TrainingSample
        Return New TrainingSample With {
            .classify = validate,
            .sample = trainingSet.NormalizeInput(input, method),
            .sampleID = App.NextTempName
        }
    End Function
End Module
