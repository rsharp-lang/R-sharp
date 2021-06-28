#Region "Microsoft.VisualBasic::0d443fee17914e9819d1c37b8d495c73, studio\Rsharp_kit\MLkit\validation.vb"

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

' Module validation
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: ANN_ROC, AUC, createSampleValidation, Tabular
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.My.JavaScript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("validation", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
Module validation

    Sub New()
        REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(Evaluation.Validation()), AddressOf Tabular)
    End Sub

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

    <ExportAPI("prediction")>
    Public Function prediction(predicts As Double(), labels As Boolean()) As ROC
        Dim result = Evaluation.Validation _
            .ROC(predicts.Sequence, Function(i, d) labels(i), Function(i, cut) predicts(i) >= cut) _
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
    Public Function ANN_ROC(ANN As Network, validateSet As TrainingSample(), range As Double(), attribute%, Optional n% = 20) As Evaluation.Validation()
        Return ANN.CreateValidateResult(validateSet).ROC(range, attribute, n)
    End Function

    <ExportAPI("as.validation")>
    Public Function createSampleValidation(trainingSet As NormalizeMatrix, input As Double(), validate As Double(), Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler) As TrainingSample
        Return New TrainingSample With {
            .classify = validate,
            .sample = trainingSet.NormalizeInput(input, method),
            .sampleID = App.NextTempName
        }
    End Function
End Module

Public Class ROC : Inherits JavaScriptObject

    Public Property threshold As Double()
    Public Property specificity As Double()
    ''' <summary>
    ''' TPR
    ''' </summary>
    ''' <returns></returns>
    Public Property sensibility As Double()
    Public Property accuracy As Double()
    Public Property precision As Double()
    Public Property BER As Double()
    Public Property FPR As Double()
    Public Property NPV As Double()
    Public Property F1Score As Double()
    Public Property F2Score As Double()
    Public Property All As Integer()
    Public Property TP As Integer()
    Public Property FP As Integer()
    Public Property TN As Integer()
    Public Property FN As Integer()

    Public Function AUC() As Double
        Dim TPR = sensibility
        Dim FPR = Me.FPR

        With TPR.Select(Function(x, i) (x, i)).OrderBy(Function(t) t.x).ToArray
            TPR = .Select(Function(t) t.x).ToArray
            FPR = .Select(Function(t) FPR(t.i)).ToArray
        End With

        Return Evaluation.SimpleAUC(TPR, FPR)
    End Function

End Class