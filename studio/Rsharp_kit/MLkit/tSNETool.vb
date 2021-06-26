
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MachineLearning.tSNE
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("t-SNE")>
Module tSNETool

    Sub New()
        Call Internal.generic.add("plot", GetType(tSNE), AddressOf datasetKit.EmbeddingRender)

    End Sub

    <ExportAPI("algorithm")>
    Public Function createtSNEAlgorithm(Optional perplexity As Double = 30,
                                        Optional dimension As Integer = 2,
                                        Optional epsilon As Double = 10) As tSNE

        Return New tSNE(perplexity, dimension, epsilon)
    End Function

    <ExportAPI("data")>
    <RApiReturn(GetType(tSNEDataSet))>
    Public Function LoadDataSet(tSNE As tSNE, <RRawVectorArgument> dataset As Object, Optional env As Environment = Nothing) As Object
        If dataset Is Nothing Then
            Return Internal.debug.stop("the required dataset can not be nothing!", env)
        End If

        Dim matrix As New List(Of Double())
        Dim labels As String()

        If TypeOf dataset Is dataframe Then
            With DirectCast(dataset, dataframe)
                labels = .getRowNames

                For Each row In .forEachRow
                    matrix.Add(REnv.asVector(Of Double)(row.ToArray))
                Next
            End With
        Else
            Return Message.InCompatibleType(GetType(dataframe), dataset.GetType, env)
        End If

        Call tSNE.InitDataRaw(matrix)

        Return New tSNEDataSet With {
            .algorithm = tSNE,
            .labels = labels
        }
    End Function

    <ExportAPI("solve")>
    Public Function Solve(tSNE As tSNEDataSet, Optional iterations% = 500) As dataframe
        For i As Integer = 0 To iterations
            Call tSNE.algorithm.Step()
        Next

        Return tSNE.GetOutput
    End Function

End Module

Public Class tSNEDataSet

    Friend algorithm As tSNE
    Friend labels As String()

    Public Function GetOutput() As dataframe
        Dim matrix = algorithm.GetEmbedding
        Dim result As New dataframe With {.columns = New Dictionary(Of String, Array)}
        Dim width As Integer = matrix(Scan0).Length

        For i As Integer = 0 To width - 1
#Disable Warning
            result.columns($"X{i + 1}") = matrix _
                .Select(Function(r) r(i)) _
                .ToArray
#Enable Warning
        Next

        Return result
    End Function

End Class