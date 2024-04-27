#Region "Microsoft.VisualBasic::6d7aa6bc53f7e7ec55817b9b563e761a, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//dataset/datasetKit.vb"

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

    '   Total Lines: 975
    '    Code Lines: 702
    ' Comment Lines: 158
    '   Blank Lines: 115
    '     File Size: 40.97 KB


    ' Module datasetKit
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: addRow, binEncoder, boolEncoder, CreateMLdataset, dataDescription
    '               demoMatrix, dimensionRange, EmbeddingRender, Encoding, estimate_alphabets
    '               factorEncoder, fitSgt, get_feature, getDataSetDimension, getMNISTImageSize
    '               getMNISTRawDataset, getNormalizeMatrix, mapEncoder, mapLambda, project_features
    '               q_factors, readMNISTLabelledVector, readModelDataset, readSampleSet, sample_id
    '               sampledataDataSet, SampleList, SGT, sort_samples, sortById
    '               split_training_test, Tabular, TakeSubset, toDataframe, toFeatureSet
    '               toMatrix, writeMLDataset, writeSampleSet
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.FeatureFrame
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure.DataPack
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports DataTable = Microsoft.VisualBasic.Data.csv.IO.DataSet
Imports FeatureFrame = Microsoft.VisualBasic.Math.DataFrame.DataFrame
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' ### the machine learning dataset toolkit
''' 
''' Datasets are collections of raw data gathered during the research process 
''' usually in the form of numerical data. Many organizations, e.g. government 
''' agencies, universities or research institutions make the data they have 
''' collected freely available on the web for other researchers to use.
''' </summary>
<Package("dataset", Category:=APICategories.UtilityTools)>
<RTypeExport("data_matrix", GetType(UnionMatrix))>
Module datasetKit

    Sub New()
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(FeatureFrame), AddressOf toDataframe)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(UnionMatrix), AddressOf toMatrix)
        Call REnv.Internal.generic.add("fit", GetType(SequenceGraphTransform), AddressOf fitSgt)
        Call REnv.Internal.generic.add("dim", GetType(DataSet), AddressOf getDataSetDimension)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(SampleData()), AddressOf sampledataDataSet)
    End Sub

    <RGenericOverloads("as.data.frame")>
    Private Function sampledataDataSet(ds As SampleData(), args As list, env As Environment) As Object
        Dim features As Integer = ds(0).features.Length
        Dim labels As Integer = ds(0).labels.Length
        Dim featureNames As String() = args.getValue(Of String())("feature.names", env)
        Dim labelText As String() = args.getValue(Of String())("label.names", env)

        If featureNames.IsNullOrEmpty Then
            featureNames = features.Sequence.Select(Function(i) $"X{i + 1}").ToArray
        End If
        If labelText.IsNullOrEmpty Then
            labelText = labels.Sequence.Select(Function(i) $"OUT{i + 1}").ToArray
        End If

        Dim d As New Dictionary(Of String, List(Of Double))
        Dim rownames As New List(Of String)
        Dim idx As i32 = 1

        For Each name In featureNames
            d.Add(name, New List(Of Double))
        Next
        For Each name In labelText
            d.Add(name, New List(Of Double))
        Next

        For Each x As SampleData In ds
            For i As Integer = 0 To features - 1
                d(featureNames(i)).Add(x.features(i))
            Next
            For i As Integer = 0 To labels - 1
                d(labelText(i)).Add(x.labels(i))
            Next

            rownames.Add(If(x.id, (++idx).ToString))
        Next

        Return New Rdataframe With {
            .columns = d _
                .ToDictionary(Function(x) x.Key,
                              Function(x)
                                  Return DirectCast(x.Value.ToArray, Array)
                              End Function),
                .rownames = rownames.ToArray
        }
    End Function

    Private Function getDataSetDimension(x As DataSet, args As list, env As Environment) As Object
        Dim dims As New list With {
            .slots = New Dictionary(Of String, Object)
        }
        Dim idset As String() = x.DataSamples.AsEnumerable.Select(Function(a) a.ID).ToArray
        Dim features As String() = x.NormalizeMatrix.names
        Dim labels As String() = x.output

        Call dims.add("samples", idset)
        Call dims.add("features", features)
        Call dims.add("outputs", labels)

        Return dims
    End Function

    <RGenericOverloads("as.data.frame")>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function toMatrix(data As UnionMatrix, args As list, env As Environment) As Rdataframe
        Return data.CreateMatrix
    End Function

    <RGenericOverloads("as.data.frame")>
    Private Function fitSgt(sgt As SequenceGraphTransform, args As list, env As Environment) As Object
        Dim sequence As Object = args.getBySynonyms("sequence", "seq", "seqs", "sequences")
        Dim parallel As Boolean = args.getValue(Of Boolean)({"parallel", "par"}, env, [default]:=False)
        Dim result = env.EvaluateFramework(Of String, Dictionary(Of String, Double))(sequence, AddressOf sgt.fit, parallel:=parallel)
        Dim as_df As Boolean = args.getValue({"dataframe", "df", "matrix"}, env, [default]:=False)

        If Not as_df Then
            Return result
        End If

        Dim df As New Rdataframe With {.columns = New Dictionary(Of String, Array)}

        If TypeOf sequence Is list Then
            df.rownames = DirectCast(sequence, list).getNames
        Else
