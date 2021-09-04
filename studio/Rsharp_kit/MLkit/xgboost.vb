
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.TagData
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

    End Function

    <ExportAPI("predict")>
    Public Function predict(gbm As GBM, data As TestData) As Double()

    End Function

    <ExportAPI("tree")>
    Public Function tree(modelLines As String()) As GBM

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
            Dim train As ValidationData = matrix.ToValidateSet(data.colnames)

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
            Dim train As TrainData = matrix.ToTrainingSet(data.colnames)

            Return train
        End If
    End Function

End Module
