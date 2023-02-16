#Region "Microsoft.VisualBasic::0bef29e0cf9a75725c3614a847481e07, R-sharp\studio\Rsharp_kit\MLkit\MachineLearning\xgboost.vb"

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

    '   Total Lines: 144
    '    Code Lines: 87
    ' Comment Lines: 45
    '   Blank Lines: 12
    '     File Size: 5.50 KB


    ' Module xgboost
    ' 
    '     Function: DMatrix, predict, serialize, tree, xgboost
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

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
        Dim eval_metric As Metrics = Metric.Parse(params.getValue("eval_metric", env, "auc"))  ' mse for regression
        Dim loss = params.getValue("loss", env, "logloss") ' squareloss for regression?
        Dim eta = params.getValue("eta", env, 0.3)
        Dim num_boost_round = params.getValue("num_boost_round", env, 20)
        Dim max_depth = params.getValue("max_depth", env, 7)
        Dim scale_pos_weight = params.getValue("scale_pos_weight", env, 1.0)
        Dim rowsample = params.getValue("subsample", env, 0.8)
        Dim colsample = params.getValue("colsample", env, 0.8)
        Dim min_child_weight = params.getValue("min_child_weight", env, 1.0)
        Dim min_sample_split = params.getValue("min_sample_split", env, 5)
        Dim lambda = params.getValue("reg_lambda", env, 1.0)
        Dim gamma = params.getValue("gamma", env, 0.0)
        Dim num_thread = params.getValue("num_thread", env, -1)
        Dim model As New GBM

        Call model.fit(
            trainset:=data,
            valset:=validates,
            early_stopping_rounds:=early_stopping_round,
            maximize:=maximize,
            eval_metric:=eval_metric,
            loss:=loss,
            eta:=eta,
            num_boost_round:=num_boost_round,
            num_thread:=num_thread,
            scale_pos_weight:=scale_pos_weight,
            rowsample:=rowsample,
            lambda:=lambda,
            gamma:=gamma,
            colsample:=colsample,
            max_depth:=max_depth,
            min_child_weight:=min_child_weight,
            min_sample_split:=min_sample_split
        )

        Return model
    End Function

    ''' <summary>
    ''' do predictions
    ''' </summary>
    ''' <param name="gbm"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <ExportAPI("predict")>
    Public Function predict(gbm As GBM, data As TestData) As Double()
        Return gbm.predict(data.origin_feature)
    End Function

    ''' <summary>
    ''' save model
    ''' </summary>
    ''' <param name="model"></param>
    ''' <returns></returns>
    <ExportAPI("serialize")>
    Public Function serialize(model As GBM) As String()
        Return ModelSerializer.save_model(model).ToArray
    End Function

    ''' <summary>
    ''' load model
    ''' </summary>
    ''' <param name="modelLines"></param>
    ''' <returns></returns>
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
    ''' <param name="label">
    ''' labels for training data should be integer value
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("xgb.DMatrix")>
    <RApiReturn(GetType(TrainData), GetType(TestData), GetType(ValidationData))>
    Public Function DMatrix(data As dataframe,
                            Optional label As Double() = Nothing,
                            Optional validate_set As Boolean = False,
                            Optional categorical_features As String() = Nothing,
                            Optional feature_names As String() = Nothing,
                            Optional env As Environment = Nothing) As Object

        If label.IsNullOrEmpty Then
            ' test data set
            Return data.testDataSet(featureNames:=feature_names)
        ElseIf validate_set Then
            ' validation dataset
            Return data.validationDataSet(label, featureNames:=feature_names)
        Else
            ' training dataset
            Return data.trainingDataSet(
                label:=label,
                categorical_features:=categorical_features,
                featureNames:=feature_names
            )
        End If
    End Function

End Module
