
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

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
    <RApiReturn(GetType(Problem))>
    Public Function expandProblem(problem As Problem, tag As Integer(), data As Object, Optional env As Environment = Nothing) As Object
        Dim part As New List(Of Node())()
        Dim labels As New List(Of Double)()
        Dim vectors As New Dictionary(Of String, Double())

        If data Is Nothing Then
            Return Internal.debug.stop("no problem data was provided!", env)
        ElseIf TypeOf data Is list Then
            vectors = DirectCast(data, list).AsGeneric(Of Double())(env)
        ElseIf TypeOf data Is dataframe Then
            vectors = DirectCast(data, dataframe).columns.ToDictionary(Function(a) a.Key, Function(a) DirectCast(REnv.asVector(Of Double)(a.Value), Double()))
        Else
            Return Message.InCompatibleType(GetType(dataframe), data.GetType, env)
        End If

        Dim n As Integer = vectors.Values.First.Length
        Dim getTag As Func(Of Integer, Integer)

        If tag.Length = 1 Then
            getTag = Function() tag(Scan0)
        Else
            getTag = Function(i) tag(i)
        End If

        Dim index As Integer
        Dim row As Node()

        For i As Integer = 0 To n - 1
            labels.Add(getTag(i))
            index = i
            row = problem.DimensionNames _
                .Select(Function([dim], j)
                            Return New Node(j + 1, vectors([dim])(index))
                        End Function) _
                .ToArray
            part.Add(row)
        Next

        problem.X = problem.X.AsList + part.AsEnumerable
        problem.Y = problem.Y.AsList + labels.AsEnumerable

        Return problem
    End Function
End Module
