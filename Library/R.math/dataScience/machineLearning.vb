Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# machine learning library
''' </summary>
<Package("machineLearning")>
Module machineLearning

    <ExportAPI("read.ML_model")>
    Public Function readModelDataset(file As String) As StoreProcedure.DataSet
        Return file.LoadXml(Of StoreProcedure.DataSet)
    End Function

    <ExportAPI("training.ANN")>
    Public Function runANNTraining(model As StoreProcedure.DataSet,
                                   <RRawVectorArgument(GetType(Integer))>
                                   Optional hiddenSize As Object = "25,100,30",
                                   Optional learnRate As Double = 0.1,
                                   Optional momentum As Double = 0.9,
                                   Optional weight0 As Object = "random",
                                   <RDefaultValue("hidden: Sigmoid(alpha:=2.0); output: Sigmoid(alpha:=2.0)")>
                                   Optional active As activation = Nothing,
                                   Optional normalMethod As Methods = Methods.RelativeScaler,
                                   Optional learnRateDecay As Double = 0.0000000001,
                                   Optional truncate As Double = -1,
                                   Optional selectiveMode As Boolean = False,
                                   Optional maxIterations As Integer = 10000,
                                   Optional minErr As Double = 0.01,
                                   Optional parallel As Boolean = True) As StoreProcedure.NeuralNetwork

        Dim w0 As Func(Of Double)

        If weight0 Is Nothing OrElse Scripting.ToString(REnv.getFirst(weight0)) = "random" Then
            w0 = Helpers.RandomWeightInitializer
        Else
            w0 = Helpers.UnifyWeightInitializer(REnv.asVector(Of Double)(weight0).GetValue(Scan0))
        End If

        Dim trainingHelper As New TrainingUtils(
            model.Size.Width, hiddenSize,
            model.OutputSize,
            learnRate,
            momentum,
            active.CreateActivations,
            weightInit:=w0
        ) With {.Selective = selectiveMode}

        trainingHelper.NeuronNetwork.LearnRateDecay = learnRateDecay
        trainingHelper.Truncate = truncate

        For Each sample As Sample In model.PopulateNormalizedSamples(method:=normalMethod)
            Call trainingHelper.Add(sample.status, sample.target)
        Next

        Helpers.MaxEpochs = maxIterations
        Helpers.MinimumError = minErr

        Call trainingHelper _
            .AttachReporter(Sub(i, err, ANN)
                            End Sub) _
            .Train(parallel)

        Return trainingHelper.TakeSnapshot
    End Function
End Module

Public Class activation

    Public Property hidden As String
    Public Property output As String

    Public Function CreateActivations() As LayerActives
        Dim defaultActive As [Default](Of String) = ActiveFunction.Sigmoid

        Return New LayerActives With {
            .hiddens = ActiveFunction.Parse(hidden Or defaultActive),
            .output = ActiveFunction.Parse(output Or defaultActive),
            .input = ActiveFunction.Parse(defaultActive)
        }
    End Function

    Public Overloads Shared Widening Operator CType([default] As String) As activation
        Dim tokens As String() = [default].StringSplit(";\s*")
        Dim actives As New activation
        Dim configs As NamedValue(Of String)() = tokens _
            .Select(Function(str)
                        Return str.GetTagValue(":", trim:=True)
                    End Function) _
            .ToArray

        actives.hidden = configs.Where(Function(a) a.Name.TextEquals(NameOf(activation.hidden))).FirstOrDefault.Value
        actives.output = configs.Where(Function(a) a.Name.TextEquals(NameOf(activation.output))).FirstOrDefault.Value

        Return actives
    End Operator
End Class
