
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports dataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports Parameter = Microsoft.VisualBasic.MachineLearning.SVM.Parameter
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("SVM")>
<RTypeExport("problem", GetType(Problem))>
Module SVM

    Sub New()
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of ColorClass)(Function(o) o.ToString)

        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(Problem), AddressOf problemDataframe)
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(ProblemTable), AddressOf problemsDataframe)
    End Sub

    Private Function problemDataframe(problem As Problem, args As list, env As Environment) As dataframe
        Dim data As New dataframe With {.columns = New Dictionary(Of String, Array)}
        Dim index As Integer

        data.columns("[output]") = problem.Y.Select(Function(a) a.name).ToArray

        For i As Integer = 0 To problem.DimensionNames.Length - 1
            index = i
            data.columns(problem.DimensionNames(i)) = problem.X _
                .Select(Function(row) row(index).Value) _
                .ToArray
        Next

        Return data
    End Function

    Private Function problemsDataframe(problems As ProblemTable, args As list, env As Environment) As dataframe
        Dim data As New dataframe With {.columns = New Dictionary(Of String, Array)}
        Dim index As Integer
        Dim attrKey As String

        For Each topic As String In problems.GetTopics
            data.columns($"[{topic}]") = problems.GetTopicLabels(topic)
        Next

        For i As Integer = 0 To problems.DimensionNames.Length - 1
            index = i
            attrKey = problems.DimensionNames(i)
            data.columns(attrKey) = problems.vectors.Select(Function(a) a(attrKey)).ToArray
        Next

        data.rownames = problems.vectors.Keys.ToArray

        Return data
    End Function

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

        Return New Problem With {
            .DimensionNames = dimNames,
            .MaxIndex = dimNames.Length,
            .X = {},
            .Y = {}
        }
    End Function

    ''' <summary>
    ''' append problem data into current problem dataset
    ''' </summary>
    ''' <param name="problem"></param>
    ''' <param name="tag"></param>
    ''' <param name="data"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("append.trainingSet")>
    <RApiReturn(GetType(Problem))>
    Public Function expandProblem(problem As Problem, tag As String(), data As Object, Optional env As Environment = Nothing) As Object
        Dim part As New List(Of Node())()
        Dim labels As New List(Of String)()
        Dim row As (label As String, data As Node())
        Dim n As Integer
        Dim err As Message = Nothing
        Dim getData = getDataLambda(problem.DimensionNames, tag, data, env, err, n)

        If Not err Is Nothing Then
            Return err
        End If

        For i As Integer = 0 To n - 1
            row = getData(i)
            labels.Add(row.label)
            part.Add(row.data)
        Next

        problem.X = problem.X.AsList + part.AsEnumerable
        problem.Y = ClassEncoder.Union(problem.Y, labels)

        Return problem
    End Function

    ''' <summary>
    ''' merge two problem table by row append. 
    ''' (this api is usually apply for join 
    ''' positive set and negative set.) 
    ''' </summary>
    ''' <param name="x">positive set or negative set</param>
    ''' <param name="y">positive set or negative set</param>
    ''' <returns></returns>
    <ExportAPI("join.problems")>
    Public Function joinTable(x As ProblemTable, y As ProblemTable) As ProblemTable
        Return ProblemTable.Append(x, y)
    End Function

    <ExportAPI("parse.SVM_problems")>
    Public Function ParseProblemTableJSON(text As String) As ProblemTable
        Return text.LoadJSON(Of ProblemTable)
    End Function

    Private Function getDataLambda(dimNames As String(), tag As String(), data As Object, env As Environment,
                                   ByRef err As Message,
                                   ByRef n As Integer) As Func(Of Integer, (label As String, data As Node()))

        Dim vectors As New Dictionary(Of String, Double())

        If data Is Nothing Then
            err = Internal.debug.stop("no problem data was provided!", env)
            Return Nothing
        ElseIf TypeOf data Is list Then
            vectors = DirectCast(data, list).AsGeneric(Of Double())(env)
        ElseIf TypeOf data Is dataframe Then
            vectors = DirectCast(data, dataframe).columns.ToDictionary(Function(a) a.Key, Function(a) DirectCast(REnv.asVector(Of Double)(a.Value), Double()))
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

    ''' <summary>
    ''' train SVM model
    ''' </summary>
    ''' <param name="problem"></param>
    ''' <param name="svmType">Type of SVM (default C-SVC)</param>
    ''' <param name="kernelType">Type of kernel function (default Polynomial)</param>
    ''' <param name="degree">Degree in kernel function (default 3).</param>
    ''' <param name="gamma">Gamma in kernel function (default 1/k)</param>
    ''' <param name="coefficient0">Zeroeth coefficient in kernel function (default 0)</param>
    ''' <param name="nu">The parameter nu of nu-SVC, one-class SVM, and nu-SVR (default 0.5)</param>
    ''' <param name="cacheSize">Cache memory size in MB (default 100)</param>
    ''' <param name="C">The parameter C of C-SVC, epsilon-SVR, and nu-SVR (default 1)</param>
    ''' <param name="EPS">Tolerance of termination criterion (default 0.001)</param>
    ''' <param name="P">The epsilon in loss function of epsilon-SVR (default 0.1)</param>
    ''' <param name="shrinking">Whether to use the shrinking heuristics, (default True)</param>
    ''' <param name="probability">
    ''' Whether to train an SVC or SVR model for probability estimates, (default False)
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("trainSVMModel")>
    <RApiReturn(GetType(SVMModel), GetType(SVMMultipleSet))>
    Public Function trainSVMModel(problem As Object,
                                  Optional svmType As SvmType = SvmType.C_SVC,
                                  Optional kernelType As KernelType = KernelType.RBF,
                                  Optional degree As Integer = 3,
                                  Optional gamma As Double = 0.5,
                                  Optional coefficient0 As Double = 0,
                                  Optional nu As Double = 0.5,
                                  Optional cacheSize As Integer = 40,
                                  Optional C As Double = 1,
                                  Optional EPS As Double = 0.001,
                                  Optional P As Double = 0.1,
                                  Optional shrinking As Boolean = True,
                                  Optional probability As Boolean = False,
                                  Optional weights As list = Nothing,
                                  Optional verbose As Boolean = False,
                                  Optional env As Environment = Nothing) As Object

        Dim param As New Parameter With {
            .SvmType = svmType,
            .KernelType = kernelType,
            .C = C,
            .CacheSize = cacheSize,
            .Coefficient0 = coefficient0,
            .Degree = degree,
            .EPS = EPS,
            .Gamma = gamma,
            .Nu = nu,
            .P = P,
            .Probability = probability,
            .Shrinking = shrinking
        }

        If problem Is Nothing Then
            Return Internal.debug.stop("the required SVM training dataset can not be nothing!", env)
        End If

        If Not (TypeOf problem Is Problem OrElse TypeOf problem Is ProblemTable) Then
            Return Message.InCompatibleType(GetType(Problem), problem.GetType, env)
        End If

        If Not weights Is Nothing Then
            With weights.AsGeneric(Of Double)(env)
                For Each label In .AsEnumerable
                    Call param.Weights.Add(CInt(label.Key), label.Value)
                Next
            End With
        Else
            If TypeOf problem Is Problem Then
                For Each label As ColorClass In DirectCast(problem, Problem) _
                    .Y _
                    .GroupBy(Function(a) a.name) _
                    .Select(Function(a) a.First)

                    Call param.Weights.Add(label.enumInt, 1)
                Next
            End If
        End If

        Training.IsVerbose = verbose

        If TypeOf problem Is Problem Then
            Return getSvmModel(DirectCast(problem, Problem), param)
        Else
            Dim table As ProblemTable = DirectCast(problem, ProblemTable)
            Dim result As New Dictionary(Of String, SVMModel)

            For Each topic As String In table.GetTopics
                problem = table.GetProblem(topic)

                For Each label As ColorClass In DirectCast(problem, Problem) _
                    .Y _
                    .GroupBy(Function(a) a.name) _
                    .Select(Function(a) a.First)

                    ' 因为会被反复使用，所以可能会出现重名的问题
                    ' 在这里直接设置
                    param.Weights.Item(label.enumInt) = 1
                Next

                result(topic) = DirectCast(problem, Problem).getSvmModel(param)
            Next

            Return New SVMMultipleSet With {
                .dimensionNames = table.DimensionNames,
                .topics = result
            }
        End If
    End Function

    <Extension>
    Private Function getSvmModel(problem As Problem, par As Parameter) As SVMModel
        Dim transform As RangeTransform = RangeTransform.Compute(problem)
        Dim model As Model = Training.Train(transform.Scale(problem), par)

        Return New SVMModel With {
            .transform = transform,
            .model = model,
            .factors = New ClassEncoder(problem.Y)
        }
    End Function

    <ExportAPI("svm_classify")>
    Public Function svmClassify(svm As Object, data As Object, Optional env As Environment = Nothing) As Object
        If svm Is Nothing Then
            Return Internal.debug.stop("the required svm model can not be nothing!", env)
        ElseIf TypeOf svm Is SVMModel Then
            Return DirectCast(svm, SVMModel).svmClassify1(data, env)
        ElseIf TypeOf svm Is SVMMultipleSet Then
            Return DirectCast(svm, SVMMultipleSet).svmClassify2(data, env)
        Else
            Return Message.InCompatibleType(GetType(SVMModel), svm.GetType, env)
        End If
    End Function

    <Extension>
    Private Function svmClassify2(models As SVMMultipleSet, data As Object, env As Environment) As Object
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

        Dim label As Double
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

            For Each SVM In outputVectors
                datum = SVM.transform.Transform(row.data)
                label = SVM.model.Predict(datum)
                factor = SVM.factors.GetColor(label)
                info.Add(SVM.topic, factor.name)
            Next

            result += New EntityObject With {.ID = uid, .Properties = info}
        Next

        Return result.ToArray
    End Function

    <Extension>
    Private Function svmClassify1(svm As SVMModel, data As Object, env As Environment) As Object
        Dim row As (label As String, data As Node())
        Dim n As Integer
        Dim err As Message = Nothing
        Dim getData = getDataLambda(svm.DimensionNames, {"n/a"}, data, env, err, n)
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

        Dim label As Double
        Dim factor As ColorClass

        For i As Integer = 0 To n - 1
            row = getData(i)
            datum = transform.Transform(row.data)
            label = svm.model.Predict(datum)
            factor = svm.factors.GetColor(label)
            labels.Add(names(i), factor)
        Next

        Return New list With {.slots = labels}
    End Function
End Module