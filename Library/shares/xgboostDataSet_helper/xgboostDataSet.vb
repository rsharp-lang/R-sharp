Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization

Module xgboostDataSet

    ''' <summary>
    ''' test data set
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="featureNames"></param>
    ''' <returns></returns>
    <Extension>
    Public Function testDataSet(data As dataframe, Optional featureNames As String() = Nothing) As TestData
        Dim matrix As Single()() = data _
            .forEachRow(featureNames) _
            .Select(Function(v)
                        Return CLRVector.asFloat(v.value)
                    End Function) _
            .ToArray
        Dim test As TestData = matrix.ToTestDataSet

        Return test
    End Function

    ''' <summary>
    ''' validation dataset
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="featureNames"></param>
    ''' <returns></returns>
    <Extension>
    Public Function validationDataSet(data As dataframe, label As Double(), Optional featureNames As String() = Nothing) As ValidationData
        Dim matrix As DoubleTagged(Of Single())() = data _
            .forEachRow(featureNames) _
            .Select(Function(v, i)
                        Return New DoubleTagged(Of Single()) With {
                            .Tag = label(i),
                            .Value = CLRVector.asFloat(v.value)
                        }
                    End Function) _
            .ToArray
        Dim train As ValidationData = matrix.ToValidateSet()

        Return train
    End Function

    ''' <summary>
    ''' training dataset
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function trainingDataSet(data As dataframe, label As Double(),
                                    Optional categorical_features As String() = Nothing,
                                    Optional featureNames As String() = Nothing) As TrainData

        Dim matrix As DoubleTagged(Of Single())() = data _
            .forEachRow(featureNames) _
            .Select(Function(v, i)
                        Return New DoubleTagged(Of Single()) With {
                            .Tag = label(i),
                            .Value = CLRVector.asFloat(v.value)
                        }
                    End Function) _
            .ToArray
        Dim colnames As String() = If(featureNames.IsNullOrEmpty, data.colnames, featureNames)
        Dim train As TrainData = matrix.ToTrainingSet(colnames, If(categorical_features, colnames))

        Return train
    End Function
End Module
