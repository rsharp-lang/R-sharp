#Region "Microsoft.VisualBasic::c5277b79488fd1ed776d0193deafb9a6, studio\Rsharp_kit\MLkit\MachineLearning\VAE.vb"

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

'   Total Lines: 65
'    Code Lines: 20 (30.77%)
' Comment Lines: 35 (53.85%)
'    - Xml Docs: 11.43%
' 
'   Blank Lines: 10 (15.38%)
'     File Size: 2.39 KB


' Module VAE
' 
'     Function: Trainer
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' VAE embedding method implements
''' </summary>
<Package("VAE")>
Module VAE

    ''' <summary>
    ''' create vae training algorithm
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("vae")>
    Public Function Trainer(<RRawVectorArgument>
                            dims As Object,
                            Optional latent_dims As Integer = 100,
                            Optional env As Environment = Nothing) As Object

        'Dim imgDims = InteropArgumentHelper.getSize(dims, env, [default]:=Nothing)

        'If imgDims.StringEmpty Then
        '    Return Internal.debug.stop("the required image dimension size must be specificed!", env)
        'End If

        'With imgDims.SizeParser
        '    Return New Trainer(.Width, .Height, latent_dims)
        'End With
    End Function

    '<ExportAPI("train")>
    '<RApiReturn(GetType(VAE))>
    'Public Function train(vae As Trainer,
    '                      <RRawVectorArgument>
    '                      ds As Object,
    '                      Optional steps As Integer = 100,
    '                      Optional env As Environment = Nothing) As Object

    '    Dim dataset_input As stdvec()

    '    If TypeOf ds Is list Then
    '        dataset_input = DirectCast(ds, list).slots _
    '            .Values _
    '            .Select(Function(o) New stdvec(CLRVector.asNumeric(o))) _
    '            .ToArray
    '    ElseIf TypeOf ds Is DataSet Then
    '        dataset_input = DirectCast(ds, DataSet).DataSamples _
    '            .AsEnumerable _
    '            .Select(Function(si) New stdvec(si.vector)) _
    '            .ToArray
    '    Else
    '        Throw New NotImplementedException
    '    End If

    '    Call vae.train(dataset_input, steps)

    '    Return vae.autoencoder
    'End Function

    ''' <summary>
    ''' make matrix data embedding via the VAE method
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="dims"></param>
    ''' <param name="batch_size"></param>
    ''' <param name="max_iteration"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' a dataframe object contains the embedding result.
    ''' </returns>
    ''' <remarks>
    ''' input training data is the output result
    ''' </remarks>
    <ExportAPI("embedding")>
    Public Function embedding_vae(<RRawVectorArgument> x As Object,
                                  Optional dims As Integer = 9,
                                  Optional batch_size As Integer = 100,
                                  Optional max_iteration As Integer = 1000,
                                  Optional verbose As Boolean? = Nothing,
                                  Optional env As Environment = Nothing) As Object

        Dim dataset = SampledataParser.ConvertVAE(x, env)

        If dataset Like GetType(Message) Then
            Return dataset.TryCast(Of Message)
        End If

        Dim matrix As SampleData() = SampleData.TransformDataset(dataset, is_generative:=True, is_training:=True).ToArray
        Dim input_width As Integer = matrix.First.features.Length
        Dim layers = New LayerBuilder() + CNNTools.input_layer({1, 1}, input_width, 1) +
            CNNTools.full_connected_layer(input_width / 2) +
            CNNTools.full_connected_layer(input_width / 5) +
            CNNTools.full_connected_layer(dims) +  ' embedding layer
            CNNTools.full_connected_layer(input_width / 3) +
            CNNTools.full_connected_layer(input_width) +
            CNNTools.regression_layer()

        Dim model As New ConvolutionalNN(layers)
        Dim alg As New AdaGradTrainer(batch_size, 0.001F)
        Dim cnn_val = New Trainer(alg:=alg.SetKernel(model),
                              log:=Sub(s) base.print(s,, env),
                              verbose:=If(verbose Is Nothing, env.verboseOption(False), CBool(verbose))) _
            .train(matrix, max_iteration)

        ' make embedding
        ' get embedding result from the embedding layer
        Dim embedding_layer As ConvolutionalNN = model.take(3)
        Dim rows As New List(Of NamedCollection(Of Double))
        Dim embedding As Double()

        For Each sample As SampleData In matrix
            embedding = embedding_layer.predict(sample.features)
        Next
    End Function
End Module