#Disable Warning
            df.rownames = REnv.asVector(Of Object)(sequence) _
                .AsObjectEnumerator _
                .Select(Function(obj) any.ToString(obj)) _
                .ToArray
#Enable Warning
        End If

        Dim matrix As Dictionary(Of String, Double)()
        Dim v As Double()

        If TypeOf result Is list Then
            matrix = df.rownames _
                .Select(Function(key)
                            Return DirectCast(DirectCast(result, list).getByName(key), Dictionary(Of String, Double))
                        End Function) _
                .ToArray
        Else
#Disable Warning
            matrix = REnv.asVector(Of Dictionary(Of String, Double))(result)
#Enable Warning
        End If

        For Each key As String In sgt.feature_names
            v = matrix _
                .Select(Function(r) r(key)) _
                .ToArray
            df.add(key, v)
        Next

        Return df
    End Function

    ''' <summary>
    ''' cast the clr dataframe object to R# dataframe runtime object
    ''' </summary>
    ''' <param name="features"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <RGenericOverloads("as.data.frame")>
    Private Function toDataframe(features As FeatureFrame, args As list, env As Environment) As Rdataframe
        Return MathDataSet.toDataframe(features, args, env)
    End Function

    ''' <summary>
    ''' Sequence Graph Transform (SGT) — Sequence Embedding for Clustering, Classification, and Search
    ''' 
    ''' Sequence Graph Transform (SGT) is a sequence embedding function. SGT extracts 
    ''' the short- and long-term sequence features and embeds them in a finite-dimensional 
    ''' feature space. The long and short term patterns embedded in SGT can be tuned 
    ''' without any increase in the computation.
    ''' 
    ''' > https://github.com/cran2367/sgt/blob/25bf28097788fbbf9727abad91ec6e59873947cc/python/sgt-package/sgt/sgt.py
    ''' </summary>
    ''' <remarks>
    ''' Compute embedding of a single or a collection of discrete item
    ''' sequences. A discrete item sequence is a sequence made from a set
    ''' discrete elements, also known as alphabet set. For example,
    ''' suppose the alphabet set is the set of roman letters,
    ''' {A, B, ..., Z}. This set is made of discrete elements. Examples of
    ''' sequences from such a set are AABADDSA, UADSFJPFFFOIHOUGD, etc.
    ''' Such sequence datasets are commonly found in online industry,
    ''' for example, item purchase history, where the alphabet set is
    ''' the set of all product items. Sequence datasets are abundant in
    ''' bioinformatics as protein sequences.
    ''' Using the embeddings created here, classification and clustering
    ''' models can be built for sequence datasets.
    ''' Read more in https://arxiv.org/pdf/1608.03533.pdf
    ''' </remarks>
    ''' <param name="alphabets">Optional, except if mode is Spark.
    ''' The set of alphabets that make up all
    ''' the sequences in the dataset. If not passed, the
    ''' alphabet set is automatically computed as the
    ''' unique set of elements that make all the sequences.
    ''' A list or 1d-array of the set of elements that make up the
    ''' sequences. For example, np.array(["A", "B", "C"].
    ''' If mode is 'spark', the alphabets are necessary.
    ''' </param>
    ''' <param name="kappa">
    ''' Tuning parameter, kappa > 0, to change the extraction of
    ''' long-term dependency. Higher the value the lesser
    ''' the long-term dependency captured in the embedding.
    ''' Typical values for kappa are 1, 5, 10.</param>
    ''' <param name="length_sensitive">Default False. This is set to true if the embedding of
    ''' should have the information of the length of the sequence.
    ''' If set to false then the embedding of two sequences with
    ''' similar pattern but different lengths will be the same.
    ''' lengthsensitive = false is similar to length-normalization.</param>
    ''' <returns></returns>
    <ExportAPI("SGT")>
    Public Function SGT(Optional alphabets As Char() = Nothing,
                        Optional kappa As Double = 1,
                        Optional length_sensitive As Boolean = False,
                        Optional mode As SequenceGraphTransform.Modes = SequenceGraphTransform.Modes.Full,
                        Optional env As Environment = Nothing) As SequenceGraphTransform

        Dim println = env.WriteLineHandler

        Call println($"SGT algorithm running in mode: {mode.Description}")
        Call println(New Dictionary(Of String, String) From {
            {"alphabets", alphabets.JoinBy("")},
            {"kappa", kappa},
            {"length.sensitive", length_sensitive}
        }.GetJson)

        Return New SequenceGraphTransform(
            alphabets:=alphabets,
            kappa:=kappa,
            lengthsensitive:=length_sensitive,
            mode:=mode
        )
    End Function

    <ExportAPI("split_training_test")>
    <RApiReturn("training", "test")>
    Public Function split_training_test(ds As SampleData(), Optional ratio As Double = 0.7) As Object
        Dim len As Integer = ds.Length * ratio
        ds = ds.Shuffles
        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"training", ds.Take(len).ToArray},
                {"test", ds.Skip(len).ToArray}
            }
        }
    End Function

    <ExportAPI("estimate_alphabets")>
    Public Function estimate_alphabets(<RRawVectorArgument> seqs As Object) As Char()
        Return SequenceGraphTransform.estimate_alphabets(CLRVector.asCharacter(seqs))
    End Function

    <ExportAPI("sample_id")>
    <RApiReturn(GetType(String))>
    Public Function sample_id(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim data As pipeline = pipeline.TryCreatePipeline(Of SampleData)(x, env)

        If data.isError Then
            Return data.getError
        End If

        Return data.populates(Of SampleData)(env) _
            .Select(Function(si) si.id) _
            .ToArray
    End Function

    ''' <summary>
    ''' get feature vector by a given feature column index
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="feature">the feature column index in the sample dataset, start based 1.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("get_feature")>
    Public Function get_feature(<RRawVectorArgument> x As Object, feature As Integer, Optional env As Environment = Nothing) As Object
        Dim data As pipeline = pipeline.TryCreatePipeline(Of SampleData)(x, env)

        If data.isError Then
            Return data.getError
        Else
            feature -= 1
        End If

        Return data.populates(Of SampleData)(env) _
            .Select(Function(si) si.features(feature)) _
            .ToArray
    End Function

    ''' <summary>
    ''' Makes the feature projection
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="features">the feature column index in the sample dataset, start based 1.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("project_features")>
    Public Function project_features(<RRawVectorArgument> x As Object, features As Integer(), Optional env As Environment = Nothing) As Object
        Dim data As pipeline = pipeline.TryCreatePipeline(Of SampleData)(x, env)

        If data.isError Then
            Return data.getError
        Else
            features = features.Select(Function(offset) offset - 1).ToArray
        End If

        Return data.populates(Of SampleData)(env) _
            .Select(Function(si)
                        Return New SampleData(
                            features:=features.Select(Function(i) si.features(i)).ToArray,
                            labels:=si.labels
                        ) With {
                            .id = si.id
                        }
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' sort the sample dataset
    ''' </summary>
    ''' <param name="x">should be a collection of the sample dataset</param>
    ''' <param name="order_id">
    ''' this parameter value could be a:
    ''' 
    ''' 1. the sample id collection
    ''' 2. a feature index, start from base 1
    ''' </param>
    ''' <param name="desc">
    ''' sort the feature data in desc order, this parameter only works when the
    ''' order id parameter is a single feature index value.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' the input sample data will be sorted based on the given <paramref name="order_id"/> vector.
    ''' </returns>
    <ExportAPI("sort_samples")>
    Public Function sort_samples(<RRawVectorArgument> x As Object,
                                 <RRawVectorArgument> order_id As Object,
                                 Optional desc As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

        Dim data As pipeline = pipeline.TryCreatePipeline(Of SampleData)(x, env)

        If data.isError Then
            Return data.getError
        End If

        Dim sort As String() = CLRVector.asCharacter(order_id)

        If sort.TryCount = 1 AndAlso sort(0).IsPattern("\d+") Then
            ' is a feature index
            Dim i As Integer = Integer.Parse(sort(0)) - 1

            Return data.populates(Of SampleData)(env) _
                .Sort(
                    by:=Function(a) a.features(i),
                    desc:=desc
                ) _
                .ToArray
        Else
            Return data.sortById(sort, env)
        End If
    End Function

    <Extension>
    Private Function sortById(data As pipeline, sort As String(), env As Environment) As SampleData()
        ' sort by sample id matches
        Dim hash As Dictionary(Of String, SampleData()) = data _
            .populates(Of SampleData)(env) _
            .GroupBy(Function(si) si.id) _
            .ToDictionary(Function(si) si.Key,
                          Function(si)
                              Return si.ToArray
                          End Function)
        Return sort _
            .Select(Function(id)
                        If hash.ContainsKey(id) Then
                            Return hash(id)
                        Else
                            Return {}
                        End If
                    End Function) _
            .IteratesALL _
            .ToArray
    End Function

    ''' <summary>
    ''' Add a data sample into the target sparse sample matrix object
    ''' </summary>
    ''' <param name="matrix">the sparse matrix object</param>
    ''' <param name="sample_id">the row name, unique sample id</param>
    ''' <param name="x">the sample data, should be in format of [feature_name=>value] key-value tuple list.</param>
    ''' <returns></returns>
    <ExportAPI("add_sample")>
    Public Function addRow(matrix As UnionMatrix, sample_id As String, x As list) As UnionMatrix
        Call matrix.Add(sample_id, x)
        Return matrix
    End Function

    ''' <summary>
    ''' helper function for cast the R# dataframe runtime object as the clr dataframe object
    ''' </summary>
    ''' <param name="x">should be a R# dataframe runtime object</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("toFeatureSet")>
    <RApiReturn(GetType(FeatureFrame))>
    Public Function toFeatureSet(x As Rdataframe, Optional env As Environment = Nothing) As Object
        Return MathDataSet.toFeatureSet(x, env)
    End Function

    ''' <summary>
    ''' Convert the sciBASIC general dataframe as the Machine learning general dataset
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="labels"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.MLdataset")>
    Public Function CreateMLdataset(<RRawVectorArgument> x As Object,
                                    <RRawVectorArgument>
                                    Optional labels As Object = Nothing,
                                    Optional env As Environment = Nothing) As Object
        Dim ds As DataSet

        If TypeOf x Is FeatureFrame Then
            ds = DirectCast(x, FeatureFrame).Imports(labels:=CLRVector.asCharacter(labels))
        ElseIf DataFramework.IsCollection(Of Sample)(x.GetType) Then
            Dim sampleList As New SampleList(DirectCast(x, IEnumerable(Of Sample)))
            Dim names As String() = sampleList(0).vector _
                .Select(Function(nil, i) $"x{i + 1}") _
                .ToArray
            Dim norm As NormalizeMatrix = NormalizeMatrix.CreateFromSamples(sampleList, names, estimateQuantile:=False)

            ds = New DataSet With {
                .DataSamples = sampleList,
                .NormalizeMatrix = norm,
                .output = sampleList(0).target _
                    .Select(Function(nil, i) $"y{i + 1}") _
                    .ToArray
            }
        ElseIf DataFramework.IsCollection(Of SampleData)(x.GetType) Then
            Return SampleData.CreateDataSet(DirectCast(x, IEnumerable(Of SampleData)))
        Else
            Return Message.InCompatibleType(GetType(FeatureFrame), x.GetType, env)
        End If

        Return ds
    End Function

    ''' <summary>
    ''' get summary and descriptions about the given dataset
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("description")>
    Public Function dataDescription(x As Object, Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is Rdataframe Then
            x = datasetKit.toFeatureSet(x, env)
        End If

        If TypeOf x Is Message Then
            Return x
        ElseIf Not TypeOf x Is FeatureFrame Then
            Return Message.InCompatibleType(GetType(FeatureFrame), x.GetType, env)
        End If

        Return New Rdataframe With {
            .rownames = FeatureDescription _
                .GetDescriptions _
                .ToArray,
            .columns = DirectCast(x, FeatureFrame).features _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return FeatureDescription.DescribFeature(a.Value)
                              End Function)
        }
    End Function

    <RGenericOverloads("plot")>
    Friend Function EmbeddingRender(input As IDataEmbedding, args As list, env As Environment) As GraphicsData
        Dim size$ = InteropArgumentHelper.getSize(args!size, env)
        Dim pointSize# = args.getValue("point_size", env, 15.0)
        Dim showLabels As Boolean = args.getValue("show_labels", env, False)
        Dim showBubble As Boolean = args.getValue("show_bubble", env, False)
        Dim labels As String() = args.getValue(Of String())("labels", env)
        Dim labelStyle$ = args.getValue("label_style", env, CSSFont.Win10Normal)
        Dim labelColor$ = args.getValue("label_color", env, "black")
        ' [label => clusterid]
        Dim clusters As list = args.getValue(Of list)("clusters", env)
        Dim bubbleAlpha As Integer = args.getValue("bubble_alpha", env, 0.0) * 255
        Dim legendLabelCSS$ = args.getValue("legendlabel", env, CSSFont.PlotLabelNormal)
        Dim colors As String = args.getValue("colorSet", env, "Clusters")
        Dim padding As String = args.getValue("padding", env, "padding:150px 150px 300px 300px;")
        Dim clusterData As Dictionary(Of String, String) = Nothing

        If Not clusters Is Nothing Then
            clusterData = clusters.slots _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return any.ToString([single](a.Value))
                              End Function)
        End If

        If input.dimension = 2 Then
            Return input.DrawEmbedding2D(
                size:=size,
                labels:=labels,
                clusters:=clusterData,
                pointSize:=pointSize,
                showConvexHull:=showBubble,
                legendLabelCSS:=legendLabelCSS,
                colorSet:=colors,
                padding:=padding
            )
        Else
            Dim camera As Camera = args.getValue(Of Camera)("camera", env)

            If camera Is Nothing Then
                env.AddMessage("the 3D camera Is nothing, default camera value will be apply!", MSG_TYPES.WRN)
                camera = New Camera With {
                    .screen = size.SizeParser,
                    .angleX = 120,
                    .angleY = 120,
                    .angleZ = 30,
                    .fov = 1500,
                    .viewDistance = 500
                }
            Else
                size = $"{camera.screen.Width},{camera.screen.Height}"
            End If

            Return input.DrawEmbedding3D(
                camera:=camera,
                size:=size,
                showLabels:=showLabels,
                pointSize:=pointSize,
                labels:=labels,
                labelCSS:=labelStyle,
                clusters:=clusterData,
                labelColor:=labelColor,
                bubbleAlpha:=bubbleAlpha,
                colorSet:=colors,
                padding:=padding
            )
        End If
    End Function

    ''' <summary>
    ''' get the normalization matrix from a given machine learning training dataset.
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <returns></returns>
    <ExportAPI("normalize_matrix")>
    Public Function getNormalizeMatrix(dataset As DataSet) As NormalizeMatrix
        Return dataset.NormalizeMatrix
    End Function

    ''' <summary>
    ''' convert machine learning dataset to dataframe table.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="markOuput"></param>
    ''' <returns></returns>
    <ExportAPI("as.tabular")>
    Public Function Tabular(x As DataSet, Optional markOuput As Boolean = True) As DataTable()
        Return x.ToTable(markOuput).ToArray
    End Function

    <ExportAPI("as.sampleSet")>
    Public Function SampleList(x As DataSet) As SampleData()
        Return x.DataSamples _
            .AsEnumerable _
            .Select(Function(si) New SampleData(si)) _
            .ToArray
    End Function

    ''' <summary>
    ''' read the dataset for training the machine learning model
    ''' </summary>
    ''' <param name="file">a xml data file or a binary stream pack data file</param>
    ''' <returns></returns>
    <ExportAPI("read.ML_model")>
    Public Function readModelDataset(file As String) As DataSet
        If file.ExtensionSuffix("xml") Then
            Return file.LoadXml(Of DataSet)
        Else
            Using buf As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Dim reader As New PackReader(buf)
                Dim ds As New DataSet With {
                    .DataSamples = New SampleList With {
                        .items = reader.GetAllSamples.ToArray
                    },
                    .NormalizeMatrix = reader.GetMatrix,
                    .output = reader.output_labels
                }

                Return ds
            End Using
        End If
    End Function

    ''' <summary>
    ''' write the data model to file
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("write.ML_model")>
    Public Function writeMLDataset(x As DataSet, file As String) As Object
        If file.ExtensionSuffix("xml") Then
            Return x.GetXml.SaveTo(file)
        Else
            Dim buf As Stream = file.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)

            Using writer As New PackWriter(buf)
                Call writer.WriteDataSet(x)
            End Using

            Return True
        End If
    End Function

    <ExportAPI("write.sample_set")>
    Public Function writeSampleSet(<RRawVectorArgument> x As Object, file As Object, Optional env As Environment = Nothing) As Object
        Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)
        Dim sampleSet As pipeline = pipeline.TryCreatePipeline(Of SampleData)(x, env)

        If buf Like GetType(Message) Then
            Return buf.TryCast(Of Message)
        ElseIf sampleSet.isError Then
            Return sampleSet.getError
        End If

        Call SampleData.Save(sampleSet.populates(Of SampleData)(env), buf.TryCast(Of Stream))
        Call buf.TryCast(Of Stream).Flush()

        Return True
    End Function

    <ExportAPI("read.sample_set")>
    <RApiReturn(GetType(SampleData))>
    Public Function readSampleSet(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If buf Like GetType(Message) Then
            Return buf.TryCast(Of Message)
        Else
            Return SampleData.Load(buf.TryCast(Of Stream)).ToArray
        End If
    End Function

    <Extension>
    Friend Function TakeSubset(Of T)(x As IEnumerable(Of T), takes As Integer) As IEnumerable(Of T)
        If takes > 0 Then
            Return x.Take(takes)
        Else
            Return x
        End If
    End Function

    <ExportAPI("MNIST.dims")>
    Public Function getMNISTImageSize(file As String) As Object
        Dim size As Size = MNIST.GetImageSize(file)
        Dim dims As New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"width", size.Width},
                {"height", size.Height}
            }
        }

        Return dims
    End Function

    ''' <summary>
    ''' read mnist dataset file as R# dataframe object
    ''' </summary>
    ''' <param name="path">The MNIST image data file path</param>
    ''' <param name="subset">Just take a subset of the target dataset, this parameter is the sample size of the sub dataset</param>
    ''' <param name="args">
    ''' + format, format = labelledvector, labelled data vector in message pack format
    ''' + labelfile, labelfile = /path/to/mnist.labels
    ''' + dataset, dataset = vector/dataframe/image
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("read.MNIST")>
    Public Function readMNISTLabelledVector(path As String,
                                            Optional subset As Integer = -1,
                                            <RListObjectArgument>
                                            Optional args As list = Nothing,
                                            Optional env As Environment = Nothing) As Object

        Dim format As String = args.getValue("format", env, [default]:="labelledvector")
        Dim dataset As String = args.getValue("dataset", env, [default]:="vector")

        Select Case format
            Case "labelledvector"
                Using file As Stream = path.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                    Dim rawdata = MsgPackSerializer.Deserialize(Of LabelledVector())(file) _
                        .TakeSubset(subset) _
                        .ToArray

                    Select Case dataset
                        Case "vector"
                            Return New list With {
                                .slots = rawdata _
                                    .Select(Function(r, i) (r, i)) _
                                    .ToDictionary(Function(v) $"{v.r.UID}-{v.i + 1}",
                                                  Function(v)
                                                      Return CObj(CLRVector.asNumeric(v.r.vector))
                                                  End Function)
                            }
                        Case "dataframe"
                            Return LabelledVector.CreateDataFrame(vector:=rawdata)
                        Case Else
                            Return Internal.debug.stop(New NotImplementedException($"the data set format '{dataset}' is not yet implemented!"), env)
                    End Select
                End Using
            Case Else
                Dim labelfile As String = args.getValue("labelfile", env, [default]:="")

                If labelfile.StringEmpty Then
                    Return Internal.debug.stop("No 'labelfile' was provided for the MNIST image dataset!", env)
                End If

                Using MNIST As New MNIST(imagesFile:=path, labelfile)
                    Return MNIST.getMNISTRawDataset(dataset, subset, env)
                End Using
        End Select
    End Function

    <Extension>
    Private Function getMNISTRawDataset(MNIST As MNIST, dataset As String, subset As Integer, env As Environment) As Object
        Select Case dataset
            Case "vector"
                Return New list With {
                    .slots = MNIST _
                        .ExtractVectors _
                        .TakeSubset(subset) _
                        .ToDictionary(Function(v) v.name,
                                      Function(v)
                                          Return CObj(CLRVector.asNumeric(v.value))
                                      End Function)
                }
            Case "dataframe"
                Dim all = MNIST.ExtractVectors.TakeSubset(subset).ToArray
                Dim labels As String() = all.Select(Function(v) v.description).ToArray
                Dim df As New Rdataframe With {
                    .rownames = all.Select(Function(v) v.name).ToArray,
                    .columns = New Dictionary(Of String, Array)
                }

                Call df.add("label", labels)

                Dim offset As Integer

                For i As Integer = 0 To all(0).Length - 1
                    offset = i
                    df.add($"X{i + 1}", CLRVector.asNumeric(all.Select(Function(v) v(offset)).ToArray))
                Next

                Return df
            Case "image"
                Return New list With {
                    .slots = MNIST _
                        .ExtractImages _
                        .TakeSubset(subset) _
                        .ToDictionary(Function(v) v.Name,
                                      Function(v)
                                          Return CObj(v.Value)
                                      End Function)
                }
            Case Else
                Return Internal.debug.stop(New NotImplementedException($"the data set format '{dataset}' is not yet implemented!"), env)
        End Select
    End Function

    ''' <summary>
    ''' create demo matrix for run test
    ''' </summary>
    ''' <param name="size">number of rows</param>
    ''' <param name="dimensions">number of columns</param>
    ''' <param name="pzero">percentage of zero in an entity vector</param>
    ''' <param name="nclass">number of class tags</param>
    ''' <returns></returns>
    <ExportAPI("gaussian")>
    Public Function demoMatrix(size As Integer, dimensions As Integer,
                               Optional pzero As Double = 0.8,
                               Optional nclass% = 5) As Rdataframe

        Dim tagRanges = nclass _
            .Sequence _
            .Select(Function(tag)
                        Return New NamedCollection(Of Func(Of Double))($"class_{tag + 1}", dimensionRange(dimensions, pzero))
                    End Function) _
            .ToArray
        Dim dataset As New List(Of NamedValue(Of Double()))

        For i As Integer = 1 To size
            Dim tag = tagRanges(randf.NextInteger(nclass))
            Dim vec = tag.Select(Function(p) p()).ToArray

            dataset.Add(New NamedValue(Of Double()) With {.Name = tag.name, .Value = vec, .Description = i})
        Next

        Dim matrix As New Rdataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        For i As Integer = 0 To dimensions - 1
#Disable Warning
            matrix.columns($"X{i + 1}") = dataset _
                .Select(Function(d) d.Value(i)) _
                .ToArray
#Enable Warning
        Next
        Return matrix
    End Function

    Private Iterator Function dimensionRange(dimensions As Integer, pzero As Double) As IEnumerable(Of Func(Of Double))
        For i As Integer = 0 To dimensions - 1
            Dim min As Double = randf.NextInteger(10000000)
            Dim max As Double = min * 100

            Yield Function()
                      If randf.NextDouble <= pzero Then
                          Return 0
                      Else
                          Return randf.NextDouble(min, max)
                      End If
                  End Function
        Next
    End Function

    ''' <summary>
    ''' encode a given numeric sequence as factors by quantile levels
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="levels">The number of quantile levels to encode the target numeric sequence</param>
    ''' <returns></returns>
    <ExportAPI("q_factors")>
    <RApiReturn(GetType(String))>
    Public Function q_factors(<RRawVectorArgument> x As Object, levels As Integer, Optional fast As Boolean = True) As Object
        Dim data As Double() = CLRVector.asNumeric(x)
        Dim qlevels As String() = data _
            .QuantileLevels(steps:=1 / levels, fast:=fast) _
            .Select(Function(li) CStr(li)) _
            .ToArray
        Dim factors = factor.CreateFactor(qlevels)
        Dim str = qlevels _
            .Select(Function(li)
                        Return factors.GetFactor(li).ToString
                    End Function) _
            .ToArray

        Return str
    End Function

    ''' <summary>
    ''' do feature encoding
    ''' </summary>
    ''' <param name="features"></param>
    ''' <param name="encoder">
    ''' a set of the encoder function that apply to the 
    ''' corresponding feature data.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("encoding")>
    <RApiReturn(GetType(FeatureFrame))>
    Public Function Encoding(features As FeatureFrame,
                             <RListObjectArgument>
                             encoder As list,
                             Optional env As Environment = Nothing) As Object

        Dim encoderMaps As New Encoder

        For Each fieldName As String In encoder.getNames
            Dim code As Object = encoder.getByName(fieldName)
            Dim err As Message = Nothing

            If TypeOf code Is RMethodInfo Then
                err = mapEncoder(DirectCast(code, RMethodInfo).GetNetCoreCLRDeclaration.Name, fieldName, encoderMaps, env)
            ElseIf TypeOf code Is DeclareLambdaFunction Then
                err = encoderMaps.mapLambda(DirectCast(code, DeclareLambdaFunction), env)
            Else
                Return Internal.debug.stop(New NotImplementedException($"{fieldName} -> {code.GetType.FullName}"), env)
            End If

            If Not err Is Nothing Then
                Return err
            End If
        Next

        Return encoderMaps.Encoding(features)
    End Function

    <Extension>
    Private Function mapLambda(encoderMaps As Encoder, lambda As DeclareLambdaFunction, env As Environment) As Object
        Dim fieldName = lambda.parameterNames.First
        Dim code = lambda.closure.Evaluate(env)

        If Program.isException(code) Then
            Return code
        End If

        If TypeOf code Is SymbolReference Then
            Return mapEncoder(DirectCast(code, SymbolReference).symbol, fieldName, encoderMaps, env)
        ElseIf TypeOf code Is NamespaceFunctionSymbolReference Then
            code = DirectCast(code, NamespaceFunctionSymbolReference).symbol
            Return mapEncoder(DirectCast(code, SymbolReference).symbol, fieldName, encoderMaps, env)
        ElseIf TypeOf code Is RMethodInfo Then
            Return mapEncoder(DirectCast(code, RMethodInfo).GetNetCoreCLRDeclaration.Name, fieldName, encoderMaps, env)
        ElseIf TypeOf code Is FeatureEncoder Then
            encoderMaps.AddEncodingRule(fieldName, DirectCast(code, FeatureEncoder))
        Else
            Return Internal.debug.stop(New NotImplementedException($"{fieldName} -> {code.GetType.FullName}"), env)
        End If

        Return Nothing
    End Function

    Private Function mapEncoder(code As String, fieldName As String, encoderMaps As Encoder, env As Environment) As Message
        Select Case code
            Case NameOf(binEncoder), "to_bins"
                encoderMaps.AddEncodingRule(fieldName, binEncoder)
            Case NameOf(factorEncoder), "to_factors"
                encoderMaps.AddEncodingRule(fieldName, factorEncoder)
            Case NameOf(boolEncoder), "to_ints"
                encoderMaps.AddEncodingRule(fieldName, boolEncoder)
            Case Else
                Return Internal.debug.stop(New NotImplementedException($"{fieldName} -> {code}"), env)
        End Select

        Return Nothing
    End Function

    <ExportAPI("to_bins")>
    Public Function binEncoder(Optional nbins As Integer = 3, Optional format As String = "G4") As FeatureEncoder
        Return New NumericBinsEncoder(nbins, format)
    End Function

    <ExportAPI("to_factors")>
    Public Function factorEncoder() As EnumEncoder
        Return New EnumEncoder
    End Function

    <ExportAPI("to_ints")>
    Public Function boolEncoder() As FlagEncoder
        Return New FlagEncoder
    End Function
End Module
