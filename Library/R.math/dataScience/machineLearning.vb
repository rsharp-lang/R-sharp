Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports DataTable = Microsoft.VisualBasic.Data.csv.IO.DataSet
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# machine learning library
''' </summary>
<Package("machineLearning", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
Module machineLearning

    Sub New()
        REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(StoreProcedure.DataSet), AddressOf Tabular)
    End Sub

    Public Function Tabular(x As StoreProcedure.DataSet, args As list, env As Environment) As Rdataframe
        Dim markOuput As Boolean = args.getValue(Of Boolean)("mark.output", env)
        Dim table As DataTable() = x.ToTable(markOuput).ToArray
        Dim a As Array
        Dim dataframe As New Rdataframe With {
            .rownames = table.Keys,
            .columns = New Dictionary(Of String, Array) From {
                {NameOf(DataTable.ID), .rownames}
            }
        }

        For Each col As String In table.PropertyNames
            a = table.Vector(col)
            dataframe.columns.Add(col, a)
        Next

        Return dataframe
    End Function

    ''' <summary>
    ''' convert machine learning dataset to dataframe table.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="markOuput"></param>
    ''' <returns></returns>
    <ExportAPI("as.tabular")>
    Public Function Tabular(x As StoreProcedure.DataSet, Optional markOuput As Boolean = True) As DataTable()
        Return x.ToTable(markOuput).ToArray
    End Function

    ''' <summary>
    ''' read the dataset for training the machine learning model
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("read.ML_model")>
    Public Function readModelDataset(file As String) As StoreProcedure.DataSet
        Return file.LoadXml(Of StoreProcedure.DataSet)
    End Function

    <ExportAPI("new.ML_model")>
    Public Function createEmptyMLDataset(file As String) As RDispose
        Return New RDispose(
            New StoreProcedure.DataSet With {.DataSamples = New SampleList},
            Sub(d)
                Dim dataset As StoreProcedure.DataSet = DirectCast(d, StoreProcedure.DataSet)

                dataset.NormalizeMatrix = NormalizeMatrix.CreateFromSamples(
                    samples:=dataset.DataSamples.items,
                    names:=dataset.width _
                        .Sequence _
                        .Select(Function(i) $"X_{i}")
                )
                dataset _
                    .GetXml _
                    .SaveTo(file)
            End Sub)
    End Function

    <ExportAPI("add")>
    Public Function addTrainingSample(model As Object, input As Double(), output As Double(), Optional env As Environment = Nothing) As Object
        Dim dataset As StoreProcedure.DataSet

        If model Is Nothing Then
            Return Nothing
        ElseIf TypeOf model Is RDispose Then
            With DirectCast(model, RDispose)
                If .type Like GetType(StoreProcedure.DataSet) Then
                    dataset = .Value
                Else
                    Return Internal.debug.stop({
                        $"invalid model data type: { .type}!",
                        $"required: {GetType(StoreProcedure.DataSet).FullName}"
                    }, env)
                End If
            End With
        ElseIf TypeOf model Is StoreProcedure.DataSet Then
            dataset = model
        Else
            Return Internal.debug.stop({
                $"invalid model data type: {model.GetType.FullName}!",
                $"required: {GetType(StoreProcedure.DataSet).FullName}"
            }, env)
        End If

        dataset.DataSamples.items.Add(New Sample(input) With {.ID = App.NextTempName, .target = output})

        Return model
    End Function

    ''' <summary>
    ''' check the errors that may exists in the dataset file.
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <returns></returns>
    <ExportAPI("check.ML_model")>
    Public Function checkModelDataset(dataset As StoreProcedure.DataSet) As LogEntry()
        Return Diagnostics.CheckDataSet(dataset).ToArray
    End Function

    ''' <summary>
    ''' save a trained ANN network model into a given xml files.
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="file$"></param>
    ''' <param name="scattered"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("write.ANN_network")>
    Public Function writeANNNetwork(model As Object, file$, Optional scattered As Boolean = True, Optional env As Environment = Nothing) As Object
        If model Is Nothing Then
            Return False
        ElseIf TypeOf model Is Network Then
            model = StoreProcedure.NeuralNetwork.Snapshot(DirectCast(model, Network))
        ElseIf Not TypeOf model Is NeuralNetwork Then
            Return Internal.debug.stop({
                $"invalid data type for save: {model.GetType.FullName}",
                $"required: {GetType(NeuralNetwork).FullName}"
            }, env)
        End If

        With DirectCast(model, NeuralNetwork)
            If Not scattered Then
                Return .GetXml.SaveTo(file)
            Else
                Return .ScatteredStore(file)
            End If
        End With
    End Function

    <ExportAPI("open.debugger")>
    <RApiReturn(GetType(ANNDebugger))>
    Public Function openDebugger(ANN As Object, file$, Optional env As Environment = Nothing) As Object
        Dim model As Network

        If ANN Is Nothing Then
            Return Internal.debug.stop("the ANN network model can Not be nothing!", env)
        ElseIf TypeOf ANN Is TrainingUtils Then
            ANN = DirectCast(ANN, TrainingUtils).NeuronNetwork
        End If

        If Not TypeOf ANN Is Network Then
            Return Internal.debug.stop({
                $"unsupported object type: {ANN.GetType.FullName}!",
                $"required: {GetType(Network).FullName}"
            }, env)
        Else
            model = ANN
        End If

        Return New RDispose(
            x:=New ANNDebugger(model),
            final:=Sub(debugger)
                       DirectCast(debugger, ANNDebugger).Save(file, model)
                   End Sub)
    End Function

    <ExportAPI("ANN.training_model")>
    Public Function CreateANNTrainer(inputSize%, outputSize%,
                                     <RRawVectorArgument(GetType(Integer))>
                                     Optional hiddenSize As Object = "25,100,30",
                                     Optional learnRate As Double = 0.1,
                                     Optional momentum As Double = 0.9,
                                     Optional active As activation = Nothing,
                                     Optional weight0 As Object = "random",
                                     Optional learnRateDecay As Double = 0.0000000001,
                                     Optional truncate As Double = -1,
                                     Optional selectiveMode As Boolean = False) As TrainingUtils
        Dim w0 As Func(Of Double)
        Dim sizeVec As Integer() = REnv.asVector(Of Integer)(hiddenSize)

        If weight0 Is Nothing OrElse Scripting.ToString(REnv.getFirst(weight0)) = "random" Then
            w0 = Helpers.RandomWeightInitializer
        Else
            w0 = Helpers.UnifyWeightInitializer(REnv.asVector(Of Double)(weight0).GetValue(Scan0))
        End If

        Dim trainingHelper As New TrainingUtils(
            inputSize, sizeVec,
            outputSize,
            learnRate,
            momentum,
            active.CreateActivations,
            weightInit:=w0
        ) With {.Selective = selectiveMode}

        trainingHelper.NeuronNetwork.LearnRateDecay = learnRateDecay
        trainingHelper.Truncate = truncate

        Return trainingHelper
    End Function

    ''' <summary>
    ''' do ANN model training
    ''' </summary>
    ''' <param name="trainSet">A dataset object that used for ANN model training.</param>
    ''' <param name="hiddenSize">An integer vector for indicates the network size of the hidden layers in the ANN network.</param>
    ''' <param name="learnRate"></param>
    ''' <param name="momentum"></param>
    ''' <param name="weight0">weight method for initialize the ANN network model.</param>
    ''' <param name="active"></param>
    ''' <param name="normalMethod"></param>
    ''' <param name="learnRateDecay"></param>
    ''' <param name="truncate"></param>
    ''' <param name="selectiveMode"></param>
    ''' <param name="maxIterations"></param>
    ''' <param name="minErr"></param>
    ''' <param name="parallel"></param>
    ''' <param name="outputSnapshot">
    ''' this parameter will config the output object type. this function is returns the raw ANN model 
    ''' by default, and you can change the output type to file model by set this parameter value to 
    ''' ``TRUE``. 
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("training.ANN")>
    <RApiReturn(GetType(StoreProcedure.NeuralNetwork), GetType(Network))>
    Public Function runANNTraining(trainSet As StoreProcedure.DataSet,
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
                                   Optional parallel As Boolean = True,
                                   Optional outputSnapshot As Boolean = False) As Object

        Dim trainingHelper As TrainingUtils = CreateANNTrainer(
            inputSize:=trainSet.Size.Width,
            outputSize:=trainSet.OutputSize,
            hiddenSize:=hiddenSize,
            learnRate:=learnRate,
            momentum:=momentum,
            weight0:=weight0,
            learnRateDecay:=learnRateDecay,
            truncate:=truncate,
            selectiveMode:=selectiveMode,
            active:=active
        )

        For Each sample As Sample In trainSet.PopulateNormalizedSamples(method:=normalMethod)
            Call trainingHelper.Add(sample.vector, sample.target)
        Next

        Helpers.MaxEpochs = maxIterations
        Helpers.MinimumError = minErr

        Call trainingHelper _
            .AttachReporter(Sub(i, err, ANN)
                            End Sub) _
            .Train(parallel)

        If outputSnapshot Then
            Return trainingHelper.TakeSnapshot
        Else
            Return trainingHelper.NeuronNetwork
        End If
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
