#Region "Microsoft.VisualBasic::7e3b50c56908fd5e28f76a6db6685887, R-sharp\studio\Rsharp_kit\MLkit\xgboost.vb"

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

    '   Total Lines: 137
    '    Code Lines: 95
    ' Comment Lines: 27
    '   Blank Lines: 15
    '     File Size: 5.78 KB


    ' Module xgboost
    ' 
    '     Function: DMatrix, predict, serialize, tree, xgboost
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' Extreme Gradient Boosting
''' </summary>
<Package("xgboost")>
Public Module xgboost

    ''' <summary>
    ''' eXtreme Gradient Boosting Training
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="params"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("xgboost")>
    Public Function xgboost(data As TrainData, Optional validates As ValidationData = Nothing,
                            <RListObjectArgument>
                            Optional params As list = Nothing,
                            Optional env As Environment = Nothing) As GBM

        If params.length = 1 AndAlso TypeOf params.slots.First.Value Is list Then
            params = params.slots.First.Value
        End If

        Dim early_stopping_round = params.getValue("early_stopping_rounds", env, 10)
        Dim maximize = params.getValue("maximize", env, True)
        Dim eval_metric As Metrics = Metric.Parse(params.getValue("eval_metric", env, "auc"))
        Dim loss = params.getValue("loss", env, "logloss")
        Dim eta = params.getValue("eta", env, 0.3)
        Dim num_boost_round = params.getValue("num_boost_round", env, 20)
        Dim max_depth = params.getValue("max_depth", env, 7)
        Dim scale_pos_weight = params.getValue("scale_pos_weight", env, 1.0)
        Dim rowsample = params.getValue("subsample", env, 0.8)
        Dim colample = params.getValue("colsample", env, 0.8)
        Dim min_child_weight = params.getValue("min_child_weight", env, 1.0)
        Dim min_sample_split = params.getValue("min_sample_split", env, 5)
        Dim lambda = params.getValue("reg_lambda", env, 1.0)
        Dim gamma = params.getValue("gamma", env, 0.0)
        Dim num_thread = params.getValue("num_thread", env, -1)
        Dim model As New GBM

        Call model.fit(data, validates)

        Return model
    End Function

    <ExportAPI("predict")>
    Public Function predict(gbm As GBM, data As TestData) As Double()
        Return gbm.predict(data.origin_feature)
    End Function

    <ExportAPI("serialize")>
    Public Function serialize(model As GBM) As String()
        Return ModelSerializer.save_model(model).ToArray
    End Function

    <ExportAPI("parseTree")>
    Public Function tree(modelLines As String()) As GBM
        Return ModelSerializer.load_model(modelLines)
    End Function

    ''' <summary>
    ''' ### Construct xgb.DMatrix object
    ''' 
    ''' Construct xgb.DMatrix object from either a dense matrix, 
    ''' a sparse matrix, or a local file. Supported input file 
    ''' formats are either a libsvm text file or a binary file 
    ''' that was created previously by xgb.DMatrix.save).
    ''' </summary>
    ''' <param name="data">
    ''' a matrix object (either numeric or integer), a dgCMatrix 
    ''' object, or a character string representing a filename.
    ''' </param>
    ''' <param name="label"></param>
    ''' <returns></returns>
    <ExportAPI("xgb.DMatrix")>
    <RApiReturn(GetType(TrainData), GetType(TestData), GetType(ValidationData))>
    Public Function DMatrix(data As dataframe,
                            Optional label As Double() = Nothing,
                            Optional validate_set As Boolean = False,
                            Optional categorical_features As String() = Nothing,
                            Optional env As Environment = Nothing) As Object

        If label.IsNullOrEmpty Then
            ' test data set
            Dim matrix As Single()() = data _
                .forEachRow _
                .Select(Function(v)
                            Return DirectCast(REnv.asVector(Of Single)(v.value), Single())
                        End Function) _
                .ToArray
            Dim test As TestData = matrix.ToTestDataSet

            Return test
        ElseIf validate_set Then
            ' validation dataset
            Dim matrix As DoubleTagged(Of Single())() = data _
                .forEachRow() _
                .Select(Function(v, i)
                            Return New DoubleTagged(Of Single()) With {
                                .Tag = label(i),
                                .Value = DirectCast(REnv.asVector(Of Single)(v.value), Single())
                            }
                        End Function) _
                .ToArray
            Dim train As ValidationData = matrix.ToValidateSet()

            Return train
        Else
            ' training dataset
            Dim matrix As DoubleTagged(Of Single())() = data _
                .forEachRow() _
                .Select(Function(v, i)
                            Return New DoubleTagged(Of Single()) With {
                                .Tag = label(i),
                                .Value = DirectCast(REnv.asVector(Of Single)(v.value), Single())
                            }
                        End Function) _
                .ToArray
            Dim colnames As String() = data.colnames
            Dim train As TrainData = matrix.ToTrainingSet(colnames, If(categorical_features, colnames))

            Return train
        End If
    End Function

End Module
