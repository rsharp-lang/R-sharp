
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("SVM")>
<RTypeExport("problem", GetType(Problem))>
Module SVM

    <ExportAPI("svm.problem")>
    <RApiReturn(GetType(Problem))>
    Public Function NewProblem(<RRawVectorArgument> dimensions As Object, Optional env As Environment = Nothing) As Object
        If dimensions Is Nothing Then
            Return Internal.debug.stop("the required SVM dimension can not be nothing!", env)
        End If

        Dim dimRaw = pipeline.TryCreatePipeline(Of Integer)(dimensions, env, suppress:=True)
        Dim dimNames As String()

        If dimRaw.isError Then
            dimRaw = pipeline.TryCreatePipeline(Of String)(dimensions, env)

            If dimRaw.isError Then
                Return dimRaw.getError
            End If

            dimNames = dimRaw.populates(Of String)(env).ToArray
        Else
            Dim ints = dimRaw.populates(Of Integer)(env).ToArray

            If ints.Length = 1 Then
                dimNames = ints(Scan0) _
                    .Sequence _
                    .Select(Function(i) $"X_{i}") _
                    .ToArray
            Else
                dimNames = ints.Select(Function(a) CStr(a)).ToArray
            End If
        End If

        Return New Problem With {.DimensionNames = dimNames}
    End Function

    <ExportAPI("append.trainingSet")>
    Public Function expandProblem(problem As Problem, tag As Integer, data As dataframe) As Problem
        Dim part As New List(Of Node())()
        Dim labels As New List(Of Double)()

    End Function
End Module
