Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("machineLearning.validation", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
Module validation

    Sub New()
        REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(Evaluation.Validation()), AddressOf Tabular)
    End Sub

    Public Function Tabular(x As Object, args As list, env As Environment) As Rdataframe
        Dim input As Evaluation.Validation() = DirectCast(x, Evaluation.Validation())
        Dim dataframe As New Rdataframe With {
            .rownames = input.Select(Function(r) CStr(r.Threshold)).ToArray,
            .columns = New Dictionary(Of String, Array) From {
                {"threshold", .rownames},
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
