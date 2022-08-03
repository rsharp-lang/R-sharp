#Region "Microsoft.VisualBasic::d109d3d243eda1cc2247e1a21a62e6e4, R-sharp\studio\Rsharp_kit\MLkit\SVM.vb"

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

    '   Total Lines: 736
    '    Code Lines: 543
    ' Comment Lines: 83
    '   Blank Lines: 110
    '     File Size: 29.01 KB


    ' Module SVMkit
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: expandProblem, getDataLambda, (+2 Overloads) getSvmModel, joinTable, NewProblem
    '               packCache, ParseProblemTableJSON, parseSVMJSON, plotROC, problemDataframe
    '               problemsDataframe, problemValidateLabels, svmClassify, svmClassify1, svmClassify2
    '               SVMJSON, svmModelTrimNULL, svmValidates, trainSVMModel, trimSingleProblem
    '               validateMultipleSvmModel, validateSingleSvmModel
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Evaluation
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports Microsoft.VisualBasic.MachineLearning.SVM.StorageProcedure
Imports Microsoft.VisualBasic.MIME.application
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports dataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports Parameter = Microsoft.VisualBasic.MachineLearning.SVM.Parameter
Imports REnv = SMRUCC.Rsharp.Runtime
Imports stdNum = System.Math

<Package("SVM")>
<RTypeExport("problem", GetType(Problem))>
<RTypeExport("svm", GetType(SVMModel))>
<RTypeExport("svmSet", GetType(SVMMultipleSet))>
Module SVMkit

    Sub New()
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of ColorClass)(Function(o) o.ToString)

        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(Problem), AddressOf problemDataframe)
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(ProblemTable), AddressOf problemsDataframe)

        Call Internal.generic.add("plot", GetType(PerformanceEvaluator), AddressOf plotROC)
    End Sub

    Private Function plotROC(validates As PerformanceEvaluator, args As list, env As Environment) As Object
        Dim ROC = validates.ROCCurve
        Dim curve As New SerialData With {
            .color = Color.Black,
            .lineType = DashStyle.Solid,
            .pointSize = 5,
            .shape = LegendStyles.Circle,
            .title = validates.AuC,
            .width = 5,
            .pts = ROC _
                .Select(Function(a)
                            Return New PointData() With {
                                .pt = a
                            }
                        End Function) _
                .ToArray
        }

        Return ROCPlot.Plot(roc:=curve)
    End Function

    Private Function problemDataframe(problem As Problem, args As list, env As Environment) As dataframe
        Dim data As New dataframe With {.columns = New Dictionary(Of String, Array)}
        Dim index As Integer

        data.columns("[output]") = problem.Y.Select(Function(a) a.name).ToArray

        For i As Integer = 0 To problem.dimensionNames.Length - 1
            index = i
            data.columns(problem.dimensionNames(i)) = problem.X _
                .Select(Function(row) row(index).value) _
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

        For i As Integer = 0 To problems.dimensionNames.Length - 1
            index = i
            attrKey = problems.dimensionNames(i)
            data.columns(attrKey) = problems.vectors.Select(Function(a) a(attrKey)).ToArray
        Next

        data.rownames = problems.vectors.Keys.ToArray

        Return data
    End Function

    ''' <summary>
    ''' removes all columns that will all value equals to each other
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("problem.trim")>
    <RApiReturn(GetType(Problem), GetType(ProblemTable))>
    Public Function svmModelTrimNULL(model As Object, Optional env As Environment = Nothing) As Object
        If model Is Nothing Then
            Return Nothing
        ElseIf TypeOf model Is Problem Then
            Return trimSingleProblem(DirectCast(model, Problem))
        ElseIf TypeOf model Is ProblemTable Then
            Dim problem As ProblemTable = DirectCast(model, ProblemTable).Clone

            For Each name As String In problem.dimensionNames
                Dim val As Double = problem.vectors(Scan0)(name)

                If problem.vectors.All(Function(vec) stdNum.Abs(vec(name) - val) <= 0.000000001) Then
                    For Each vec As SupportVector In problem.vectors
                        vec.Properties.Remove(name)
                    Next
                End If
            Next

            problem.dimensionNames = problem.vectors _
                .Select(Function(vec) vec.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray

            Return problem
        Else
            Return Message.InCompatibleType(GetType(Problem), model.GetType, env)
        End If
    End Function

    Private Function trimSingleProblem(problem As Problem) As Problem
        Dim trim As New List(Of Node())
        Dim dimNames As New List(Of String)

        For i As Integer = 0 To problem.maxIndex - 1
            ' 如果这一列全部等于第一个值
            ' 则删除
            Dim val As Double = problem.X(Scan0)(i).value
            Dim j = i

            If problem.X.Any(Function(row) stdNum.Abs(row(j).value - val) > 0.0000001) Then
                trim.Add(problem.X.Select(Function(row) row(j)).ToArray)
                dimNames.Add(problem.dimensionNames(j))
            End If
        Next

        For Each row As Node() In trim.PopAll.MatrixTranspose.ToArray
            trim.Add(row.Select(Function(node, i) New Node(i, node.value)).ToArray)
        Next

        Return New Problem With {
            .dimensionNames = dimNames,
            .maxIndex = dimNames.Count,
            .X = trim.ToArray,
            .Y = problem.Y _
                .Select(Function(a)
                            Return New ColorClass With {
                                .color = a.color,
                                .enumInt = a.enumInt,
                                .name = a.name
                            }
                        End Function) _
                .ToArray
        }
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
            .dimensionNames = dimNames,
            .maxIndex = dimNames.Length,
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
        Dim getData = getDataLambda(problem.dimensionNames, tag, data, env, err, n)

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

    ''' <summary>
    ''' extract the classify labels part from the 
    ''' validation set object.
    ''' </summary>
    ''' <param name="problem"></param>
    ''' <returns></returns>
    <ExportAPI("problem.validateLabels")>
    Public Function problemValidateLabels(problem As ProblemTable) As dataframe
        Dim labels As New dataframe With {.columns = New Dictionary(Of String, Array)}

        For Each dimension As String In problem.GetTopics
            labels.columns.Add(dimension, problem.GetTopicLabels(dimension))
        Next

        labels.rownames = problem.vectors.Select(Function(a) a.id).ToArray

        Return labels
    End Function

    Private Function getDataLambda(dimNames As String(), tag As String(), data As Object, env As Environment,
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

                    vectors(name) = REnv.asVector(Of Double)(.columns(name))
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
            .svmType = svmType,
            .kernelType = kernelType,
            .c = C,
            .cacheSize = cacheSize,
            .coefficient0 = coefficient0,
            .degree = degree,
            .EPS = EPS,
            .gamma = gamma,
            .nu = nu,
            .P = P,
            .probability = probability,
            .shrinking = shrinking
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
                    Call param.weights.Add(CInt(label.Key), label.Value)
                Next
            End With
        Else
            If TypeOf problem Is Problem Then
                For Each label As ColorClass In DirectCast(problem, Problem) _
                    .Y _
                    .GroupBy(Function(a) a.name) _
                    .Select(Function(a) a.First)

                    Call param.weights.Add(label.enumInt, 1)
                Next
            End If
        End If

        Logging.IsVerbose = verbose

        If TypeOf problem Is Problem Then
            Return getSvmModel(DirectCast(problem, Problem), param)
        Else
            Return getSvmModel(DirectCast(problem, ProblemTable), param)
        End If
    End Function

    Private Function getSvmModel(table As ProblemTable, param As Parameter) As SVMMultipleSet
        Dim result As New Dictionary(Of String, SVMModel)
        Dim allocated = table _
            .GetTopics _
            .Select(Function(topic) table.packCache(topic, param)) _
            .AsParallel _
            .Select(Function(subTopic)
                        Dim model As SVMModel = subTopic.topicProblem.getSvmModel(subTopic.args)
                        Return (model, subTopic.topic)
                    End Function)

        For Each topic As (Model As SVMModel, topic$) In allocated
            Call $"trainSVMModel::{topic.topic}".__INFO_ECHO
            result(topic.topic) = topic.Model
        Next

        Return New SVMMultipleSet With {
            .dimensionNames = table.dimensionNames,
            .topics = result
        }
    End Function

    <Extension>
    Private Function packCache(table As ProblemTable, topic As String, param As Parameter) As (args As Parameter, topicProblem As Problem, topic As String)
        Dim topicProblem As Problem = table.GetProblem(topic)
        Dim args As Parameter = param.Clone

        For Each label As ColorClass In DirectCast(topicProblem, Problem) _
            .Y _
            .GroupBy(Function(a) a.name) _
            .Select(Function(a) a.First)

            ' 因为会被反复使用，所以可能会出现重名的问题
            ' 在这里直接设置
            args.weights(key:=label.enumInt) = 1
        Next

        Return (args, topicProblem, topic)
    End Function

    <Extension>
    Private Function getSvmModel(problem As Problem, par As Parameter) As SVMModel
        Dim transform As RangeTransform = RangeTransform.Compute(problem)
        Dim scale = transform.Scale(problem)
        Dim model As SVM.Model = Training.Train(scale, par)

        Call Logging.flush()

        Return New SVMModel With {
            .transform = transform,
            .model = model,
            .factors = New ClassEncoder(problem.Y)
        }
    End Function

    <ExportAPI("parse.SVM_json")>
    <RApiReturn(GetType(SVMModel), GetType(SVMMultipleSet))>
    Public Function parseSVMJSON(x As Object, Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Internal.debug.stop("the required json value can not be nothing!", env)
        End If

        Dim jsonObj As JsonObject

        If TypeOf x Is String Then
            jsonObj = New json.JsonParser().OpenJSON(x)
        ElseIf TypeOf x Is JsonObject Then
            jsonObj = x
        Else
            Return Message.InCompatibleType(GetType(JsonObject), x.GetType, env)
        End If

        If jsonObj.Score(GetType(SVMMultipleSetJSON)) > jsonObj.Score(GetType(SvmModelJSON)) Then
            Return jsonObj.CreateObject(Of SVMMultipleSetJSON)(decodeMetachar:=True).CreateSVMModel
        Else
            Return jsonObj.CreateObject(Of SvmModelJSON)(decodeMetachar:=True).CreateSVMModel
        End If
    End Function

    ''' <summary>
    ''' serialize the SVM model as json string for save to a file
    ''' </summary>
    ''' <param name="svm"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("svm_json")>
    <RApiReturn(GetType(String))>
    Public Function SVMJSON(svm As Object,
                            Optional fileModel As Boolean = False,
                            Optional env As Environment = Nothing) As Object

        If svm Is Nothing Then
            Return "null"
        ElseIf TypeOf svm Is SVMModel Then
            Dim file = SvmModelJSON.CreateJSONModel(DirectCast(svm, SVMModel))

            If fileModel Then
                Return file
            Else
                Return file.GetJson
            End If
        ElseIf TypeOf svm Is SVMMultipleSet Then
            Dim file = SVMMultipleSetJSON.CreateJSONModel(DirectCast(svm, SVMMultipleSet))

            If fileModel Then
                Return file
            Else
                Return file.GetJson
            End If
        ElseIf TypeOf svm Is ProblemTable Then
            Return DirectCast(svm, ProblemTable).GetJson
        Else
            Return Message.InCompatibleType(GetType(SVMModel), svm.GetType, env)
        End If
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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="svm"></param>
    ''' <param name="validateSet"></param>
    ''' <param name="labels"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' <see cref="PerformanceEvaluator"/>
    ''' </returns>
    Private Function validateSingleSvmModel(svm As SVMModel, validateSet As Object, labels As Object, env As Environment) As Object
        Dim result As Object = DirectCast(svm, SVMModel).svmClassify1(validateSet, env)
        Dim labelsList As String() = REnv.asVector(Of String)(labels)

        If Program.isException(result) Then
            Return result
        End If

        Dim classifyResult As list = DirectCast(result, list)
        Dim keys As String() = classifyResult.slots.Keys.ToArray
        Dim points As New List(Of RankPair)

        For i As Integer = 0 To keys.Length - 1
            Dim p As ColorClass = classifyResult.slots(keys(i))
            Dim validate = labelsList(i)

            If p.name <> validate Then
                points.Add(New RankPair(0, 0))
            Else
                points.Add(New RankPair(1, 1))
            End If
        Next

        Return New PerformanceEvaluator(points)
    End Function

    Private Function validateMultipleSvmModel(svm As SVMMultipleSet, validateSet As Object, labels As Object, env As Environment) As Object
        Dim result As Object = DirectCast(svm, SVMMultipleSet).svmClassify2(validateSet, env)

        If Program.isException(result) Then
            Return result
        End If

        Dim classifyResult As EntityObject() = DirectCast(result, EntityObject())
        Dim validates As dataframe = DirectCast(labels, dataframe)
        Dim resultList As New Dictionary(Of String, Object)

        For Each dimension As String In validates.columns.Keys
            Dim validateVector As String() = REnv.asVector(Of String)(validates.columns(dimension))
            Dim points As New List(Of RankPair)

            For i As Integer = 0 To classifyResult.Length - 1
                Dim p As String = classifyResult(i)(dimension)
                Dim validate As String = validateVector(i)

                If p <> validate Then
                    points.Add(New RankPair(1, 0))
                Else
                    points.Add(New RankPair(1, 1))
                End If
            Next

            resultList.Add(dimension, New PerformanceEvaluator(points))
        Next

        Return New list With {.slots = resultList}
    End Function

    ''' <summary>
    ''' SVM model validation
    ''' </summary>
    ''' <param name="svm">a trained SVM model</param>
    ''' <param name="validateSet">
    ''' a dataframe object which contains the validate set data, 
    ''' each column should be exists in the dimensin name of 
    ''' the trainingSet.
    ''' </param>
    ''' <param name="labels">a dataframe object which contains 
    ''' the classify label result corresponding to the input 
    ''' ``validateSet`` rows.</param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' ``PerformanceEvaluator`` dataset for draw a ROC curve.
    ''' </returns>
    <ExportAPI("svm_validates")>
    Public Function svmValidates(svm As Object, validateSet As Object, <RRawVectorArgument> labels As Object, Optional env As Environment = Nothing) As Object
        If svm Is Nothing Then
            Return Internal.debug.stop("the required svm model can not be nothing!", env)
        ElseIf TypeOf svm Is SVMModel Then
            Return validateSingleSvmModel(DirectCast(svm, SVMModel), validateSet, labels, env)
        ElseIf TypeOf svm Is SVMMultipleSet Then
            Return validateMultipleSvmModel(DirectCast(svm, SVMMultipleSet), validateSet, labels, env)
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

            result += New EntityObject With {.ID = uid, .Properties = info}
        Next

        Return result.ToArray
    End Function

    <Extension>
    Private Function svmClassify1(svm As SVMModel, data As Object, env As Environment) As Object
        Dim row As (label As String, data As Node())
        Dim n As Integer
        Dim err As Message = Nothing
        Dim getData = SVMkit.getDataLambda(svm.dimensionNames, {"n/a"}, data, env, err, n)
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
End Module
