#Region "Microsoft.VisualBasic::a9bff2c6c1fbb09fb7035b756237c991, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//MachineLearning/CNN.vb"

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

'   Total Lines: 71
'    Code Lines: 40
' Comment Lines: 22
'   Blank Lines: 9
'     File Size: 2.44 KB


' Module CNN
' 
'     Function: detectObject, loadModel, saveModel
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' feed-forward phase of deep Convolutional Neural Networks
''' </summary>
<Package("CNN")>
<RTypeExport("cnn", GetType(LayerBuilder))>
Module CNNTools

    Sub Main()
        Internal.ConsolePrinter.AttachConsoleFormatter(Of CNNFunction)(Function(cnn) cnn.ToString)
        Internal.ConsolePrinter.AttachConsoleFormatter(Of ConvolutionalNN)(Function(cnn) cnn.ToString)
    End Sub

    ''' <summary>
    ''' get/set of the CNN parallel thread number
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    <ExportAPI("n_threads")>
    Public Function n_threads(Optional n As Integer? = Nothing) As Integer
        If Not n Is Nothing Then
            ConvolutionalNN.SetThreads(CInt(n))
        End If

        Return ConvolutionalNN.GetThreads
    End Function

    ''' <summary>
    ''' Create a new CNN model
    ''' 
    ''' Convolutional neural network (CNN) is a regularized type of feed-forward
    ''' neural network that learns feature engineering by itself via filters 
    ''' (or kernel) optimization. Vanishing gradients and exploding gradients, 
    ''' seen during backpropagation in earlier neural networks, are prevented by 
    ''' using regularized weights over fewer connections.
    ''' </summary>
    ''' <param name="file">
    ''' if the given model file parameter is not default nothing, then the new 
    ''' CNN model object will be loaded from the specific given file, otherwise 
    ''' a blank new model object will be created from the CNN layer model 
    ''' builder.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("cnn")>
    <RApiReturn(GetType(ConvolutionalNN), GetType(LayerBuilder))>
    Public Function cnn_new(<RRawVectorArgument>
                            Optional file As Object = Nothing,
                            Optional env As Environment = Nothing) As Object

        If file Is Nothing Then
            Return New LayerBuilder
        Else
            Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

            If buf Like GetType(Message) Then
                Return buf.TryCast(Of Message)
            End If

            Return New CNNFunction With {.cnn = ReadModelCNN.Read(buf.TryCast(Of Stream))}
        End If
    End Function

    <ROperator("+")>
    Public Function addLayer(cnn As LayerBuilder, args As CNNLayerArguments) As LayerBuilder
        Return args.CreateLayer(cnn)
    End Function

    ''' <summary>
    ''' The input layer is a simple layer that will pass the data though and
    ''' create a window into the full training data set. So for instance if
    ''' we have an image of size 28x28x1 which means that we have 28 pixels
    ''' in the x axle and 28 pixels in the y axle and one color (gray scale),
    ''' then this layer might give you a window of another size example 24x24x1
    ''' that is randomly chosen in order to create some distortion into the
    ''' dataset so the algorithm don't over-fit the training.
    ''' </summary>
    ''' <param name="size"></param>
    ''' <param name="depth"></param>
    ''' <param name="c"></param>
    ''' <returns></returns>
    <ExportAPI("input_layer")>
    Public Function input_layer(<RRawVectorArgument>
                                size As Object,
                                Optional depth As Integer = 1,
                                Optional c As Double = 0) As CNNLayerArguments

        Dim sz As Integer() = CLRVector.asInteger(size)
        Dim sz_val As New Dimension(sz(0), sz(1))
        Dim layer As New CNNLayerArguments With {
            .type = NameOf(input_layer),
            .args = New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"depth", depth},
                    {"c", c},
                    {"dims", sz_val}
                }
            }
        }

        Return layer
    End Function

    <ExportAPI("regression_layer")>
    Public Function regression_layer() As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(regression_layer),
            .args = New list With {
                .slots = New Dictionary(Of String, Object)
            }
        }
    End Function

    ''' <summary>
    ''' This layer uses different filters to find attributes of the data that
    ''' affects the result. As an example there could be a filter to find
    ''' horizontal edges in an image.
    ''' </summary>
    ''' <param name="sx"></param>
    ''' <param name="filters"></param>
    ''' <param name="stride"></param>
    ''' <param name="padding"></param>
    ''' <returns></returns>
    <ExportAPI("conv_layer")>
    Public Function conv_layer(sx As Integer,
                               filters As Integer,
                               Optional stride As Integer = 1,
                               Optional padding As Integer = 0) As CNNLayerArguments

        Return New CNNLayerArguments With {
            .type = NameOf(conv_layer),
            .args = New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"sx", sx},
                    {"filters", filters},
                    {"stride", stride},
                    {"padding", padding}
                }
            }
        }
    End Function

    <ExportAPI("conv_transpose_layer")>
    Public Function conv_transpose_layer(<RRawVectorArgument> dims As Object,
                                         <RRawVectorArgument> filter As Object,
                                         Optional filters As Integer = 3,
                                         Optional stride As Integer = 1) As CNNLayerArguments

        Dim sz As Integer() = CLRVector.asInteger(filter)
        Dim sz_val As New Dimension(sz(0), sz(1))
        Dim dims_ints As Integer() = CLRVector.asInteger(dims)
        Dim dims_val As New OutputDefinition(dims_ints(0), dims_ints(1), dims_ints(2))
        Dim layer As New CNNLayerArguments With {
            .type = NameOf(conv_transpose_layer),
            .args = New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"filter", sz_val},
                    {"filters", filters},
                    {"stride", stride},
                    {"dims", dims_val}
                }
            }
        }

        Return layer
    End Function

    ''' <summary>
    ''' This layer is useful when we are dealing with ReLU neurons. Why is that?
    ''' Because ReLU neurons have unbounded activations and we need LRN to normalize
    ''' that. We want to detect high frequency features with a large response. If we
    ''' normalize around the local neighborhood of the excited neuron, it becomes even
    ''' more sensitive as compared to its neighbors.
    ''' 
    ''' At the same time, it will dampen the responses that are uniformly large in any
    ''' given local neighborhood. If all the values are large, then normalizing those
    ''' values will diminish all of them. So basically we want to encourage some kind
    ''' of inhibition and boost the neurons with relatively larger activations. This
    ''' has been discussed nicely in Section 3.3 of the original paper by Krizhevsky et al.
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    <ExportAPI("lrn_layer")>
    Public Function lrn_layer(Optional n As Integer = 5) As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(lrn_layer),
            .args = New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"n", n}
                }
            }
        }
    End Function

    ''' <summary>
    ''' Implements Tanh nonlinearity elementwise x to tanh(x)
    ''' so the output is between -1 and 1.
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("tanh_layer")>
    Public Function tanh_layer() As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(tanh_layer)
        }
    End Function

    ''' <summary>
    ''' [*loss_layers] This layer will squash the result of the activations in the fully
    ''' connected layer and give you a value of 0 to 1 for all output activations.
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("softmax_layer")>
    Public Function softmax_layer() As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(softmax_layer)
        }
    End Function

    ''' <summary>
    ''' This is a layer of neurons that applies the non-saturating activation
    ''' function f(x)=max(0,x). It increases the nonlinear properties of the
    ''' decision function and of the overall network without affecting the
    ''' receptive fields of the convolution layer.
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("relu_layer")>
    Public Function relu_layer() As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(relu_layer)
        }
    End Function

    <ExportAPI("leaky_relu_layer")>
    Public Function leaky_relu_layer() As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(leaky_relu_layer)
        }
    End Function

    ''' <summary>
    ''' Implements Maxout nonlinearity that computes x to max(x)
    ''' where x is a vector of size group_size. Ideally of course,
    ''' the input size should be exactly divisible by group_size
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("maxout_layer")>
    Public Function maxout_layer() As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(maxout_layer)
        }
    End Function

    ''' <summary>
    ''' Implements Sigmoid nonlinearity elementwise x to 1/(1+e^(-x))
    ''' so the output is between 0 and 1.
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("sigmoid_layer")>
    Public Function sigmoid_layer() As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(sigmoid_layer)
        }
    End Function

    ''' <summary>
    ''' This layer will reduce the dataset by creating a smaller zoomed out
    ''' version. In essence you take a cluster of pixels take the sum of them
    ''' and put the result in the reduced position of the new image.
    ''' </summary>
    ''' <param name="sx"></param>
    ''' <param name="stride"></param>
    ''' <param name="padding"></param>
    ''' <returns></returns>
    <ExportAPI("pool_layer")>
    Public Function pool_layer(sx As Integer, stride As Integer, padding As Integer) As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(pool_layer),
            .args = New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"sx", sx},
                    {"stride", stride},
                    {"padding", padding}
                }
            }
        }
    End Function

    ''' <summary>
    ''' This layer will remove some random activations in order to
    ''' defeat over-fitting.
    ''' </summary>
    ''' <param name="drop_prob"></param>
    ''' <returns></returns>
    <ExportAPI("dropout_layer")>
    Public Function dropout_layer(Optional drop_prob As Double = 0.5) As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(dropout_layer),
            .args = New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"drop_prob", drop_prob}
                }
            }
        }
    End Function

    ''' <summary>
    ''' Neurons in a fully connected layer have full connections to all
    ''' activations in the previous layer, as seen in regular Neural Networks.
    ''' Their activations can hence be computed with a matrix multiplication
    ''' followed by a bias offset.
    ''' </summary>
    ''' <param name="size"></param>
    ''' <returns></returns>
    <ExportAPI("full_connected_layer")>
    Public Function full_connected_layer(size As Integer) As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(full_connected_layer),
            .args = New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"size", size}
                }
            }
        }
    End Function

    <ExportAPI("gaussian_layer")>
    Public Function gaussian_layer() As CNNLayerArguments
        Return New CNNLayerArguments With {
            .type = NameOf(gaussian_layer)
        }
    End Function

    <ExportAPI("sample_dataset")>
    <RApiReturn(GetType(SampleData))>
    Public Function sample_dataset(<RRawVectorArgument> dataset As Object,
                                   <RRawVectorArgument>
                                   Optional labels As Object = Nothing,
                                   Optional env As Environment = Nothing) As Object

        If TypeOf dataset Is dataframe Then
            Return DirectCast(dataset, dataframe).sample_dataset_from_df(labels, env)
        ElseIf TypeOf dataset Is list Then
            Dim li As list = DirectCast(dataset, list)
            Dim ds As New List(Of SampleData)

            If TypeOf labels Is list Then
                Dim labelList As list = DirectCast(labels, list)

                For Each name As String In li.getNames
                    Call ds.Add(New SampleData(
                        features:=CLRVector.asNumeric(li.getByName(name)),
                        labels:=CLRVector.asNumeric(labelList.getByName(name)))
                    )
                Next
            Else
                Dim v As Double() = CLRVector.asNumeric(labels)

                For Each obj As SeqValue(Of Object) In li.data.SeqIterator
                    ds.Add(New SampleData(CLRVector.asNumeric(obj.value), v(obj)))
                Next
            End If

            Return ds
        Else
            Return Message.InCompatibleType(GetType(dataframe), dataset.GetType, env)
        End If
    End Function

    <ExportAPI("sample_dataset.image")>
    <RApiReturn(GetType(SampleData))>
    Public Function sample_image_dataset(<RRawVectorArgument> images As Object,
                                         <RRawVectorArgument>
                                         Optional labels As Object = Nothing,
                                         <RRawVectorArgument>
                                         Optional resize As Object = Nothing,
                                         Optional env As Environment = Nothing) As Object

        Dim resize_int As Integer() = CLRVector.asInteger(resize)

        If labels Is Nothing Then
            ' create sample data for self encoder
            labels = New list With {.slots = New Dictionary(Of String, Object)}
        End If

        If TypeOf images Is list Then
            Dim imageList = DirectCast(images, list)
            Dim ds As New List(Of SampleData)

            If TypeOf labels Is list Then
                Dim labelSet As list = DirectCast(labels, list)
                Dim keys = imageList.getNames

                For Each key As String In keys
                    Dim img As Image = imageList.getByName(key)
                    Dim v As Double() = RasterScaler.ToRasterVector(img, resize_int)
                    Dim lable As Double() = CLRVector.asNumeric(labelSet.getByName(key))

                    ds.Add(New SampleData(v, lable) With {.id = key})
                Next
            Else
                Dim labelSet As Double() = CLRVector.asNumeric(labels)
                Dim keys = imageList.getNames

                For i As Integer = 0 To keys.Length - 1
                    Dim img As Image = imageList.getByName(keys(i))
                    Dim v As Double() = RasterScaler.ToRasterVector(img, resize_int)
                    Dim label As Double = labelSet(i)

                    ds.Add(New SampleData(v, label) With {.id = keys(i)})
                Next
            End If

            Return ds.ToArray
        Else
            Dim list = pipeline.TryCreatePipeline(Of Image)(images, env)

            If list.isError Then
                Return list.getError
            End If

            Dim imageList = list.populates(Of Image)(env).ToArray

            Throw New NotImplementedException
        End If
    End Function

    <Extension>
    Private Function sample_dataset_from_df(df As dataframe, labels As Object, env As Environment) As Object
        If TypeOf labels Is String Then
            ' labels comes from one of the data fields inside the given dataframe
            ' single class label
            Dim label As Double() = CLRVector.asNumeric(df(CStr(labels)))

            df = df.projectByColumn({CStr(labels)}, env, reverse:=True)

            Return df _
                .forEachRow _
                .Select(Function(r, i)
                            Return New SampleData(CLRVector.asNumeric(r.value), label(i)) With {
                                .id = r.name
                            }
                        End Function) _
                .ToArray
        ElseIf TypeOf labels Is list Then
            Dim li = DirectCast(labels, list)

            If li.data.All(Function(a) DataFramework.IsNumericCollection(a.GetType)) Then
                ' multiple class
                Dim ds As New List(Of SampleData)

                For Each row As NamedCollection(Of Object) In df.forEachRow
                    ds.Add(New SampleData(CLRVector.asNumeric(row.value), CLRVector.asNumeric(li.getByName(row.name))))
                Next

                Return ds
            Else
                Return Message.InCompatibleType(GetType(String), labels.GetType, env)
            End If
        Else
            labels = REnv.TryCastGenericArray(REnv.asVector(Of Object)(labels), env)

            If DataFramework.IsNumericCollection(labels.GetType) Then
                Dim label As Double() = CLRVector.asNumeric(labels)

                Return df _
                    .forEachRow() _
                    .Select(Function(r, i)
                                Return New SampleData(CLRVector.asNumeric(r.value), label(i)) With {
                                    .id = r.name
                                }
                            End Function) _
                    .ToArray
            ElseIf TypeOf labels Is String() Then
                Dim label_strs As String() = labels

                If df.nrows = label_strs.Length Then
                    Throw New NotImplementedException
                Else
                    ' multiple fileds contains labels vector data
                    Dim features As String() = df.colnames.Where(Function(si) label_strs.IndexOf(si) <= -1).ToArray
                    Dim feature_df = df.forEachRow(features).ToArray
                    Dim labels_df = df.forEachRow(label_strs).ToArray
                    Dim sample_data As SampleData() = feature_df _
                        .Select(Function(r, i)
                                    Return New SampleData(
                                        features:=CLRVector.asNumeric(r.value),
                                        labels:=CLRVector.asNumeric(labels_df(i).value)
                                    ) With {
                                        .id = r.name
                                    }
                                End Function) _
                        .ToArray

                    Return sample_data
                End If
            Else
                Return Message.InCompatibleType(GetType(String), labels.GetType, env)
            End If
        End If
    End Function

    <ExportAPI("auto_encoder")>
    <RApiReturn(GetType(CNNFunction))>
    Public Function auto_encoder(cnn As Object, dataset As SampleData(),
                                 Optional max_loops As Integer = 100,
                                 Optional algorithm As TrainerAlgorithm = Nothing,
                                 Optional env As Environment = Nothing) As Object
        dataset = dataset _
            .Select(Function(si)
                        Return New SampleData(si.features, si.features) With {
                            .id = si.id
                        }
                    End Function) _
            .ToArray

        Return CNNTools.training(
            cnn:=cnn,
            dataset:=dataset,
            max_loops:=max_loops,
            algorithm:=algorithm,
            env:=env
        )
    End Function

    ''' <summary>
    ''' Do CNN network model training
    ''' </summary>
    ''' <param name="cnn"></param>
    ''' <param name="dataset"></param>
    ''' <param name="max_loops"></param>
    ''' <param name="algorithm"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("training")>
    <RApiReturn(GetType(CNNFunction))>
    Public Function training(cnn As Object, dataset As SampleData(),
                             Optional max_loops As Integer = 100,
                             Optional algorithm As TrainerAlgorithm = Nothing,
                             Optional env As Environment = Nothing) As Object

        Dim cnn_val As ConvolutionalNN
        Dim batchSize As Integer = dataset.Length / 30
        Dim alg As TrainerAlgorithm

        If TypeOf cnn Is ConvolutionalNN Then
            cnn_val = cnn
        ElseIf TypeOf cnn Is LayerBuilder Then
            cnn_val = New ConvolutionalNN(cnn)
        Else
            Return Message.InCompatibleType(GetType(ConvolutionalNN), cnn.GetType, env)
        End If

        If Not algorithm Is Nothing Then
            alg = algorithm
        Else
            alg = New AdaGradTrainer(batchSize, 0.001F)
        End If

        alg = alg.SetKernel(cnn_val)
        cnn_val = New Trainer(alg, Sub(s) base.print(s,, env)).train(cnn_val, dataset, max_loops)

        Return New CNNFunction With {.cnn = cnn_val}
    End Function

    ''' <summary>
    ''' Adaptive delta will look at the differences between the expected result and the current result to train the network.
    ''' </summary>
    ''' <param name="batch_size"></param>
    ''' <param name="l2_decay"></param>
    ''' <param name="ro"></param>
    ''' <returns></returns>
    <ExportAPI("ada_delta")>
    Public Function AdaDeltaTrainer(batch_size As Integer, Optional l2_decay As Single = 0.001, Optional ro As Double = 0.95) As TrainerAlgorithm
        Return New AdaDeltaTrainer(batch_size, l2_decay, ro)
    End Function

    ''' <summary>
    ''' The adaptive gradient trainer will over time sum up the square of
    ''' the gradient and use it to change the weights.
    ''' </summary>
    ''' <param name="batch_size"></param>
    ''' <param name="l2_decay"></param>
    ''' <returns></returns>
    <ExportAPI("ada_grad")>
    Public Function AdaGradTrainer(batch_size As Integer, Optional l2_decay As Single = 0.001) As TrainerAlgorithm
        Return New AdaGradTrainer(batch_size, l2_decay)
    End Function

    ''' <summary>
    ''' Adaptive Moment Estimation is an update to RMSProp optimizer. In this running average of both the
    ''' gradients and their magnitudes are used.
    ''' </summary>
    ''' <param name="batch_size"></param>
    ''' <param name="l2_decay"></param>
    ''' <param name="beta1"></param>
    ''' <param name="beta2"></param>
    ''' <returns></returns>
    <ExportAPI("adam")>
    Public Function AdamTrainer(batch_size As Integer,
                                Optional l2_decay As Single = 0.001,
                                Optional beta1 As Double = 0.9,
                                Optional beta2 As Double = 0.999) As TrainerAlgorithm
        Return New AdamTrainer(batch_size, l2_decay, beta1, beta2)
    End Function

    ''' <summary>
    ''' Another extension of gradient descent is due to Yurii Nesterov from 1983,[7] and has been subsequently generalized
    ''' </summary>
    ''' <param name="batch_size"></param>
    ''' <param name="l2_decay"></param>
    ''' <returns></returns>
    <ExportAPI("nesterov")>
    Public Function NesterovTrainer(batch_size As Integer, Optional l2_decay As Single = 0.001) As TrainerAlgorithm
        Return New NesterovTrainer(batch_size, l2_decay)
    End Function

    ''' <summary>
    ''' Stochastic gradient descent (often shortened in SGD), also known as incremental gradient descent, is a
    ''' stochastic approximation of the gradient descent optimization method for minimizing an objective function
    ''' that is written as a sum of differentiable functions. In other words, SGD tries to find minimums or
    ''' maximums by iteration.
    ''' </summary>
    ''' <param name="batch_size"></param>
    ''' <param name="l2_decay"></param>
    ''' <returns></returns>
    <ExportAPI("sgd")>
    Public Function SGDTrainer(batch_size As Integer,
                               Optional learning_rate As Double = 0.01,
                               Optional momentum As Double = 0.9,
                               Optional eps As Double = 0.00000001,
                               Optional l2_decay As Single = 0.001) As TrainerAlgorithm

        Return New SGDTrainer(batch_size, l2_decay) With {
            .eps = eps,
            .learning_rate = learning_rate, .momentum = momentum
        }
    End Function

    ''' <summary>
    ''' This is AdaGrad but with a moving window weighted average
    ''' so the gradient is not accumulated over the entire history of the run.
    ''' it's also referred to as Idea #1 in Zeiler paper on AdaDelta.
    ''' </summary>
    ''' <param name="batch_size"></param>
    ''' <param name="l2_decay"></param>
    ''' <param name="ro"></param>
    ''' <returns></returns>
    <ExportAPI("window_grad")>
    Public Function WindowGradTrainer(batch_size As Integer, Optional l2_decay As Single = 0.001, Optional ro As Double = 0.95) As TrainerAlgorithm
        Return New WindowGradTrainer(batch_size, l2_decay, ro)
    End Function

    <ExportAPI("predict")>
    Public Function predict(cnn As Object, dataset As Object,
                            <RRawVectorArgument>
                            Optional class_labels As Object = "class_%d",
                            Optional is_generative As Boolean = False,
                            Optional env As Environment = Nothing) As Object

        If TypeOf cnn Is ConvolutionalNN Then
            Return CNNFunction.DoPrediction(cnn, dataset, class_labels, is_generative, env)
        ElseIf TypeOf cnn Is CNNFunction Then
            Return DirectCast(cnn, CNNFunction).DoPrediction(dataset, class_labels, is_generative, env)
        Else
            Return Message.InCompatibleType(GetType(CNNFunction), cnn.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' load a CNN model from file
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("CeNiN")>
    Public Function loadModel(file As String) As CeNiN
        Return New CeNiN(file)
    End Function

    ''' <summary>
    ''' classify a object from a given image data
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="target"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("detectObject")>
    Public Function detectObject(model As CeNiN, target As Bitmap, Optional env As Environment = Nothing) As dataframe
        Dim result As NamedValue(Of Double)() = model.DetectObject(target, dev:=env.globalEnvironment.stdout)
        Dim classes As String() = result.Select(Function(v) v.Name).ToArray
        Dim probs As Double() = result.Select(Function(v) v.Value).ToArray

        Return New dataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"class", classes},
                {"probs", probs}
            }
        }
    End Function

    ''' <summary>
    ''' save the CNN model into a binary data file
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("saveModel")>
    <RApiReturn(TypeCodes.boolean)>
    Public Function saveModel(model As Object, file As Object, Optional env As Environment = Nothing) As Object
        Dim buffer = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Dim result As Boolean = True

        If TypeOf model Is CeNiN Then
            result = DirectCast(model, CeNiN).Save(buffer)
        ElseIf TypeOf model Is ConvolutionalNN Then
            Call SaveModelCNN.Write(model, buffer)
        ElseIf TypeOf model Is CNNFunction Then
            Call SaveModelCNN.Write(DirectCast(model, CNNFunction).cnn, buffer)
        Else
            Return Message.InCompatibleType(GetType(ConvolutionalNN), model.GetType, env)
        End If

        If TypeOf file Is String Then
            Call buffer.TryCast(Of Stream).Dispose()
        End If

        Return result
    End Function
End Module
