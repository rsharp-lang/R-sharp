#Region "Microsoft.VisualBasic::e3f0ac309e8658e1d0e3fd18d5d0e67d, R-sharp\studio\Rsharp_kit\MLkit\machineLearning.vb"

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

    '   Total Lines: 541
    '    Code Lines: 375
    ' Comment Lines: 107
    '   Blank Lines: 59
    '     File Size: 22.18 KB


    ' Module machineLearning
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: addSamples, addTrainingSample, ANNpredict, checkModelDataset, configuration
    '               createANN, CreateANNTrainer, createEmptyMLDataset, createNormalizationMatrix, getRawSamples
    '               inputSize, loadParallelANN, normalizeData, openDebugger, outputSize
    '               readANNModel, (+2 Overloads) runANNTraining, setTrainingSet, Softmax, tabular
    '               writeANNNetwork
    ' 
    '     Sub: doFileSave
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports DataTable = Microsoft.VisualBasic.Data.csv.IO.DataSet
Imports MLDataSet = Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure.DataSet
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# machine learning library
''' </summary>
<Package("machineLearning", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
Module machineLearning

    Sub New()
        REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(MLDataSet), AddressOf tabular)
    End Sub

    Public Function tabular(x As MLDataSet, args As list, env As Environment) As Rdataframe
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

    <ExportAPI("softmax")>
    Public Function Softmax(<RRawVectorArgument> V As Object) As Object
        Return SoftmaxLayer.Softmax(REnv.asVector(Of Double)(V)).DoCall(AddressOf vector.asVector)
    End Function

    <ExportAPI("raw_samples")>
    Public Function getRawSamples(x As MLDataSet) As Sample()
        Return x.DataSamples.items
    End Function

    ''' <summary>
    ''' add new training sample collection into the model dataset
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="samples"></param>
    ''' <param name="estimateQuantile"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("add_samples")>
    <RApiReturn(GetType(MLDataSet))>
    Public Function addSamples(x As MLDataSet,
                               <RRawVectorArgument>
                               samples As Object,
                               Optional estimateQuantile As Boolean = True,
                               Optional env As Environment = Nothing) As Object

        Dim sampleList = pipeline.TryCreatePipeline(Of Sample)(samples, env)

        If sampleList.isError Then
            Return sampleList.getError
        End If

        Return MLDataSet.JoinSamples(x, sampleList.populates(Of Sample)(env), estimateQuantile)
    End Function

    <ExportAPI("read.ANN_network")>
    Public Function readANNModel(file As String) As NeuralNetwork
        Return NeuralNetwork.LoadModel(file)
    End Function

    <ExportAPI("load.parallel_ANN")>
    Public Function loadParallelANN(dir As String, normalize As NormalizeMatrix, Optional method As Methods = Methods.NormalScaler) As ParallelNetwork
        Return ParallelNetwork.LoadSnapshot(dir, normalize, method)
    End Function

    <ExportAPI("as.ANN")>
    Public Function createANN(model As NeuralNetwork) As Network
        Return model.LoadModel
    End Function

    ''' <summary>
    ''' create a new model dataset
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("new.ML_model")>
    Public Function createEmptyMLDataset(file As String) As RDispose
        Return New RDispose(
            New MLDataSet With {
                .DataSamples = New SampleList
            },
            Sub(d)
                doFileSave(DirectCast(d, MLDataSet), file)
            End Sub)
    End Function

    Private Sub doFileSave(dataset As MLDataSet, file As String)
        dataset.NormalizeMatrix = NormalizeMatrix.CreateFromSamples(
            samples:=dataset.DataSamples.items,
            names:=dataset.width _
                .Sequence _
                .Select(Function(i) $"X_{i + 1}")
        )
        dataset _
            .GetXml _
            .SaveTo(file)
    End Sub

    ''' <summary>
    ''' add new training sample into the model dataset
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="input"></param>
    ''' <param name="output"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("add")>
    Public Function addTrainingSample(model As Object, input As Double(), output As Double(), Optional env As Environment = Nothing) As Object
        Dim dataset As MLDataSet

        If model Is Nothing Then
            Return Nothing
        ElseIf TypeOf model Is RDispose Then
            With DirectCast(model, RDispose)
                If .type Like GetType(MLDataSet) Then
                    dataset = .Value
                Else
                    Return Internal.debug.stop({
                        $"invalid model data type: { .type}!",
                        $"required: {GetType(MLDataSet).FullName}"
                    }, env)
                End If
            End With
        ElseIf TypeOf model Is MLDataSet Then
            dataset = model
        Else
            Return Internal.debug.stop({
                $"invalid model data type: {model.GetType.FullName}!",
                $"required: {GetType(MLDataSet).FullName}"
            }, env)
        End If

        Call New Sample(input) With {
            .ID = App.NextTempName,
            .target = output
        }.DoCall(AddressOf dataset.DataSamples.items.Add)

        Return model
    End Function

    <ExportAPI("ANN.predict")>
    Public Function ANNpredict(model As Object, input As Double(), Optional env As Environment = Nothing) As Object
        If model Is Nothing Then
            Return Internal.debug.stop("the required neuron network can not be nothing!", env)
        ElseIf TypeOf model Is Network Then
            Return DirectCast(model, Network).Compute(input)
        ElseIf TypeOf model Is ParallelNetwork Then
            Return DirectCast(model, ParallelNetwork).Predicts(input).ToArray
        Else
            Return Message.InCompatibleType(GetType(Network), model.GetType, env)
        End If
    End Function

    <ExportAPI("normalize")>
    Public Function normalizeData(trainingSet As NormalizeMatrix, input As Double(), Optional method As Normalizer.Methods = Normalizer.Methods.NormalScaler) As Double()
        Return trainingSet.NormalizeInput(input, method)
    End Function

    ''' <summary>
    ''' Create normalization matrix data object for the given ANN training data model
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <param name="names">the names of the input dimensions</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("create.normalize")>
    <RApiReturn(GetType(MLDataSet))>
    Public Function createNormalizationMatrix(dataset As MLDataSet,
                                              Optional names As String() = Nothing,
                                              Optional estimateQuantile As Boolean = True,
                                              Optional env As Environment = Nothing) As Object

        If dataset.DataSamples Is Nothing OrElse dataset.DataSamples.AsEnumerable.Count = 0 Then
            Return Internal.debug.stop("the required training sample data can not be empty!", env)
        End If

        If names.IsNullOrEmpty Then
            names = dataset.DataSamples _
                .AsEnumerable _
                .First _
                .vector _
                .Select(Function(x, i) $"X_{i + 1}") _
                .ToArray
        End If

        dataset.NormalizeMatrix = NormalizeMatrix.CreateFromSamples(
            samples:=dataset.DataSamples.AsEnumerable,
            names:=names,
            estimateQuantile:=estimateQuantile
        )

        Return dataset
    End Function

    ''' <summary>
    ''' check the errors that may exists in the dataset file.
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <returns></returns>
    <ExportAPI("check.ML_model")>
    Public Function checkModelDataset(dataset As MLDataSet) As LogEntry()
        Return Diagnostics.CheckDataSet(dataset).ToArray
    End Function

    ''' <summary>
    ''' save a trained ANN network model into a given xml files.
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="file">the xml file path or directory path.</param>
    ''' <param name="scattered"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("write.ANN_network")>
    <RApiReturn(GetType(Boolean))>
    Public Function writeANNNetwork(model As Object, file$, Optional scattered As Boolean = True, Optional env As Environment = Nothing) As Object
        If model Is Nothing Then
            Return False
        ElseIf TypeOf model Is Network Then
            model = StoreProcedure.NeuralNetwork.Snapshot(DirectCast(model, Network))
        ElseIf TypeOf model Is TrainingUtils Then
            model = DirectCast(model, TrainingUtils).TakeSnapshot
        ElseIf TypeOf model Is IndividualParallelTraining Then
            Call DirectCast(model, IndividualParallelTraining).Snapshot(file)
            Return True
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

    ''' <summary>
    ''' open a file connection to the model debug file.
    ''' </summary>
    ''' <param name="ANN"></param>
    ''' <param name="file">the file path to the debug file.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
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

    ''' <summary>
    ''' create a new ANN training model
    ''' </summary>
    ''' <param name="inputSize">the number of nodes for the input layer</param>
    ''' <param name="outputSize">the number of nodes for the output layer</param>
    ''' <param name="hiddenSize">the hiden size of nodes for each hidden layers</param>
    ''' <param name="learnRate"></param>
    ''' <param name="momentum"></param>
    ''' <param name="active"></param>
    ''' <param name="weight0"></param>
    ''' <param name="learnRateDecay"></param>
    ''' <param name="truncate"></param>
    ''' <returns></returns>
    <ExportAPI("ANN.training_model")>
    Public Function CreateANNTrainer(inputSize%, outputSize%,
                                     <RRawVectorArgument(GetType(Integer))>
                                     Optional hiddenSize As Object = "25,100,30",
                                     Optional learnRate As Double = 0.1,
                                     Optional momentum As Double = 0.9,
                                     <RDefaultValue("hidden: Sigmoid(alpha:=2.0); output: Sigmoid(alpha:=2.0)")>
                                     Optional active As activation = Nothing,
                                     Optional weight0 As Object = "random",
                                     Optional learnRateDecay As Double = 0.0000000001,
                                     Optional truncate As Double = -1,
                                     Optional split As Boolean = False) As ANNTrainer
        Dim w0 As Func(Of Double)
        Dim sizeVec As Integer() = REnv.asVector(Of Integer)(hiddenSize)

        If weight0 Is Nothing OrElse Scripting.ToString(REnv.getFirst(weight0)) = "random" Then
            w0 = Helpers.RandomWeightInitializer
        Else
            w0 = Helpers.UnifyWeightInitializer(REnv.asVector(Of Double)(weight0).GetValue(Scan0))
        End If

        Dim trainingHelper As ANNTrainer

        If split Then
            trainingHelper = New IndividualParallelTraining(
                inputSize, sizeVec,
                outputSize,
                learnRate,
                momentum,
                active.CreateActivations,
                weightInit:=w0
            )
        Else
            trainingHelper = New TrainingUtils(
                inputSize, sizeVec,
                outputSize,
                learnRate,
                momentum,
                active.CreateActivations,
                weightInit:=w0
            )
        End If

        trainingHelper.NeuronNetwork.LearnRateDecay = learnRateDecay
        trainingHelper.Truncate = truncate

        Return trainingHelper
    End Function

    ''' <summary>
    ''' Apply configuration on the ANN training model.
    ''' </summary>
    ''' <param name="util"></param>
    ''' <param name="dropout">
    ''' a percentage value range in [0,1].
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("configuration")>
    Public Function configuration(util As ANNTrainer,
                                  Optional softmax As Boolean? = Nothing,
                                  Optional selectiveMode As Boolean? = Nothing,
                                  Optional dropout As Double? = Nothing,
                                  Optional snapshotLocation As String = "NA") As ANNTrainer

        If Not softmax Is Nothing Then
            util = util.SetLayerNormalize(opt:=softmax)
        End If
        If Not dropout Is Nothing Then
            util = util.SetDropOut(percentage:=dropout)
        End If
        If Not selectiveMode Is Nothing Then
            util = util.SetSelective(opt:=selectiveMode)
        End If
        If Not snapshotLocation = "NA" Then
            util = util.SetSnapshotLocation(snapshotLocation)
        End If

        Return util
    End Function

    <ExportAPI("input.size")>
    Public Function inputSize(trainSet As MLDataSet) As Integer
        Return trainSet.Size.Width
    End Function

    <ExportAPI("output.size")>
    Public Function outputSize(trainSet As MLDataSet) As Integer
        Return trainSet.OutputSize
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="ann"></param>
    ''' <param name="trainingSet"></param>
    ''' <param name="normalMethod"></param>
    ''' <param name="attribute">
    ''' run training for a single output or all of the result output.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("set.trainingSet")>
    Public Function setTrainingSet(ann As ANNTrainer,
                                   trainingSet As MLDataSet,
                                   Optional normalMethod As Methods = Methods.RelativeScaler,
                                   Optional attribute% = -1,
                                   Optional setOutputNames As Boolean = True) As ANNTrainer

        For Each sample As Sample In trainingSet.PopulateNormalizedSamples(method:=normalMethod)
            If attribute < 0 Then
                Call ann.Add(sample.vector, sample.target)
            Else
                Call ann.Add(sample.vector, sample.target(attribute))
            End If
        Next

        If setOutputNames Then
            Call ann.SetOutputNames(trainingSet.output)
        End If

        Return ann
    End Function

    <ExportAPI("training")>
    Public Function runANNTraining(training As ANNTrainer,
                                   Optional maxIterations As Integer = 10000,
                                   Optional minErr As Double = 0.01,
                                   Optional parallel As Boolean = True) As ANNTrainer

        Helpers.MaxEpochs = maxIterations
        Helpers.MinimumError = minErr

        Call training _
            .AttachReporter(Sub(i, err, ANN)
                            End Sub) _
            .Train(parallel)

        Return training
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
    ''' <param name="attribute">
    ''' run training for a single output or all of the result output.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("training.ANN")>
    <RApiReturn(GetType(StoreProcedure.NeuralNetwork), GetType(Network))>
    Public Function runANNTraining(trainSet As MLDataSet,
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
                                   Optional softmax As Boolean = True,
                                   Optional selectiveMode As Boolean = False,
                                   Optional dropout As Double = 0,
                                   Optional maxIterations As Integer = 10000,
                                   Optional minErr As Double = 0.01,
                                   Optional parallel As Boolean = True,
                                   Optional outputSnapshot As Boolean = False,
                                   Optional attribute% = -1) As Object

        Dim trainingHelper As TrainingUtils = CreateANNTrainer(
            inputSize:=trainSet.Size.Width,
            outputSize:=trainSet.OutputSize,
            hiddenSize:=hiddenSize,
            learnRate:=learnRate,
            momentum:=momentum,
            weight0:=weight0,
            learnRateDecay:=learnRateDecay,
            truncate:=truncate,
            active:=active
        ).SetLayerNormalize(opt:=softmax) _
         .SetDropOut(percentage:=dropout) _
         .SetSelective(opt:=selectiveMode)

        For Each sample As Sample In trainSet.PopulateNormalizedSamples(method:=normalMethod)
            If attribute < 0 Then
                Call trainingHelper.Add(sample.vector, sample.target)
            Else
                Call trainingHelper.Add(sample.vector, sample.target(attribute))
            End If
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
