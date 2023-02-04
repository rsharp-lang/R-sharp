Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports dataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

''' <summary>
''' solve svm model
''' </summary>
Module solveSVM

    <Extension>
    Public Function svmClassify1(svm As SVM.SVMModel, data As Object, env As Environment) As Object
        Dim row As (label As String, data As Node())
        Dim n As Integer
        Dim err As Message = Nothing
        Dim getData = svmDataSet.getDataLambda(svm.dimensionNames, {"n/a"}, data, env, err, n)
        Dim datum As Node()

        If Not err Is Nothing Then
            Return err
        End If

        Dim transform As IRangeTransform = svm.transform
        Dim labels As New Dictionary(Of String, Object)
        Dim names As String()

        If TypeOf data Is dataframe Then
            names = DirectCast(data, dataframe).getRowNames
        Else
            names = DirectCast(data, list).getNames
        End If

        Dim label As SVMPrediction
        Dim factor As ColorClass

        For i As Integer = 0 To n - 1
            row = getData(i)
            datum = transform.Transform(row.data)
            label = svm.model.Predict(datum)
            factor = svm.factors.GetColor(label.class)
            labels.Add(names(i), factor)
        Next

        Return New list With {.slots = labels}
    End Function

    <Extension>
    Public Function svmClassify2(models As SVMMultipleSet, data As Object, env As Environment) As Object
        Dim row As (label As String, data As Node())
        Dim n As Integer
        Dim err As Message = Nothing
        Dim getData = getDataLambda(models.dimensionNames, {"n/a"}, data, env, err, n)
        Dim datum As Node()

        If Not err Is Nothing Then
            Return err
        End If

        Dim names As String()

        If TypeOf data Is dataframe Then
            names = DirectCast(data, dataframe).getRowNames
        Else
            names = DirectCast(data, list).getNames
        End If

        Dim label As SVMPrediction
        Dim factor As ColorClass
        Dim uid As String
        Dim outputVectors = models.topics _
            .Select(Function(a)
                        Return (topic:=a.Key, a.Value.transform, a.Value.model, a.Value.factors)
                    End Function) _
            .ToArray
        Dim info As New Dictionary(Of String, String)
        Dim result As New List(Of EntityObject)

        For i As Integer = 0 To n - 1
            row = getData(i)
            uid = names(i)
            info = New Dictionary(Of String, String)

            For Each SVM As (topic$, transform As IRangeTransform, model As SVM.Model, factors As ClassEncoder) In outputVectors
                datum = SVM.transform.Transform(row.data)
                label = SVM.model.Predict(datum)
                factor = SVM.factors.GetColor(label.class)
                info.Add(SVM.topic, factor.name)
            Next

            result += New EntityObject With {
                .ID = uid,
                .Properties = info
            }
        Next

        Return result.ToArray
    End Function
End Module