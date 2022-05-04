#Region "Microsoft.VisualBasic::8ae2db3a995e56501ac84cbc3aa1b021, R-sharp\Library\R.math\math.vb"

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

'   Total Lines: 515
'    Code Lines: 345
' Comment Lines: 101
'   Blank Lines: 69
'     File Size: 20.72 KB


' Module math
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: asFormula, create_deSolve_DataFrame, DiffEntropy, getBinTable, Hist
'               lm, loess, predict, (+2 Overloads) RK4, sim
'               ssm, summaryFit
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.Calculus.ODESolver
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.Information
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
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
            Call env.Push(v.Name, REnv.asVector(Of Double)(v.Value), [readonly]:=False, mode:=TypeCodes.double)
            Call names.Add(v.Name, env.FindSymbol(v.Name, [inherits]:=False))
        Next

        For Each formula As DeclareLambdaFunction In system
            Dim lambda As Func(Of Double, Double) = formula.CreateLambda(Of Double, Double)(env)
            Dim name As String = formula.parameterNames(Scan0)
            Dim ref As Symbol = names(name)

            ref.SetValue(REnv.getFirst(y0.getByName(name)), env)
            solve = Function() lambda(CDbl(ref.value))
            vector(++i) = New NonlinearVar(solve) With {
                .Name = name,
                .Value = REnv.getFirst(y0.getByName(.Name))
            }
        Next

        Dim df = Sub(dx#, ByRef dy As stdVec)
                     For Each x As NonlinearVar In vector
                         dy(x) = x.Evaluate()
                         names(x.Name).SetValue(x.Value, env)
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
    ''' <param name="data">A numeric vector data sequence</param>
    ''' <param name="step">The width of the bin box.</param>
    ''' <returns></returns>
    <ExportAPI("hist")>
    Public Function Hist(<RRawVectorArgument> data As Object, Optional step! = 1) As DataBinBox(Of Double)()
        Return DirectCast(REnv.asVector(Of Double)(data), Double()) _
            .Hist([step]) _
            .ToArray
    End Function

    <ExportAPI("gini")>
    Public Function Gini(<RRawVectorArgument> data As Object) As Double
        Dim raw As Double() = DirectCast(REnv.asVector(Of Double)(data), Double())
        Dim prob As Double() = raw.AsVector / raw.Max

        Return prob.Gini
    End Function

    ''' <summary>
    ''' loess fit
    ''' </summary>
    ''' <param name="formula"></param>
    ''' <param name="data"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("loess")>
    Public Function loess(formula As FormulaExpression, data As Object, Optional env As Environment = Nothing) As Object

    End Function

    ''' <summary>
    ''' ### Fitting Linear Models
    ''' 
    ''' do linear modelling, lm is used to fit linear models. It can be used to carry out regression, 
    ''' single stratum analysis of variance and analysis of covariance (although aov may provide a 
    ''' more convenient interface for these).
    ''' </summary>
    ''' <param name="formula">a formula expression of the target expression</param>
    ''' <param name="data">A dataframe for provides the data source for doing the linear modelling.</param>
    ''' <param name="weights">
    ''' A numeric vector for provides weight value for the points in the linear modelling processing.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("lm")>
    Public Function lm(formula As FormulaExpression, data As Object,
                       <RRawVectorArgument>
                       Optional weights As Object = Nothing,
                       Optional env As Environment = Nothing) As Object

        Dim df As Rdataframe

        If data Is Nothing Then
            Return Internal.debug.stop({"the required data can not be nothing!"}, env)
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
            w = REnv.asVector(Of Double)(weights)
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
                {lm.variables(Scan0), REnv.asVector(Of Double)(x)}
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
                input(name) = REnv.asVector(Of Double)(DirectCast(x, list).getByName(name))
            Next

            names = Nothing
        ElseIf TypeOf x Is Rdataframe Then
            For Each name As String In lm.variables
                input(name) = REnv.asVector(Of Double)(DirectCast(x, Rdataframe).getColumnVector(name))
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
    <ExportAPI("dot_product")>
    Public Function ssm(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object) As Double
        Dim vx As Double() = DirectCast(REnv.asVector(Of Double)(x), Double())
        Dim vy As Double() = DirectCast(REnv.asVector(Of Double)(y), Double())

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

End Module
