#Region "Microsoft.VisualBasic::fa7a4f73206848acb9febdd44eaeb4e3, studio\Rsharp_kit\MLkit\MachineLearning\machineLearning.vb"

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

    '   Total Lines: 545
    '    Code Lines: 379 (69.54%)
    ' Comment Lines: 107 (19.63%)
    '    - Xml Docs: 98.13%
    ' 
    '   Blank Lines: 59 (10.83%)
    '     File Size: 22.41 KB


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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports DataTable = Microsoft.VisualBasic.Data.Framework.IO.DataSet
Imports MLDataSet = Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure.DataSet
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' R# machine learning library
''' </summary>
<Package("machineLearning", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
Module machineLearningTools

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
        Return MachineLearning.Softmax(CLRVector.asNumeric(V)).DoCall(AddressOf vector.asVector)
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
            samples:=dataset.DataSamples,
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
                    Return RInternal.debug.stop({
                        $"invalid model data type: { .type}!",
                        $"required: {GetType(MLDataSet).FullName}"
                    }, env)
                End If
            End With
        ElseIf TypeOf model Is MLDataSet Then
            dataset = model
        Else
            Return RInternal.debug.stop({
                $"invalid model data type: {model.GetType.FullName}!",
                $"required: {GetType(MLDataSet).FullName}"
            }, env)
        End If

        Call New Sample(input) With {
            .ID = App.NextTempName,
            .target = output
        }.DoCall(AddressOf dataset.DataSamples.items.Append)

        Return model
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
            Return RInternal.debug.stop("the required training sample data can not be empty!", env)
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

    <ExportAPI("input.size")>
    Public Function inputSize(trainSet As MLDataSet) As Integer
        Return trainSet.Size.Width
    End Function

    <ExportAPI("output.size")>
    Public Function outputSize(trainSet As MLDataSet) As Integer
        Return trainSet.OutputSize
    End Function
End Module
