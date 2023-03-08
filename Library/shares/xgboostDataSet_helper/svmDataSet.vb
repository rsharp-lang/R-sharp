Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports dataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

Module svmDataSet

    Public Function svrProblem(dimensionNames As String(), label As String, data As dataframe, env As Environment) As [Variant](Of Message, SVM.Problem)
        Dim y As Double() = CLRVector.asNumeric(data(label))
        Dim n As Integer
        Dim err As Message = Nothing
        Dim getData = getDataLambda(dimensionNames, New String(y.Length - 1) {}, data, env, err, n)
        Dim row As (label As String, data As Node())
        Dim part As New List(Of Node())()

        If Not err Is Nothing Then
            Return err
        End If

        For i As Integer = 0 To n - 1
            row = getData(i)
            part.Add(row.data)
        Next

        Return New SVM.Problem With {
            .dimensionNames = dimensionNames,
            .maxIndex = .dimensionNames.Length,
            .Y = y _
                .Select(Function(yi)
                            Return New ColorClass With {
                                .color = "#000000",
                                .factor = yi,
                                .name = yi.ToString
                            }
                        End Function) _
                .ToArray,
            .X = part.ToArray
        }
    End Function

    Public Function svmProblem(dimensionNames As String(),
                               tag As String(),
                               data As Object,
                               env As Environment) As [Variant](Of Message, SVM.Problem)

        Dim part As New List(Of Node())()
        Dim labels As New List(Of String)()
        Dim row As (label As String, data As Node())
        Dim n As Integer
        Dim err As Message = Nothing
        Dim getData = getDataLambda(dimensionNames, tag, data, env, err, n)

        If Not err Is Nothing Then
            Return err
        End If

        For i As Integer = 0 To n - 1
            row = getData(i)
            labels.Add(row.label)
            part.Add(row.data)
        Next

        Return New SVM.Problem With {
            .dimensionNames = dimensionNames,
            .maxIndex = .dimensionNames.Length,
            .X = part.ToArray,
            .Y = New ClassEncoder(labels).PopulateFactors.ToArray
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dimNames">
    ''' the feature names of the training dataset
    ''' </param>
    ''' <param name="tag">the class labels</param>
    ''' <param name="data">dataframe/list</param>
    ''' <param name="env"></param>
    ''' <param name="err"></param>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Friend Function getDataLambda(dimNames As String(), tag As String(), data As Object, env As Environment,
                                  ByRef err As Message,
                                  ByRef n As Integer) As Func(Of Integer, (label As String, data As Node()))

        Dim vectors As New Dictionary(Of String, Double())

        If data Is Nothing Then
            err = Internal.debug.stop("no problem data was provided!", env)
            Return Nothing
        ElseIf TypeOf data Is list Then
            With DirectCast(data, list)
                For Each name As String In dimNames
                    If Not .hasName(name) Then
                        err = Internal.debug.stop($"missing dimension {name}!", env)
                        Return Nothing
                    End If

                    vectors(name) = .getValue(Of Double())(name, env)
                Next
            End With
        ElseIf TypeOf data Is dataframe Then
            With DirectCast(data, dataframe)
                For Each name As String In dimNames
                    If Not .hasName(name) Then
                        err = Internal.debug.stop($"missing dimension {name}!", env)
                        Return Nothing
                    End If

                    vectors(name) = CLRVector.asNumeric(.columns(name))
                Next
            End With
        Else
            err = Message.InCompatibleType(GetType(Object), data.GetType, env)
            Return Nothing
        End If

        Dim getTag As Func(Of Integer, String)

        n = vectors.Values.First.Length

        If tag.Length = 1 Then
            getTag = Function() tag(Scan0)
        Else
            getTag = Function(i) tag(i)
        End If

        Return Function(i)
                   Return (getTag(i), dimNames.Select(Function([dim], j) New Node(j + 1, vectors([dim])(i))).ToArray)
               End Function
    End Function

End Module