#Region "Microsoft.VisualBasic::6f7e94ca63a9b745d35791fde095993f, D:/GCModeller/src/R-sharp/Library/Rlapack//math.vb"

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

    '   Total Lines: 783
    '    Code Lines: 487
    ' Comment Lines: 201
    '   Blank Lines: 95
    '     File Size: 31.67 KB


    ' Module math
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: asFormula, asLmcall, binomial, create_deSolve_DataFrame, DiffEntropy
    '               getBinTable, getMax, getMin, Gini, glm
    '               Hist, lm, loess, predict, RamerDouglasPeucker
    '               (+2 Overloads) RK4, sim, ssm, summaryFit
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.Calculus.ODESolver
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.Information
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports baseMath = Microsoft.VisualBasic.Math
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports stdVec = Microsoft.VisualBasic.Math.LinearAlgebra.Vector
Imports vector = SMRUCC.Rsharp.Runtime.Internal.Object.vector

''' <summary>
''' the R# math module
''' </summary>
<Package("math", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@live.com")>
<RTypeExport("yfit", GetType(WeightedFit))>
Module math

    Sub New()
        REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(ODEsOut), AddressOf create_deSolve_DataFrame)
        REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(DataBinBox(Of Double)()), AddressOf getBinTable)

        REnv.Internal.generic.add("summary", GetType(lmCall), AddressOf summaryFit)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of lmCall)(Function(o) o.ToString)
    End Sub

    <Extension>
    Private Function getMin(bin As DataBinBox(Of Double)) As Double
        If bin.Count = 0 Then
            Return bin.Boundary.Min
        Else
            Return bin.Raw.Min
        End If
    End Function

    <Extension>
    Private Function getMax(bin As DataBinBox(Of Double)) As Double
        If bin.Count = 0 Then
            Return bin.Boundary.Max
        Else
            Return bin.Raw.Max
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="hist"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' a dataframe table with column names: min, max, bin_size
    ''' </returns>
    Private Function getBinTable(hist As DataBinBox(Of Double)(), args As list, env As Environment) As Rdataframe
        Dim format As String = args.getValue("format", env, "F2")
        Dim range As String() = hist _
            .Select(Function(bin)
                        Return $"{bin.getMin.ToString(format)} ~ {bin.getMax.ToString(format)}"
                    End Function) _
            .ToArray
        Dim min As String() = hist.Select(Function(bin) bin.getMin.ToString(format)).ToArray
        Dim max As String() = hist.Select(Function(bin) bin.getMax.ToString(format)).ToArray
        Dim count As Integer() = hist.Select(Function(bin) bin.Count).ToArray

        Return New Rdataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"min", min},
                {"max", max},
                {"bin_size", count}
            },
            .rownames = range
        }
    End Function

    Private Function summaryFit(x As lmCall, args As list, env As Environment) As Object
        Throw New NotImplementedException
    End Function

    Private Function create_deSolve_DataFrame(x As ODEsOut, args As list, env As Environment) As Rdataframe
        Dim data As New Rdataframe With {
            .columns = New Dictionary(Of String, Array)
        }
        Dim dx As String() = x.x _
            .Select(Function(d) CStr(d)) _
            .ToArray

        If args.hasName("x.lab") Then
            data.columns.Add(args("x.lab"), dx)
        End If

        For Each v In x.y
            data.columns.Add(v.Key, v.Value.ToArray)
        Next

        data.rownames = dx

        Return data
    End Function

    ''' <summary>
    ''' measure similarity between two data vector via entropy difference
    ''' </summary>
    ''' <param name="x">a numeric vector</param>
    ''' <param name="y">another numeric vector which should 
    ''' be in the same size as the <paramref name="x"/> vector.
    ''' </param>
    ''' <returns>Unweighted entropy similarity</returns>
    ''' 
    <ExportAPI("diff_entropy")>
    Public Function DiffEntropy(x As Double(), y As Double()) As Double
        Dim v1 = x.SeqIterator.Where(Function(i) i.value > 0).ToDictionary(Function(i) i.i.ToString, Function(i) i.value)
        Dim v2 = y.SeqIterator.Where(Function(i) i.value > 0).ToDictionary(Function(i) i.i.ToString, Function(i) i.value)

        Return v1.DiffEntropy(v2)
    End Function

    <ExportAPI("solve.RK4")>
    <RApiReturn(GetType(ODEOutput))>
    Public Function RK4(df As Object,
                        Optional y0# = 0,
                        Optional min# = -100,
                        Optional max# = 100,
                        Optional resolution% = 10000,
                        Optional env As Environment = Nothing) As Object
        Dim y As df

        If df Is Nothing Then
            Return REnv.Internal.debug.stop("Missing ``dy/dt``!", env)
        End If

        If TypeOf df Is df Then
            y = DirectCast(df, df)
        ElseIf TypeOf df Is DeclareLambdaFunction Then
            With DirectCast(df, DeclareLambdaFunction).CreateLambda(Of Double, Double)(env)
                y = Function(x, x1) .Invoke(x)
            End With
        Else
            Return REnv.Internal.debug.stop(New NotSupportedException, env)
        End If

        Return New ODE With {
            .df = y,
            .y0 = y0,
            .ID = df.ToString
        }.RK4(resolution, min, max)
    End Function

    ''' <summary>
    ''' solve a given ODE system
    ''' </summary>
    ''' <param name="system">a collection of the lambda expression</param>
    ''' <param name="y0">a list of the initialize values of the variables</param>
    ''' <param name="a">from</param>
    ''' <param name="b">to</param>
    ''' <param name="resolution%"></param>
    ''' <param name="tick">handler after each solver iteration in the solver loop</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("deSolve")>
    <RApiReturn(GetType(ODEsOut))>
    Public Function RK4(system As DeclareLambdaFunction(), y0 As list, a#, b#,
                        Optional resolution% = 10000,
                        Optional tick As Action(Of Environment) = Nothing,
                        Optional env As Environment = Nothing) As Object

        Dim vector As NonlinearVar() = New NonlinearVar(system.Length - 1) {}
        Dim i As i32 = Scan0
        Dim solve As Func(Of Double)
        Dim names As New Dictionary(Of String, Symbol)

        For Each v As NamedValue(Of Object) In y0.namedValues
            Call env.Push(v.Name, CLRVector.asNumeric(v.Value), [readonly]:=False, mode:=TypeCodes.double)
            Call names.Add(v.Name, env.FindSymbol(v.Name, [inherits]:=False))
        Next

        For Each formula As DeclareLambdaFunction In system
            Dim lambda As Func(Of Double, Double) = formula.CreateLambda(Of Double, Double)(env)
            Dim name As String = formula.parameterNames(Scan0)
            Dim ref As Symbol = names(name)

            ref.setValue(REnv.getFirst(y0.getByName(name)), env)
            solve = Function() lambda(CDbl(ref.value))
            vector(++i) = New NonlinearVar(solve) With {
                .Name = name,
                .Value = REnv.getFirst(y0.getByName(.Name))
            }
        Next

        Dim df = Sub(dx#, ByRef dy As stdVec)
                     For Each x As NonlinearVar In vector
                         dy(x) = x.Evaluate()
                         names(x.Name).setValue(x.Value, env)
                     Next
                 End Sub
        Dim ODEs As New GenericODEs(vector, df)
        Dim result As Object

        If tick Is Nothing Then
            result = ODEs.Solve(n:=resolution, a:=a, b:=b)
        Else
            result = New SolverIterator(New RungeKutta4(ODEs)) _
                .Config(ODEs.GetY0(False), resolution, a, b) _
                .Bind(Sub()
                          Call tick(env)
                      End Sub)
        End If

        Return result
    End Function

    ''' <summary>
    ''' Do fixed width cut bins
    ''' </summary>
    ''' <param name="x">A numeric vector data sequence</param>
    ''' <param name="step">The width of the bin box.</param>
    ''' <returns></returns>
    <ExportAPI("hist")>
    <RApiReturn(GetType(DataBinBox(Of Double)))>
    Public Function Hist(<RRawVectorArgument>
                         x As Object,
                         Optional step!? = Nothing,
                         Optional n%? = 10,
                         Optional env As Environment = Nothing) As Object

        Dim v As Double() = CLRVector.asNumeric(x)

        If [step] Is Nothing Then
            If n Is Nothing Then
                Return Internal.debug.stop("the historgram parameter step and n should not be both nothing!", env)
            End If

            [step] = New DoubleRange(v).Length / n
        End If

        Return v _
            .Hist([step]) _
            .ToArray
    End Function

    <ExportAPI("gini")>
    Public Function Gini(<RRawVectorArgument> data As Object) As Double
        Dim raw As Double() = CLRVector.asNumeric(data)
        Dim prob As Double() = raw.AsVector / raw.Max

        Return prob.Gini
    End Function

    ''' <summary>
    ''' ### Non-Parametric Bootstrapping
    ''' 
    ''' See Efron and Tibshirani (1993) for details on this function.
    ''' </summary>
    ''' <param name="x">a vector or a dataframe that containing the data. 
    ''' To bootstrap more complex data structures (e.g. bivariate data) 
    ''' see the last example below.</param>
    ''' <param name="nboot">The number of bootstrap samples desired.</param>
    ''' <param name="theta">function to be bootstrapped. Takes x as an 
    ''' argument, and may take additional arguments (see below and last 
    ''' example).</param>
    ''' <param name="func">(optional) argument specifying the functional 
    ''' the distribution of thetahat that is desired. If func is 
    ''' specified, the jackknife after-bootstrap estimate of its standard
    ''' error is also returned. See example below.</param>
    ''' <param name="args">any additional arguments to be passed to theta</param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' list with the following components:
    '''
    ''' 1. thetastar       the nboot bootstrap values Of theta
    ''' 2. func.thetastar  the functional func Of the bootstrap distribution Of thetastar, If func was specified
    ''' 3. jack.boot.val   the jackknife-after-bootstrap values For func, If func was specified
    ''' 4. jack.boot.se    the jackknife-after-bootstrap standard Error estimate Of func, If func was specified
    ''' 5. call            the deparsed call
    ''' 
    ''' and this function will returns the bootstrap data collection if the
    ''' <paramref name="theta"/> function is not specificed.
    ''' </returns>
    ''' <remarks>
    ''' Efron, B. and Tibshirani, R. (1986). The bootstrap method for 
    ''' standard errors, confidence intervals, and other measures of 
    ''' statistical accuracy. Statistical Science, Vol 1., No. 1, pp 1-35.
    ''' 
    ''' Efron, B. (1992) Jackknife-after-bootstrap standard errors And 
    ''' influence functions. J. Roy. Stat. Soc. B, vol 54, pages 83-127
    ''' 
    ''' Efron, B. And Tibshirani, R. (1993) An Introduction to the 
    ''' Bootstrap. Chapman And Hall, New York, London.
    ''' </remarks>
    <ExportAPI("bootstrap")>
    <RApiReturn("thetastar", "func.thetastar", "jack.boot.val", "jack.boot.se", "call")>
    Public Function bootstrap(<RRawVectorArgument>
                              x As Object,
                              nboot As Integer,
                              Optional theta As Object = Nothing,
                              Optional func As Object = Nothing,
                              <RListObjectArgument>
                              Optional args As list = Nothing,
                              Optional env As Environment = Nothing) As Object

        Dim thetaFunc As RFunction
        Dim check = applys.checkInternal(Nothing, theta, env)

        If TypeOf check Is Message Then
            Return check
        Else
            thetaFunc = theta
        End If

        Dim thetastar As New List(Of Object)

        If x Is Nothing Then
            Return Nothing
        End If

        Dim str_x As String = x.ToString

        If TypeOf x Is Rdataframe Then
            Dim df = DirectCast(x, Rdataframe)
            Dim colnames As String() = df.colnames
            Dim rows = df.forEachRow(colnames).ToArray
            Dim boots = rows.Samples(rows.Length, nboot, replace:=True).ToArray
            Dim result As Object

            For Each sample As SeqValue(Of NamedCollection(Of Object)()) In boots
                df = reshape2.ConstructDataframe(sample.value, colnames)
                result = thetaFunc.Invoke({df}, env)

                If Program.isException(result) Then
                    Return result
                Else
                    thetastar.Add(result)
                End If
            Next
        Else
            Dim arr As Array = REnv.asVector(Of Object)(x)
            Dim boots = arr.AsObjectEnumerator _
                .Samples(arr.Length, nboot, replace:=True) _
                .ToArray

            If thetastar Is Nothing Then
                Return New list With {
                    .slots = boots _
                        .ToDictionary(Function(i) (i.i + 1).ToString,
                                      Function(i)
                                          Return CObj(i.value)
                                      End Function)}
            Else
                Dim result As Object

                For Each sample As SeqValue(Of Object()) In boots
                    result = thetaFunc.Invoke({sample}, env)

                    If Program.isException(result) Then
                        Return result
                    Else
                        thetastar.Add(result)
                    End If
                Next
            End If
        End If

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"thetastar", thetastar.ToArray},
                {"func.thetastar", Nothing},
                {"jack.boot.val", Nothing},
                {"jack.boot.se", Nothing},
                {"call", $"bootstrap(x = {str_x}, nboot = {nboot}, 
theta = {objToString(thetaFunc, env:=env)}
);"}
            }
        }
    End Function

    ''' <summary>
    ''' loess fit
    ''' </summary>
    <ExportAPI("loess")>
    Public Function loess(formula As FormulaExpression, data As Object, Optional env As Environment = Nothing) As Object
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' ## Fitting Generalized Linear Models
    ''' 
    ''' glm is used to fit generalized linear models, specified by 
    ''' giving a symbolic description of the linear predictor and
    ''' a description of the error distribution.
    ''' </summary>
    ''' <param name="formula">
    ''' an object of class "formula" (Or one that can be coerced to
    ''' that class): a symbolic description Of the model To be fitted.
    ''' The details Of model specification are given under 'Details’.
    ''' </param>
    ''' <param name="family">
    ''' a description of the error distribution and link function to 
    ''' be used in the model. For glm this can be a character string 
    ''' naming a family function, a family function or the result of 
    ''' a call to a family function. For glm.fit only the third option 
    ''' is supported. (See family for details of family functions.)
    ''' </param>
    ''' <param name="data">
    ''' an optional data frame, list or environment (or object coercible
    ''' by as.data.frame to a data frame) containing the variables in 
    ''' the model. If not found in data, the variables are taken from 
    ''' environment(formula), typically the environment from which glm 
    ''' is called.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("glm")>
    Public Function glm(formula As FormulaExpression,
                        family As Object,
                        data As Object,
                        Optional env As Environment = Nothing) As Object

        Dim df As Rdataframe = DirectCast(data, Rdataframe)
        Dim y As Double() = df.getVector(Of Double)(formula.var)
        Dim println As Action(Of String) = Nothing

        If env.globalEnvironment.options.verbose Then
            Dim cat = env.WriteLineHandler
            println = Sub(msg) Call cat(msg)
        End If

        If TypeOf family Is Logistic AndAlso TypeOf data Is Rdataframe Then
            Dim symbol As Object = formula.GetSymbols(env)

            If TypeOf symbol Is Message Then
                Return symbol
            End If

            Dim columns As New List(Of Double())

            For Each colName As String In DirectCast(symbol, String())
                Call columns.Add(df.getVector(Of Double)(colName))
            Next

            Dim matrix As New List(Of Instance)

            For i As Integer = 0 To y.Length - 1
#Disable Warning
                Call matrix.Add(New Instance(label:=y(i), x:=columns.Select(Function(c) c(i)).ToArray))
#Enable Warning
            Next

            Dim log As Logistic = family
            Dim logfit = New Logistic(columns.Count, log.ALPHA, println) With {.ITERATIONS = log.ITERATIONS}.train(matrix)
            Dim lm As New lmCall(formula.var, DirectCast(symbol, String())) With {
                .formula = formula,
                .lm = logfit,
                .data = df.ToString
            }

            Return lm
        Else
            Return Internal.debug.stop(New NotImplementedException, env)
        End If
    End Function

    ''' <summary>
    ''' ## Family Objects for Models
    ''' 
    ''' Family objects provide a convenient way to specify the 
    ''' details of the models used by functions such as glm. 
    ''' See the documentation for glm for the details on how 
    ''' such model fitting takes place.
    ''' </summary>
    ''' <param name="link">
    ''' a specification For the model link Function. This can be
    ''' a name/expression, a literal character String, a length-one
    ''' character vector, Or an Object Of Class "link-glm" (such 
    ''' As generated by make.link) provided it Is Not specified 
    ''' via one Of the standard names given Next.
    ''' 
    ''' The gaussian family accepts the links (As names) identity, 
    ''' log And inverse; the binomial family the links logit, probit,
    ''' cauchit, (corresponding To logistic, normal And Cauchy 
    ''' CDFs respectively) log And cloglog (complementary log-log); 
    ''' the Gamma family the links inverse, identity And log; the 
    ''' poisson family the links log, identity, And sqrt; And the 
    ''' inverse.gaussian family the links 1/mu^2, inverse, identity 
    ''' And log.
    ''' 
    ''' The quasi family accepts the links logit, probit, cloglog, 
    ''' identity, inverse, log, 1/mu^2 And sqrt, And the Function
    ''' power can be used To create a power link Function.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("binomial")>
    Public Function binomial(Optional link As String = "logit",
                             <RListObjectArgument>
                             Optional args As list = Nothing,
                             Optional env As Environment = Nothing) As Object

        Select Case LCase(link)
            Case "logit"
                Dim learnRate As Double = args.getValue("rate", env, [default]:=0.01)
                Dim iteration As Integer = args.getValue("iteration", env, [default]:=500)

                Return New Logistic() With {
                    .ALPHA = learnRate,
                    .ITERATIONS = iteration
                }
            Case Else
                Return Internal.debug.stop(New NotImplementedException(link), env)
        End Select
    End Function

    ''' <summary>
    ''' cast the list data dump from the R ``lm`` result
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("as.lm_call")>
    Public Function asLmcall(x As list) As lmCall
        Return source.fromRlmSource(source:=x)
    End Function

    ''' <summary>
    ''' ### Fitting Linear Models
    ''' 
    ''' do linear modelling, lm is used to fit linear models. It can be used 
    ''' to carry out regression, single stratum analysis of variance and 
    ''' analysis of covariance (although aov may provide a more convenient 
    ''' interface for these).
    ''' </summary>
    ''' <param name="formula">
    ''' a formula expression of the target expression
    ''' </param>
    ''' <param name="data">
    ''' A dataframe for provides the data source for doing the linear modelling.
    ''' </param>
    ''' <param name="weights">
    ''' A numeric vector for provides weight value for the points 
    ''' in the linear modelling processing.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("lm")>
    <RApiReturn(GetType(lmCall))>
    Public Function lm(formula As FormulaExpression,
                       Optional data As Object = Nothing,
                       <RRawVectorArgument>
                       Optional weights As Object = Nothing,
                       Optional env As Environment = Nothing) As Object

        Dim df As Rdataframe = Nothing

        If data Is Nothing Then
            Dim symbol As Symbol = env.FindSymbol(formula.var)

            ' check variables in the environment
            If symbol IsNot Nothing Then
                Dim vars = SymbolAnalysis.GetSymbolReferenceList(formula.formula).ToArray

                df = New Rdataframe With {.columns = New Dictionary(Of String, Array)}
                df.add(formula.var, CLRVector.asNumeric(symbol.value))

                For Each v In vars
                    symbol = env.FindSymbol(v.Name)

                    If symbol Is Nothing Then
                        df = Nothing
                        Exit For
                    Else
                        df.add(v.Name, CLRVector.asNumeric(symbol.value))
                    End If
                Next
            End If

            If df Is Nothing Then
                Return Internal.debug.stop({"the required data can not be nothing!"}, env)
            End If
        ElseIf TypeOf data Is Rdataframe Then
            df = DirectCast(data, Rdataframe)

            If Not df.columns.ContainsKey(formula.var) Then
                Return Internal.debug.stop({
                    $"missing the required symbol '{formula.var}' in your input data!",
                    $"symbol: {formula.var}",
                    $"formula: {formula}"
                }, env)
            End If
        Else
            Return Message.InCompatibleType(GetType(Rdataframe), data.GetType, env)
        End If

        Dim w As Double()

        If Not base.isEmpty(weights) Then
            w = CLRVector.asNumeric(weights)
        Else
            w = Nothing
        End If

        Dim y As Double() = df.getVector(Of Double)(formula.var)

        If TypeOf formula.formula Is SymbolReference Then
            ' y ~ x
            Dim x_symbol As String = DirectCast(formula.formula, SymbolReference).symbol
            Dim x As Double() = df.getVector(Of Double)(x_symbol)

            If w.IsNullOrEmpty Then
                Return New lmCall(formula.var, {x_symbol}) With {
                    .formula = formula,
                    .lm = LeastSquares.LinearFit(x, y),
                    .data = df.ToString
                }
            Else
                Dim wStr As String = w.Select(Function(d) d.ToString("G3")).JoinBy(", ")

                If wStr.Length > 32 Then
                    wStr = Mid(wStr, 1, 32) & "..."
                End If

                Return New lmCall(formula.var, {x_symbol}) With {
                    .formula = formula,
                    .lm = WeightedLinearRegression.Regress(x, y, w, orderOfPolynomial:=1),
                    .data = df.ToString,
                    .weights = $"[{wStr}]"
                }
            End If
        Else
            Dim symbol As Object = formula.GetSymbols(env)

            If TypeOf symbol Is Message Then
                Return symbol
            End If

            Dim columns As New List(Of Double())

            For Each colName As String In DirectCast(symbol, String())
                Call columns.Add(df.getVector(Of Double)(colName))
            Next

            Dim matrix As New NumericMatrix(columns.ToArray, t:=True)
            Dim fit As MLRFit = MLRFit.LinearFitting(matrix, f:=y)

            Return New lmCall(formula.var, DirectCast(symbol, String())) With {
                .formula = formula,
                .lm = fit,
                .data = df.ToString
            }
        End If
    End Function

    ''' <summary>
    ''' create a lambda function based on the ``lm`` result.
    ''' </summary>
    ''' <param name="lm"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.formula")>
    <RApiReturn(GetType(DeclareLambdaFunction))>
    Public Function asFormula(lm As lmCall, Optional env As Environment = Nothing) As Object
        If lm Is Nothing Then
            Return Internal.debug.stop("the required linear model can not be nothing!", env)
        End If

        ' 主要是生成lambda函数的closure表达式
        Dim name As String = lm.lm.Polynomial.ToString("G3")
        Dim trace As StackFrame = env.stackFrame
        Dim parameter As New DeclareNewSymbol(
            names:=lm.variables,
            value:=Nothing,
            type:=TypeCodes.double,
            [readonly]:=False,
            stackFrame:=trace
        )
        Dim closure As Expression = lm.CreateFormulaCall

        Return New DeclareLambdaFunction(name, parameter, closure, env.stackFrame)
    End Function

    ''' <summary>
    ''' ### Model Predictions
    ''' 
    ''' predict is a generic function for predictions from the results of 
    ''' various model fitting functions. The function invokes particular 
    ''' methods which depend on the class of the first argument.
    ''' </summary>
    ''' <param name="lm">a model Object For which prediction Is desired.</param>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' The form of the value returned by predict depends on the class of its argument. 
    ''' See the documentation of the particular methods for details of what is 
    ''' produced by that method.
    ''' </returns>
    ''' <remarks>
    ''' Most prediction methods which are similar to those for linear models 
    ''' have an argument newdata specifying the first place to look for 
    ''' explanatory variables to be used for prediction. Some considerable 
    ''' attempts are made to match up the columns in newdata to those used 
    ''' for fitting, for example that they are of comparable types and that 
    ''' any factors have the same level set in the same order (or can be 
    ''' transformed to be so).
    ''' 
    ''' Time series prediction methods In package stats have an argument 
    ''' n.ahead specifying how many time steps ahead To predict.
    ''' 
    ''' Many methods have a logical argument se.fit saying If standard errors 
    ''' are To returned.
    ''' </remarks>
    <ExportAPI("predict")>
    Public Function predict(lm As lmCall,
                            <RRawVectorArgument>
                            x As Object,
                            Optional env As Environment = Nothing) As Object

        If lm Is Nothing Then
            Return Internal.debug.stop("the required model object can not be nothing!", env)
        End If

        Dim input As New Dictionary(Of String, Double())
        Dim names As String() = Nothing

        If x Is Nothing Then
            Return Internal.debug.stop("the required input data x can not be nothing!", env)
        ElseIf TypeOf x Is Integer OrElse TypeOf x Is Long OrElse TypeOf x Is Single OrElse TypeOf x Is Double Then
            Return lm.lm.GetY(CDbl(x))
        ElseIf TypeOf x Is Double() OrElse TypeOf x Is vector Then
            input = New Dictionary(Of String, Double()) From {
                {lm.variables(Scan0), CLRVector.asNumeric(x)}
            }

            If TypeOf x Is Double() Then
                names = Nothing
            ElseIf TypeOf x Is vector Then
                names = DirectCast(x, vector).getNames
            Else
                Throw New Exception("this exception will never happends!")
            End If
        ElseIf TypeOf x Is list Then
            For Each name As String In lm.variables
                input(name) = CLRVector.asNumeric(DirectCast(x, list).getByName(name))
            Next

            names = Nothing
        ElseIf TypeOf x Is Rdataframe Then
            For Each name As String In lm.variables
                input(name) = CLRVector.asNumeric(DirectCast(x, Rdataframe).getColumnVector(name))
            Next

            names = DirectCast(x, Rdataframe).rownames
        Else
            Return Message.InCompatibleType(GetType(Rdataframe), x.GetType, env)
        End If

        Dim result As New List(Of Double)

        With input.First.Value
            If Not input.All(Function(a) a.Value.Length = .Length) Then
                Return Internal.debug.stop("all of the data variable vector for predicts should be equals to each other in size!", env)
            End If

            For i As Integer = 0 To .Length - 1
#Disable Warning
                Dim xvec As Double() = lm.variables _
                    .Select(Function(name) input(name)(i)) _
                    .ToArray
#Enable Warning
                Call result.Add(lm.Predicts(xvec))
            Next
        End With

        Dim out As New vector(result.ToArray, RType.GetRSharpType(GetType(Double)))

        If names.IsNullOrEmpty Then
            Return out
        Else
            x = out.setNames(names, env)

            If TypeOf x Is Message Then
                Return x
            End If
        End If

        Return out
    End Function

    ''' <summary>
    ''' Evaluate cos similarity of two vector
    ''' 
    ''' the given vector x and y should be contains the elements in the same length.
    ''' </summary>
    ''' <param name="x">a numeric data sequence</param>
    ''' <param name="y">another numeric data sequence</param>
    ''' <returns></returns>
    <ExportAPI("cosine")>
    Public Function ssm(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object) As Double
        Dim vx As Double() = CLRVector.asNumeric(x)
        Dim vy As Double() = CLRVector.asNumeric(y)

        Return baseMath.SSM(vx.AsVector, vy.AsVector)
    End Function

    ''' <summary>
    ''' Create a similarity matrix
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("sim")>
    <RApiReturn(GetType(DistanceMatrix))>
    Public Function sim(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim dataset As DataSet()

        If TypeOf x Is Rdataframe Then
            Dim colnames As String() = DirectCast(x, Rdataframe).columns.Keys.ToArray

            dataset = DirectCast(x, Rdataframe) _
                .forEachRow() _
                .Select(Function(r)
                            Return New DataSet With {
                                .ID = r.name,
                                .Properties = colnames _
                                    .Select(Function(c, i) (c, r(i))) _
                                    .ToDictionary(Function(t) t.c,
                                                  Function(t)
                                                      Return Val(t.Item2)
                                                  End Function)
                            }
                        End Function) _
                .ToArray
        Else
            Dim dataPipe As pipeline = pipeline.TryCreatePipeline(Of DataSet)(x, env)

            If dataPipe.isError Then
                Return dataPipe.getError
            Else
                dataset = dataPipe.populates(Of DataSet)(env).ToArray
            End If
        End If

        Return dataset.Similarity
    End Function

    ''' <summary>
    ''' ### Ramer-Douglas-Peucker algorithm for curve fitting with a PolyLine
    ''' 
    ''' The [Ramer-Douglas-Peucker algorithm](https://en.wikipedia.org/wiki/Ramer-Douglas-Peucker_algorithm) 
    ''' for reducing the number of points on a curve.
    ''' 
    ''' If there are no more than two points it does not make sense to simplify.
    ''' In this case the input is returned without further checks of `x` and `y`.
    ''' In particular, the input is not checked for `NA` values.
    ''' 
    ''' ```r
    ''' RamerDouglasPeucker(x = c(0, 1, 3, 5), y = c(2, 1, 0, 1), epsilon = 0.5)
    ''' ```
    ''' </summary>
    ''' <param name="x">The `x` values of the curve as a vector without `NA` values.</param>
    ''' <param name="y">The `y` values of the curve as a vector without `NA` values.</param>
    ''' <param name="epsilon">The threshold for filtering outliers from the simplified curve. an number between 0 and 1. Recomended 0.01.</param>
    ''' <param name="method"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' A `data.frame` with `x` and `y` values of the simplified curve.
    ''' </returns>
    <ExportAPI("RamerDouglasPeucker")>
    Public Function RamerDouglasPeucker(x As Double(), y As Double(),
                                        Optional epsilon As Double = 0.1,
                                        <RRawVectorArgument(GetType(String))>
                                        Optional method As Object = "RDPsd|RDPppd",
                                        Optional env As Environment = Nothing) As Object

        If x Is Nothing OrElse y Is Nothing Then
            Return Internal.debug.stop("the given input of numeric vector x or y can not be nothing!", env)
        ElseIf x.Length <> y.Length Then
            Return Internal.debug.stop($"the given input vector size x({x.Length}) is not equals to y({y.Length})!", env)
        Else
            Dim methods As String() = CLRVector.asCharacter(method)
            Dim data As PointF() = x _
                .Select(Function(xi, i) New PointF(xi, y(i))) _
                .ToArray
            Dim pointList As PointF()

            Select Case methods(Scan0).ToLower
                Case "RDPsd" : pointList = data.RDPsd(epsilon)
                Case "RDPppd" : pointList = data.RDPppd(epsilon)
                Case Else
                    Return Internal.debug.stop($"unknow method: {methods(Scan0)}!", env)
            End Select

            Return New Rdataframe With {
                .columns = New Dictionary(Of String, Array) From {
                    {"x", pointList.X},
                    {"y", pointList.Y}
                }
            }
        End If
    End Function

    ''' <summary>
    ''' Use non-linear least squares to fit a function, f, to data.
    ''' 
    ''' Assumes ``ydata = f(xdata, args) + eps``.
    ''' </summary>
    ''' <param name="f">The model function, f(x, …). It must take the independent variable 
    ''' as the first argument and the parameters to fit as separate remaining arguments.
    ''' </param>
    ''' <param name="xdata">The independent variable where the data is measured. Should usually
    ''' be an M-length sequence or an (k,M)-shaped array for functions with k predictors, 
    ''' and each element should be float convertible if it is an array like object.</param>
    ''' <param name="ydata">The dependent data, a length M array - nominally f(xdata, ...).</param>
    ''' <param name="args">arguments to fit in the given function <paramref name="f"/>.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("curve_fit")>
    <RApiReturn(GetType(Double))>
    Public Function curve_fit(f As Object,
                              <RRawVectorArgument> xdata As Object,
                              <RRawVectorArgument> ydata As Object,
                              <RRawVectorArgument>
                              <RListObjectArgument> args As Object,
                              Optional env As Environment = Nothing) As Object

        If TypeOf f Is RFunction Then
            ' do nothing
        ElseIf TypeOf f Is MethodInfo Then
            f = New RMethodInfo(DirectCast(f, MethodInfo))
        Else
            Return Message.InCompatibleType(GetType(RFunction), f.GetType, env)
        End If

        Dim lambda As RFunction = DirectCast(f, RFunction)
        Dim x As Double() = CLRVector.asNumeric(xdata)
        Dim y As Double() = CLRVector.asNumeric(ydata)
        Dim xy As DataPoint() = x _
            .Select(Function(xi, i) New DataPoint(xi, y(i))) _
            .ToArray
        Dim fit As GaussNewtonSolver.FitFunction
        Dim b0 As Double()
        ' the first parameter is xi
        Dim pars = lambda.getArguments _
            .Skip(1) _
            .Select(Function(a) a.Name) _
            .ToArray

        If TypeOf args Is list Then
            ' is named vector
            Dim argList As list = args

            b0 = pars _
                .Select(Function(a)
                            Return argList.getValue(Of Double)(a, env, [default]:=0.0)
                        End Function) _
                .ToArray
        Else
            b0 = CLRVector.asNumeric(args)
        End If

        fit = Function(xi, par)
                  Dim a As Object() = New Object(b0.Length) {}
                  a(0) = xi
                  For i As Integer = 1 To b0.Length
                      a(i) = par(i, 0)
                  Next
                  Return REnv.single(CLRVector.asNumeric(lambda.Invoke(a, env)))
              End Function

        Dim gauss As New GaussNewtonSolver(fit)
        Dim result = gauss.Fit(xy, b0)

        If TypeOf args Is list Then
            Dim arguments As New list With {.slots = New Dictionary(Of String, Object)}

            For i As Integer = 0 To pars.Length - 1
                Call arguments.add(pars(i), result(i))
            Next

            Return arguments
        Else
            Return result
        End If
    End Function
End Module
