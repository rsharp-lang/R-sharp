﻿#Region "Microsoft.VisualBasic::6d129f585d23804339eebcbba61f0982, Library\Rlapack\stats.vb"

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

    '   Total Lines: 2297
    '    Code Lines: 1148 (49.98%)
    ' Comment Lines: 920 (40.05%)
    '    - Xml Docs: 87.93%
    ' 
    '   Blank Lines: 229 (9.97%)
    '     File Size: 101.72 KB


    ' Module stats
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: matrixDataFrame, matrixDataFrame2, printMatrix, printMvar, printTtest
    '               printTwoSampleTTest
    '     Enum p_adjust_methods
    ' 
    '         BH, bonferroni, BY, fdr, hochberg
    '         holm, hommel, none
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: aov, asDist, beta, chisq_test, ChiSquare
    '               cmdscale, combin, corr, corr_sign, corrTest
    '               dataframeRow, dist, dnorm, ECDF, ecdf0
    '               emd_dist, filterMissing, fisher_test, gamma, gammaCDF
    '               GetDataSetCommon, getLabels, getMatrix, getQuantileLevels, iqr_outliers
    '               kurtosis, lbeta, lgamma, Lowess, mantel_test
    '               median, moment, moran_test, mul, oplsr
    '               p_adjust, plsda, pnorm_func, PoissonDiskGenerator_func, (+2 Overloads) pow
    '               prcomp, ProductMoments, pt, quantile, safeCheck
    '               skewness, spline, tabulateMode, ttest, ttestBatch
    '               ttestImpl, varTest, z_score, z_scoreByColumn, z_scoreByRow
    ' 
    ' Enum SplineAlgorithms
    ' 
    '     Bezier, BSpline, CatmullRom, CubiSpline
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.MDSScale
Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Math.Statistics
Imports Microsoft.VisualBasic.Math.Statistics.Distributions
Imports Microsoft.VisualBasic.Math.Statistics.Distributions.MethodOfMoments
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.FishersExact
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports Microsoft.VisualBasic.Math.Statistics.MomentFunctions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports std = System.Math
Imports stdVector = Microsoft.VisualBasic.Math.LinearAlgebra.Vector
Imports vec = SMRUCC.Rsharp.Runtime.Internal.Object.vector

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Image = Microsoft.VisualBasic.Imaging.Image
#End If

''' <summary>
''' ### The R Stats Package 
''' 
''' R statistical functions, This package contains 
''' functions for statistical calculations and random 
''' number generation.
''' 
''' For a complete list of functions, use ``library(help = "stats")``.
''' </summary>
<Package("stats")>
Module stats

    Sub New()
        RInternal.ConsolePrinter.AttachConsoleFormatter(Of DistanceMatrix)(AddressOf printMatrix)
        RInternal.ConsolePrinter.AttachConsoleFormatter(Of TtestResult)(AddressOf printTtest)
        RInternal.ConsolePrinter.AttachConsoleFormatter(Of TwoSampleResult)(AddressOf printTwoSampleTTest)
        RInternal.ConsolePrinter.AttachConsoleFormatter(Of FishersExactPvalues)(Function(o) o.ToString)
        RInternal.ConsolePrinter.AttachConsoleFormatter(Of FTest)(Function(o) o.ToString)
        RInternal.ConsolePrinter.AttachConsoleFormatter(Of MultivariateAnalysisResult)(AddressOf printMvar)

        RInternal.Object.Converts.makeDataframe.addHandler(GetType(DataMatrix), AddressOf matrixDataFrame)
        RInternal.Object.Converts.makeDataframe.addHandler(GetType(DistanceMatrix), AddressOf matrixDataFrame)
        RInternal.Object.Converts.makeDataframe.addHandler(GetType(CorrelationMatrix), AddressOf matrixDataFrame2)
    End Sub

    Private Function printMvar(x As MultivariateAnalysisResult) As String
        Dim sb As New StringBuilder
        Dim text As New StringWriter(sb)

        If x.analysis Is Nothing Then
            Return "NULL"
        End If

        Select Case x.analysis
            Case GetType(PLS) : Call x.WritePlsResult(text)
            Case GetType(OPLS) : Call x.WriteOplsResult(text)
            Case GetType(PCA) : Call x.WritePcaResult(text)
            Case Else
                Return $"not implements for {x.analysis.Name}!"
        End Select

        Call text.Flush()

        Return sb.ToString
    End Function

    Private Function matrixDataFrame2(x As CorrelationMatrix, args As list, env As Environment) As Rdataframe
        Dim cutoff As Double = args.getValue("cutoff", env, [default]:=0.3)
        Dim pvalue_cut As Double = args.getValue("pvalue_cut", env, [default]:=0.05)
        Dim from As New List(Of String)
        Dim [to] As New List(Of String)
        Dim cor As New List(Of Double)
        Dim pvalue As New List(Of Double)

        For Each link In x.GetUniqueTuples
            Dim cori As Double = x.dist(link.a, link.b)
            Dim pvali As Double = x.pvalue(link.a, link.b)

            If std.Abs(cori) > cutoff AndAlso pvali < pvalue_cut Then
                Call from.Add(link.a)
                Call [to].Add(link.b)
                Call cor.Add(cori)
                Call pvalue.Add(pvali)
            End If
        Next

        Return New Rdataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"from", from.ToArray},
                {"to", [to].ToArray},
                {"cor", cor.ToArray},
                {"pvalue", pvalue.ToArray}
            }
        }
    End Function

    Private Function matrixDataFrame(x As DataMatrix, args As list, env As Environment) As Rdataframe
        Dim table As New Rdataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = x.keys
        }

        For Each row In x.PopulateRowObjects(Of DataSet)
            Call table.columns.Add(row.ID, row(table.rownames))
        Next

        Return table
    End Function

    Private Function printTtest(t As TtestResult) As String
        Return t.ToString
    End Function

    Private Function printTwoSampleTTest(t As TwoSampleResult) As String
        Return t.ToString
    End Function

    Private Function printMatrix(d As DistanceMatrix) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine($"Distance matrix of {d.keys.Length} objects:")
        Call sb.AppendLine(d.ToString)
        Call sb.AppendLine()

        For Each row In d.PopulateRowObjects(Of DataSet).Take(6)
            Call sb.AppendLine($"{row.ID}: {row.Properties.Take(8).Select(Function(t) $"{t.Key}:{t.Value.ToString("F2")}").JoinBy(", ")} ...")
        Next

        Call sb.AppendLine("...")

        Return sb.ToString
    End Function

    Public Enum p_adjust_methods
        holm
        hochberg
        hommel
        bonferroni
        BH
        BY
        fdr
        none
    End Enum

    ''' <summary>
    ''' calculates ``C(n, k)``.
    ''' </summary>
    ''' <param name="number">n</param>
    ''' <param name="number_chosen">k</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' https://en.wikipedia.org/wiki/Combination
    ''' </remarks>
    <ExportAPI("combin")>
    Public Function combin(number As Integer, number_chosen As Integer) As Double
        Return SpecialFunctions.Combination(number, number_chosen)
    End Function

    ''' <summary>
    ''' The Normal Distribution
    ''' 
    ''' Density, distribution function, quantile function and random generation 
    ''' for the normal distribution with mean equal to mean and standard deviation 
    ''' equal to sd.
    ''' </summary>
    ''' <param name="q">vector of quantiles.</param>
    ''' <param name="mean">vector of means.</param>
    ''' <param name="sd">vector of standard deviations.</param>
    ''' <param name="lower_tail">logical; if TRUE (default), probabilities are P[X \le x]P[X≤x] otherwise, P[X > x]P[X>x].</param>
    ''' <param name="log_p">logical; if TRUE, probabilities p are given as log(p).</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' For pnorm, based on
    ''' 
    ''' Cody, W. D. (1993) Algorithm 715 SPECFUN – A portable FORTRAN package of 
    ''' special function routines And test drivers. ACM Transactions on Mathematical 
    ''' Software 19, 22–32.
    ''' </remarks>
    <ExportAPI("pnorm")>
    Public Function pnorm_func(<RRawVectorArgument> q As Object,
                               Optional mean As Double = 0,
                               Optional sd As Double = 1,
                               Optional lower_tail As Boolean = True,
                               Optional log_p As Boolean = False,
                               Optional resolution As Integer = 97) As Object

        Return pnorm.eval(CLRVector.asNumeric(q).AsVector,
                          mean, sd, lower_tail, log_p,
                          resolution:=resolution).ToArray
    End Function

    <ExportAPI("dnorm")>
    Public Function dnorm(<RRawVectorArgument> x As Object, Optional mean As Double = 0, Optional sd As Double = 1) As Object
        Return pnorm.ProbabilityDensity(CLRVector.asNumeric(x).AsVector, mean, sd).ToArray
    End Function

    ''' <summary>
    ''' ### Adjust P-values for Multiple Comparisons
    ''' 
    ''' Given a set of p-values, returns p-values adjusted 
    ''' using one of several methods.
    ''' </summary>
    ''' <param name="p">
    ''' numeric vector Of p-values (possibly With NAs). Any other R Object Is 
    ''' coerced by As.numeric.
    ''' </param>
    ''' <param name="method">
    ''' correction method. Can be abbreviated.
    ''' </param>
    ''' <param name="n">
    ''' number of comparisons, must be at least length(p); only set this (to 
    ''' non-default) when you know what you are doing!
    ''' </param>
    ''' <param name="env"></param>
    ''' <remarks>
    ''' The adjustment methods include the Bonferroni correction ("bonferroni") 
    ''' in which the p-values are multiplied by the number of comparisons. Less
    ''' conservative corrections are also included by Holm (1979) ("holm"), 
    ''' Hochberg (1988) ("hochberg"), Hommel (1988) ("hommel"), Benjamini &amp; 
    ''' Hochberg (1995) ("BH" or its alias "fdr"), and Benjamini &amp; Yekutieli 
    ''' (2001) ("BY"), respectively. A pass-through option ("none") is also included.
    ''' The set of methods are contained in the p.adjust.methods vector for the 
    ''' benefit of methods that need to have the method as an option and pass it 
    ''' on to p.adjust.
    ''' 
    ''' The first four methods are designed To give strong control Of the family-wise 
    ''' Error rate. There seems no reason To use the unmodified Bonferroni correction 
    ''' because it Is dominated by Holm's method, which is also valid under arbitrary
    ''' assumptions.
    ''' 
    ''' Hochberg's and Hommel's methods are valid when the hypothesis tests are 
    ''' independent or when they are non-negatively associated (Sarkar, 1998; Sarkar 
    ''' and Chang, 1997). Hommel's method is more powerful than Hochberg's, but the 
    ''' difference is usually small and the Hochberg p-values are faster to compute.
    ''' The "BH" (aka "fdr") And "BY" method of Benjamini, Hochberg, And Yekutieli 
    ''' control the false discovery rate, the expected proportion of false discoveries
    ''' amongst the rejected hypotheses. The false discovery rate Is a less stringent 
    ''' condition than the family-wise error rate, so these methods are more powerful 
    ''' than the others.
    ''' 
    ''' Note that you can Set n larger than length(p) which means the unobserved
    ''' p-values are assumed To be greater than all the observed p For "bonferroni" 
    ''' And "holm" methods And equal To 1 For the other methods.
    ''' </remarks>
    ''' <returns>
    ''' A numeric vector of corrected p-values (of the same length as p, with names 
    ''' copied from p).
    ''' </returns>
    <ExportAPI("p.adjust")>
    <RApiReturn(GetType(Double))>
    Public Function p_adjust(p As Double(),
                             Optional method As p_adjust_methods = p_adjust_methods.fdr,
                             Optional n As Integer? = Nothing,
                             Optional env As Environment = Nothing) As Object
        Select Case method
            Case p_adjust_methods.fdr : Return p.FDR(n).ToArray
            Case Else
                Return RInternal.debug.stop(New NotImplementedException(method.Description), env)
        End Select
    End Function

    ''' <summary>
    ''' ## Empirical Cumulative Distribution Function
    ''' 
    ''' Compute an empirical cumulative distribution function, with several methods for 
    ''' plotting, printing and computing with such an “ecdf” object.
    ''' </summary>
    ''' <param name="x">
    ''' numeric vector of the observations for ecdf; for the methods, an object 
    ''' inheriting from class "ecdf".
    ''' </param>
    ''' <returns>
    ''' For ecdf, a function of class "ecdf", inheriting from the "stepfun" class,
    ''' and hence inheriting a knots() method.
    ''' </returns>
    ''' <remarks>
    ''' The objects of class "ecdf" are not intended to be used for permanent storage 
    ''' and may change structure between versions of R (and did at R 3.0.0). They 
    ''' can usually be re-created by
    '''
    ''' ```R
    ''' eval(attr(old_obj, "call"), environment(old_obj))
    ''' ```
    ''' 
    ''' since the data used Is stored As part Of the Object's environment.
    ''' </remarks>
    ''' 
    <ExportAPI("ecdf")>
    Public Function ecdf0(<RRawVectorArgument> x As Object) As Object
        Dim ecdf As New Distributions.ECDF(CLRVector.asNumeric(x))
        Dim wrap As Func(Of Object, Object) =
            Function(q As Object)
                Dim vq As Double() = CLRVector.asNumeric(q)
                Dim t As Double() = vq.Select(Function(qi) ecdf.FindThreshold(qi)).ToArray
                Return t
            End Function
        Dim x_json As String = "[" & ecdf.X.Take(6).JoinBy(", ") & "...]"
        Dim name As String = $"ecdf(x={x_json})"

        Return New RMethodInfo(name, wrap)
    End Function

    ''' <summary>
    ''' ## Empirical Cumulative Distribution Function
    ''' 
    ''' Compute an empirical cumulative distribution function
    ''' </summary>
    ''' <param name="FUNC">function for run value integral, this function should 
    ''' accept a number as parameter and produce new number as output.</param>
    ''' <param name="range">
    ''' the value range of the target function to do integral
    ''' </param>
    ''' <param name="p0">
    ''' the y0 value for the integral
    ''' </param>
    ''' <param name="resolution">
    ''' the RK4 integral algorithm resolution
    ''' </param>
    ''' <returns>
    ''' this function returns a tuple list object that contains the data slots:
    ''' 
    ''' + ``ecdf``: the result value of the integral, a numeric scalar value
    ''' + ``x``: a numeric vector for x axis
    ''' + ``y``: a numeric vector for y axis
    ''' </returns>
    <ExportAPI("CDF")>
    <RApiReturn("ecdf", "x", "y")>
    Public Function ECDF(FUNC As Object,
                         <RRawVectorArgument>
                         range As Object,
                         Optional p0 As Double = 0,
                         Optional resolution As Integer = 50000,
                         Optional env As Environment = Nothing) As Object

        Dim bounds = SMRUCC.Rsharp.GetDoubleRange(range, env, [default]:="0,0")

        If bounds Like GetType(Message) Then
            Return bounds.TryCast(Of Message)
        End If

        Dim lowUp As DoubleRange = bounds
        Dim result As Object = math.RK4(FUNC, p0, lowUp.Min, lowUp.Max, resolution, env)

        If TypeOf result Is Message Then
            Return result
        End If

        Dim output As ODEOutput = DirectCast(result, ODEOutput)
        Dim x As Double() = output.X.ToArray
        Dim y As Double() = output.Y.vector
        Dim cdf As Double = output.sum - p0

        Return New list(RType.GetRSharpType(GetType(Double))) With {
            .slots = New Dictionary(Of String, Object) From {
                {"ecdf", cdf},
                {"x", x},
                {"y", y}
            }
        }
    End Function

    ''' <summary>
    ''' Interpolating Splines
    ''' </summary>
    ''' <param name="data">
    ''' a set of the point data, could be a dataframe object that contains two data 
    ''' field: x and y, or a tuple list object contains two slot element: x and y.
    ''' or just a vector of the clr <see cref="Point"/> or <see cref="PointF"/> object
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("spline")>
    Public Function spline(<RRawVectorArgument>
                           data As Object,
                           Optional algorithm As SplineAlgorithms = SplineAlgorithms.BSpline,
                           Optional env As Environment = Nothing) As Object

        If data Is Nothing Then
            Return Nothing
        End If

        Select Case algorithm
            Case SplineAlgorithms.Bezier
            Case SplineAlgorithms.BSpline
            Case SplineAlgorithms.CatmullRom
            Case SplineAlgorithms.CubiSpline

        End Select

        Return RInternal.debug.stop($"unsupported spline algorithm: {algorithm.ToString}", env)
    End Function

    ''' <summary>
    ''' Average by removes outliers
    ''' </summary>
    ''' <param name="x">should be a numeric vector</param>
    ''' <param name="bags">
    ''' the number of histogram box of the input numeric data.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("tabulate.mode")>
    Public Function tabulateMode(<RRawVectorArgument>
                                 x As Object,
                                 Optional top_bin As Boolean = False,
                                 Optional bags As Integer = 5) As Double

        Return CLRVector.asNumeric(x) _
            .DoCall(Function(vec)
                        Return Bootstraping.TabulateMode(vec, top_bin, bags)
                    End Function)
    End Function

    ''' <summary>
    ''' ### Principal Components Analysis
    ''' 
    ''' Performs a principal components analysis on the given data matrix 
    ''' and returns the results as an object of class ``prcomp``.
    ''' 
    ''' The calculation is done by a singular value decomposition of the 
    ''' (centered and possibly scaled) data matrix, not by using eigen on 
    ''' the covariance matrix. This is generally the preferred method for 
    ''' numerical accuracy. The print method for these objects prints the 
    ''' results in a nice format and the plot method produces a scree 
    ''' plot.
    '''
    ''' Unlike princomp, variances are computed With the usual divisor N - 1.
    ''' Note that scale = True cannot be used If there are zero Or constant 
    ''' (For center = True) variables.
    ''' </summary>
    ''' <param name="x">
    ''' a numeric or complex matrix (or data frame) which provides the 
    ''' data for the principal components analysis.
    ''' </param>
    ''' <param name="center">
    ''' a logical value indicating whether the variables should be shifted 
    ''' to be zero centered. Alternately, a vector of length equal the 
    ''' number of columns of x can be supplied. The value is passed to scale.
    ''' </param>
    ''' <param name="scale">
    ''' a logical value indicating whether the variables should be scaled to 
    ''' have unit variance before the analysis takes place. The default is 
    ''' FALSE for consistency with S, but in general scaling is advisable. 
    ''' Alternatively, a vector of length equal the number of columns of x 
    ''' can be supplied. The value is passed to scale.
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' The signs of the columns of the rotation matrix are arbitrary, and 
    ''' so may differ between different programs for PCA, and even between 
    ''' different builds of R.
    ''' </remarks>
    <ExportAPI("prcomp")>
    <RApiReturn(GetType(MultivariateAnalysisResult))>
    Public Function prcomp(<RRawVectorArgument>
                           x As Object,
                           Optional scale As Boolean = False,
                           Optional center As Boolean = False,
                           Optional pc As Integer = 5,
                           Optional list As Boolean = True,
                           Optional threshold As Double = 0.0000001,
                           Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return RInternal.debug.stop("'data' must be of a vector type, was 'NULL'", env)
        End If

        Dim ds As StatisticsObject

        If TypeOf x Is Rdataframe Then
            ds = DirectCast(x, Rdataframe).GetDataSetCommon(DirectCast(x, Rdataframe).rownames)
        Else
            Throw New NotImplementedException
        End If

        Dim PCA = ds.PrincipalComponentAnalysis(pc, cutoff:=threshold)

        If Not list Then
            Return PCA
        End If

        Dim result As New list With {.slots = New Dictionary(Of String, Object)}

        result.add("contribution", PCA.Contributions.ToArray)
        result.add("score", MathDataSet.toDataframe(PCA.GetPCAScore, Nothing, env))
        result.add("loading", MathDataSet.toDataframe(PCA.GetPCALoading, Nothing, env))

        Return result
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="is_matrix"></param>
    ''' <param name="projection">
    ''' argument for extract data from the given data object, this parameter is depends 
    ''' on the <paramref name="is_matrix"/> argument, for:
    ''' 
    ''' + is_matrix: means the given data is a matrix liked data, then ``type`` should be exists.
    ''' + not is_matrix: means the given data is a tabular data for represents the data matrix, 
    '''                  then slots ``f1``, ``f2``, and ``val`` should be exists.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.dist")>
    Public Function asDist(x As Rdataframe,
                           Optional is_matrix As Boolean = True,
                           <RListObjectArgument>
                           Optional projection As list = Nothing,
                           Optional env As Environment = Nothing) As DistanceMatrix

        If is_matrix Then
            Dim matrix As Double()() = x.columns _
                .Select(Function(a)
                            Return a.Value _
                                .AsObjectEnumerator(Of String) _
                                .Select(AddressOf ParseDouble) _
                                .ToArray
                        End Function) _
                .ToArray
            Dim dist As Double()() = New Double(x.nrows - 1)() {}
            Dim is_dist As Boolean = projection!type <> "cor"
            Dim j As Integer

            For i As Integer = 0 To x.nrows - 1
                j = i
                dist(i) = matrix.Select(Function(col) col(j)).ToArray
            Next

            Return New DistanceMatrix(x.rownames, dist, is_dist)
        Else
            Dim raw As EntityObject() = x.getRowNames _
                .Select(Function(id, index)
                            Return x.dataframeRow(Of String, EntityObject)(id, index)
                        End Function) _
                .ToArray
            Dim item1$ = projection!f1
            Dim item2$ = projection!f2
            Dim val$ = projection!val

            Return LoadDataMatrix.FromTabular(raw, item1, item2, val)
        End If
    End Function

    ''' <summary>
    ''' 行的索引编号应该是以零为底的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="x"></param>
    ''' <param name="id"></param>
    ''' <param name="index">the row index</param>
    ''' <returns></returns>
    <Extension>
    Private Function dataframeRow(Of T, DataSet As {New, INamedValue, DynamicPropertyBase(Of T)})(x As Rdataframe, id As String, index%) As DataSet
        Dim row As Dictionary(Of String, Object) = x.getRowList(index, drop:=True)
        Dim props As Dictionary(Of String, T) = row _
            .ToDictionary(Function(a) a.Key,
                            Function(a)
                                Return CType(REnv.single(a.Value), T)
                            End Function)

        Return New DataSet With {
            .Key = id,
            .Properties = props
        }
    End Function

    ''' <summary>
    ''' matrix correlation
    ''' </summary>
    ''' <param name="x">evaluate correlation for each row elements</param>
    ''' <param name="positive">
    ''' only takes the positive part of the correlation matrix?
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("corr")>
    <RApiReturn(GetType(CorrelationMatrix))>
    Public Function corr(x As Rdataframe, Optional y As Rdataframe = Nothing,
                         Optional spearman As Boolean = False,
                         Optional positive As Boolean = False) As Object

        Dim cor As CorrelationMatrix

        If y Is Nothing Then
            Dim rows As DataSet() = x.getRowNames _
                .Select(Function(id, index)
                            Return x.dataframeRow(Of Double, DataSet)(id, index)
                        End Function) _
                .ToArray

            cor = rows.Correlation(spearman)
        Else
            Throw New NotImplementedException
        End If

        If positive Then
            cor = cor.PositiveMatrix
        End If

        Return cor
    End Function

    <ExportAPI("corr_sign")>
    Public Function corr_sign(c As CorrelationMatrix) As matrix
        Return New matrix With {.mat = c.Sign}
    End Function

    <ROperator("*")>
    Public Function mul(f As matrix, c As CorrelationMatrix) As CorrelationMatrix
        Return DirectCast(f.mat, Double()()) * c
    End Function

    <ROperator("^")>
    Public Function pow(c As CorrelationMatrix, p As Double) As CorrelationMatrix
        Return c.Power(p)
    End Function

    <ROperator("^")>
    Public Function pow(c As CorrelationMatrix, p As Integer) As CorrelationMatrix
        Return c.Power(p)
    End Function

    ''' <summary>
    ''' Find the correlations, sample sizes, and probability 
    ''' values between elements of a matrix or data.frame.
    ''' 
    ''' Although the cor function finds the correlations for 
    ''' a matrix, it does not report probability values. cor.test 
    ''' does, but for only one pair of variables at a time. 
    ''' corr.test uses cor to find the correlations for either 
    ''' complete or pairwise data and reports the sample sizes 
    ''' and probability values as well. For symmetric matrices, 
    ''' raw probabilites are reported below the diagonal and 
    ''' correlations adjusted for multiple comparisons above the 
    ''' diagonal. In the case of different x and ys, the default 
    ''' is to adjust the probabilities for multiple tests. Both 
    ''' corr.test and corr.p return raw and adjusted confidence 
    ''' intervals for each correlation.
    ''' </summary>
    ''' <param name="x">A matrix or dataframe</param>
    ''' <param name="y">
    ''' A second matrix or dataframe with the same number of rows as x
    ''' </param>
    ''' <param name="use">
    ''' use="pairwise" is the default value and will do pairwise 
    ''' deletion of cases. use="complete" will select just complete 
    ''' cases.</param>
    ''' <param name="method">
    ''' method="pearson" is the default value. The alternatives to 
    ''' be passed to cor are "spearman" and "kendall". These last 
    ''' two are much slower, particularly for big data sets.
    ''' </param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("corr.test")>
    Public Function corrTest(x As Rdataframe,
                             Optional y As Rdataframe = Nothing,
                             Optional use As String = "pairwise",
                             Optional method As String = "pearson")
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' ### Sample Quantiles
    ''' 
    ''' The generic function quantile produces sample quantiles corresponding 
    ''' to the given probabilities. The smallest observation corresponds to 
    ''' a probability of 0 and the largest to a probability of 1.
    ''' </summary>
    ''' <param name="x">
    ''' numeric vector whose sample quantiles are wanted, or an object of a class 
    ''' for which a method has been defined (see also ‘details’). NA and NaN 
    ''' values are not allowed in numeric vectors unless na.rm is TRUE.
    ''' </param>
    ''' <param name="probs">
    ''' numeric vector of probabilities with values in [0,1]. (Values up to 2e-14 
    ''' outside that range are accepted and moved to the nearby endpoint.)
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' A vector of length length(probs) is returned; if names = TRUE, it has a 
    ''' names attribute.
    ''' 
    ''' NA And NaN values in probs are propagated to the result.
    ''' The Default method works With classed objects sufficiently Like numeric 
    ''' vectors that sort And (Not needed by types 1 And 3) addition Of elements 
    ''' And multiplication by a number work correctly. Note that As this Is In a 
    ''' Namespace, the copy Of sort In base will be used, Not some S4 generic 
    ''' Of that name. Also note that that Is no check On the 'correctly’, and 
    ''' so e.g. quantile can be applied to complex vectors which (apart from ties) 
    ''' will be ordered on their real parts.
    ''' 
    ''' There Is a method for the date-time classes (see "POSIXt"). Types 1 And 3 
    ''' can be used for class "Date" And for ordered factors.
    ''' </returns>
    <ExportAPI("quantile")>
    <RApiReturn(GetType(QuantileEstimationGK), GetType(list))>
    Public Function quantile(x As Double(),
                             <RRawVectorArgument(GetType(Double))>
                             Optional probs As Object = "0,0.25,0.5,0.75,1",
                             Optional env As Environment = Nothing) As Object

        Dim probList As Double() = CLRVector.asNumeric(probs)
        Dim q As QuantileEstimationGK = x.GKQuantile

        If probList.IsNullOrEmpty Then
            Return q
        Else
            Dim qvals As Double() = probList _
                .Select(Function(p) q.Query(p)) _
                .ToArray
            Dim list As New Dictionary(Of String, Object)

            For i As Integer = 0 To probList.Length - 1
                list((probList(i) * 100) & "%") = qvals(i)
            Next

            Return New list With {.slots = list}
        End If
    End Function

    <ExportAPI("median")>
    Public Function median(x As Double()) As Double
        Return x.Median
    End Function

    ''' <summary>
    ''' get quantile levels
    ''' </summary>
    ''' <param name="q"></param>
    ''' <param name="level"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("level")>
    Public Function getQuantileLevels(q As QuantileQuery,
                                      <RRawVectorArgument>
                                      level As Object,
                                      Optional env As Environment = Nothing) As Object

        Dim levels As Double() = CLRVector.asNumeric(level)
        Dim result As Double() = levels.Select(AddressOf q.Query).ToArray

        Return result
    End Function

    ''' <summary>
    ''' ### Distance Matrix Computation
    ''' 
    ''' This function computes and returns the distance matrix computed by using 
    ''' the specified distance measure to compute the distances between the rows 
    ''' of a data matrix.
    ''' </summary>
    ''' <param name="x">a numeric matrix, data frame or "dist" object.</param>
    ''' <param name="method">
    ''' the distance measure to be used. This must be one of "euclidean", "maximum", 
    ''' "manhattan", "canberra", "binary" or "minkowski". Any unambiguous substring 
    ''' can be given.
    ''' </param>
    ''' <param name="diag"></param>
    ''' <param name="upper"></param>
    ''' <param name="p%"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Available distance measures are (written for two vectors x and y):
    '''
    ''' + euclidean:
    ''' Usual distance between the two vectors (2 norm aka L_2), sqrt(sum((x_i - y_i)^2)).
    '''
    ''' + maximum:
    ''' Maximum distance between two components Of x And y (supremum norm)
    '''
    ''' + manhattan:
    ''' Absolute distance between the two vectors (1 norm aka L_1).
    '''
    ''' + canberra:
    ''' ``sum(|x_i - y_i| / (|x_i| + |y_i|))``. Terms with zero numerator And 
    ''' denominator are omitted from the sum And treated as if the values were 
    ''' missing.
    '''
    ''' This Is intended for non-negative values (e.g., counts), in which case the 
    ''' denominator can be written in various equivalent ways; Originally, R used 
    ''' ``x_i + y_i``, then from 1998 to 2017, |x_i + y_i|, And then the correct 
    ''' ``|x_i| + |y_i|``.
    '''
    ''' + binary:
    ''' (aka asymmetric binary): The vectors are regarded As binary bits, so non-zero 
    ''' elements are 'on’ and zero elements are ‘off’. The distance is the proportion 
    ''' of bits in which only one is on amongst those in which at least one is on.
    '''
    ''' + minkowski:
    ''' The p norm, the pth root Of the sum Of the pth powers Of the differences 
    ''' Of the components.
    '''
    ''' Missing values are allowed, And are excluded from all computations involving 
    ''' the rows within which they occur. Further, When Inf values are involved, 
    ''' all pairs Of values are excluded When their contribution To the distance gave 
    ''' NaN Or NA. If some columns are excluded In calculating a Euclidean, Manhattan, 
    ''' Canberra Or Minkowski distance, the sum Is scaled up proportionally To the 
    ''' number Of columns used. If all pairs are excluded When calculating a 
    ''' particular distance, the value Is NA.
    '''
    ''' The "dist" method of as.matrix() And as.dist() can be used for conversion 
    ''' between objects of class "dist" And conventional distance matrices.
    '''
    ''' ``as.dist()`` Is a generic function. Its default method handles objects inheriting 
    ''' from class "dist", Or coercible to matrices using as.matrix(). Support for 
    ''' classes representing distances (also known as dissimilarities) can be added 
    ''' by providing an as.matrix() Or, more directly, an as.dist method for such 
    ''' a class.
    ''' </remarks>
    <ExportAPI("dist")>
    <RApiReturn(GetType(DistanceMatrix))>
    Public Function dist(<RRawVectorArgument> x As Object,
                         Optional method$ = "euclidean",
                         Optional diag As Boolean = False,
                         Optional upper As Boolean = False,
                         Optional p% = 2,
                         Optional env As Environment = Nothing) As Object

        Dim raw As DataSet()

        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is Rdataframe Then
            With DirectCast(x, Rdataframe)
                raw = .getRowNames _
                    .Select(Function(name, i)
                                Return .dataframeRow(Of Double, DataSet)(name, i)
                            End Function) _
                    .ToArray
            End With
        ElseIf TypeOf x Is DataSet() Then
            raw = x
        Else
            Return RInternal.debug.stop(New InvalidCastException(x.GetType.FullName), env)
        End If

        Select Case Strings.LCase(method)
            Case "euclidean"
                Return raw.Euclidean
            Case Else
                Return RInternal.debug.stop(New NotImplementedException(method), env)
        End Select
    End Function

    ''' <summary>
    ''' ## The Student t Distribution
    ''' 
    ''' Density, distribution function, quantile function and random generation for the t distribution with df degrees of freedom (and optional non-centrality parameter ncp).
    ''' </summary>
    ''' <param name="x">vector of quantiles. the t-test value
    ''' </param>
    ''' <param name="df">degrees of freedom (>0, maybe non-integer). df = Inf Is allowed.</param>
    ''' <param name="env"></param>
    ''' <returns>cdf value of the t-distribution</returns>
    <ExportAPI("pt")>
    Public Function pt(<RRawVectorArgument> x As Object, df As Integer, Optional env As Environment = Nothing) As Object
        Return t.Tcdf(CLRVector.asNumeric(x), df).ToArray
    End Function

    ''' <summary>
    ''' Student's t-Test
    ''' 
    ''' Performs one and two sample t-tests on vectors of data.
    ''' </summary>
    ''' <param name="x">a (non-empty) numeric vector of data values.</param>
    ''' <param name="y">an optional (non-empty) numeric vector of data values.
    ''' </param>
    ''' <param name="alternative">
    ''' a character string specifying the alternative hypothesis, must be one of 
    ''' "two.sided" (default), "greater" or "less". You can specify just the initial 
    ''' letter.
    ''' </param>
    ''' <param name="mu">
    ''' a number indicating the true value of the mean (or difference in means if you 
    ''' are performing a two sample test).
    ''' </param>
    ''' <param name="paired">	
    ''' a logical indicating whether you want a paired t-test.</param>
    ''' <param name="var_equal">
    ''' a logical variable indicating whether to treat the two variances as being equal. 
    ''' If TRUE then the pooled variance is used to estimate the variance otherwise the 
    ''' Welch (or Satterthwaite) approximation to the degrees of freedom is used.
    ''' </param>
    ''' <param name="conf_level">
    ''' confidence level of the interval.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("t.test")>
    <RApiReturn(GetType(TwoSampleResult), GetType(TtestResult))>
    Public Function ttest(<RRawVectorArgument> x As Object, <RRawVectorArgument> Optional y As Object = Nothing,
                          Optional alternative As Hypothesis = Hypothesis.TwoSided,
                          Optional mu# = 0,
                          Optional paired As Boolean = False,
                          Optional var_equal As Boolean = False,
                          Optional conf_level# = 0.95,
                          Optional t As FormulaExpression = Nothing,
                          Optional env As Environment = Nothing) As Object

        If Not t Is Nothing AndAlso TypeOf x Is Rdataframe Then
            Return DirectCast(x, Rdataframe).ttestBatch(t, alternative, y, mu, conf_level, var_equal, env)
        Else
            ' t-test compare two vector
            Return alternative.ttestImpl(x, y, mu, conf_level, var_equal, env)
        End If
    End Function

    <Extension>
    Private Function ttestBatch(table As Rdataframe, t As FormulaExpression,
                                alternative As Hypothesis,
                                y As Object,
                                mu As Double,
                                conf_level As Double,
                                var_equal As Boolean,
                                env As Environment) As Object

        Dim symbols As String() = FormulaExpression.GetSymbols(t.formula)
        Dim v As New List(Of NamedCollection(Of Double))
        Dim ref As Symbol

        For Each name As String In symbols
            If table.hasName(name) Then
                v.Add(name, CLRVector.asNumeric(table.getColumnVector(name)))
            Else
                ref = env.FindSymbol(name)

                If ref Is Nothing Then
                    Return RInternal.debug.stop($"missing required symbol '{name}' for evaluate formula!", env)
                Else
                    v.Add(name, CLRVector.asNumeric(ref.value))
                End If
            End If
        Next

        Dim test As Object() = New Object(table.nrows - 1) {}
        Dim idx As Integer
        Dim x As Object

        For i As Integer = 0 To table.nrows - 1
            idx = i
            x = symbols _
                .Select(Function(r, offset) v(offset)(idx)) _
                .ToArray
            x = alternative.ttestImpl(x, y, mu, conf_level, var_equal, env)

            If Program.isException(x) Then
                Return x
            Else
                test(i) = x
            End If
        Next

        Return test
    End Function

    ''' <summary>
    ''' t-test compare two vector
    ''' </summary>
    ''' <param name="alternative"></param>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="mu"></param>
    ''' <param name="conf_level"></param>
    ''' <param name="var_equal"></param>
    ''' <returns></returns>
    <Extension>
    Private Function ttestImpl(alternative As Hypothesis,
                               x As Object, y As Object,
                               mu As Double,
                               conf_level As Double,
                               var_equal As Boolean, env As Environment) As Object
        Dim test As Object
        Dim vx As Double() = CLRVector.asNumeric(x)
        Dim vy As Double()

        vx = safeCheck(vx, "x", env)

        If vx.Length > 0 Then
            vx(Scan0) += 0.0000001
        End If

        If y Is Nothing Then
            test = Statistics.Hypothesis.t.Test(vx, alternative, mu, alpha:=1 - conf_level)
        Else
            vy = CLRVector.asNumeric(y)
            vy = safeCheck(vy, "y", env)

            test = Statistics.Hypothesis.t.Test(
                a:=vx,
                b:=vy,
                alternative:=alternative,
                mu:=mu,
                alpha:=1 - conf_level,
                varEqual:=var_equal
            )
        End If

        Return test
    End Function

    Private Function safeCheck(x As Double(), tag As String, env As Environment) As Double()
        Dim offset As List(Of Integer) = Nothing

        For i As Integer = 0 To x.Length - 1
            Dim xi As Double = x(i)

            If xi.IsNaNImaginary Then
                If offset Is Nothing Then
                    offset = New List(Of Integer)
                End If

                Call offset.Add(i)
            End If
        Next

        If offset IsNot Nothing Then
            Dim x_mean As Double = x.Where(Function(d) Not d.IsNaNImaginary).Average

            For Each i As Integer In offset
                x(i) = x_mean
            Next

            Call env.AddMessage({
                $"invalid value was found in the input '{tag}' vector, NA or INF value has been replaced as vector mean: {x_mean}.",
                $"offsets(NA or INF): {offset.JoinBy(", ")}.",
                $"vector source: {tag}"
            })
        End If

        Return x
    End Function

    ''' <summary>
    ''' Fisher's Exact Test for Count Data
    ''' 
    ''' Performs Fisher's exact test for testing the null of independence 
    ''' of rows and columns in a contingency table with fixed marginals.
    ''' </summary>
    ''' <param name="a%"></param>
    ''' <param name="b%"></param>
    ''' <param name="c%"></param>
    ''' <param name="d%"></param>
    ''' <returns></returns>
    <ExportAPI("fisher.test")>
    Public Function fisher_test(a%, b%, c%, d%) As FishersExactPvalues
        Return FishersExactTest.FishersExact(a, b, c, d)
    End Function

    ''' <summary>
    ''' ### Pearson's Chi-squared Test for Count Data
    ''' 
    ''' chisq.test performs chi-squared contingency table tests and goodness-of-fit tests.
    ''' </summary>
    ''' <param name="x">a numeric vector Or matrix. x And y can also both be factors.</param>
    ''' <param name="y">a numeric vector; ignored if x is a matrix. If x is a factor, y should be a factor of the same length.</param>
    ''' <param name="correct">a logical indicating whether to apply continuity correction when computing the test statistic for 2 by 2 tables: 
    ''' one half is subtracted from all ||O-E|| differences; however, the correction will Not be bigger than the differences themselves. 
    ''' No correction Is done if simulate.p.value = TRUE.</param>
    ''' <param name="p">a vector of probabilities of the same length as x. An error is given if any entry of p is negative.</param>
    ''' <param name="rescale_p">a logical scalar; if TRUE then p is rescaled (if necessary) to sum to 1. If rescale.p is FALSE, 
    ''' and p does not sum to 1, an error is given.</param>
    ''' <param name="simulate_p_value">a logical indicating whether to compute p-values by Monte Carlo simulation.</param>
    ''' <param name="B">an integer specifying the number of replicates used in the Monte Carlo test.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' If x is a matrix with one row or column, or if x is a vector and y is not given, then a goodness-of-fit
    ''' test is performed (x is treated as a one-dimensional contingency table). The entries of x must be 
    ''' non-negative integers. In this case, the hypothesis tested is whether the population probabilities
    ''' equal those in p, or are all equal if p is not given.
    '''
    ''' If x is a matrix with at least two rows and columns, it is taken as a two-dimensional contingency table:
    ''' the entries of x must be non-negative integers. Otherwise, x and y must be vectors or factors of the
    ''' same length; cases with missing values are removed, the objects are coerced to factors, and the contingency
    ''' table is computed from these. Then Pearson's chi-squared test is performed of the null hypothesis that 
    ''' the joint distribution of the cell counts in a 2-dimensional contingency table is the product of the 
    ''' row and column marginals.
    '''
    ''' If simulate.p.value is FALSE, the p-value is computed from the asymptotic chi-squared distribution of the
    ''' test statistic; continuity correction is only used in the 2-by-2 case (if correct is TRUE, the default). 
    ''' Otherwise the p-value is computed for a Monte Carlo test (Hope, 1968) with B replicates. The default 
    ''' B = 2000 implies a minimum p-value of about 0.0005 (1 / (B + 1)).
    '''
    ''' In the contingency table case, simulation is done by random sampling from the set of all contingency tables
    ''' with given marginals, and works only if the marginals are strictly positive. Continuity correction is never 
    ''' used, and the statistic is quoted without it. Note that this is not the usual sampling situation assumed
    ''' for the chi-squared test but rather that for Fisher's exact test.
    '''
    ''' In the goodness-of-fit case simulation is done by random sampling from the discrete distribution specified 
    ''' by p, each sample being of size ``n = sum(x)``. This simulation is done in R and may be slow.
    ''' </remarks>
    ''' <example>
    ''' table &lt;- matrix(c(30, 50, 20, 10), nrow = 2)
    ''' rownames(table) &lt;- c("Group1", "Group2")
    ''' colnames(table) &lt;- c("Yes", "No")
    ''' 
    ''' result &lt;- chisq.test(table)
    ''' 
    ''' print(result)
    ''' </example>
    <ExportAPI("chisq.test")>
    Public Function chisq_test(<RRawVectorArgument> x As Object,
                               <RRawVectorArgument>
                               Optional y As Object = Nothing,
                               Optional correct As Boolean = True,
                               Optional p As Object = "~rep(1/length(x), length(x))",
                               Optional rescale_p As Boolean = False,
                               Optional simulate_p_value As Boolean = False,
                               Optional B As Integer = 2000,
                               Optional env As Environment = Nothing) As Object

        Dim observed As Double()()
        Dim expected As Double()() = Nothing

        If x Is Nothing Then
            Return RInternal.debug.stop("the required observed ``x`` ", env)
        End If

        If TypeOf x Is Rdataframe Then
            Dim df As Rdataframe = x
            Dim rows = df.columns.Select(Function(r) CLRVector.asNumeric(r.Value)).ToArray

            observed = rows
        ElseIf TypeOf x Is NumericMatrix Then
            observed = DirectCast(x, NumericMatrix).ToArray
        Else
            Return RInternal.debug.stop("the required observed ``x`` should be a numeric matrix!", env)
        End If

        Dim chisq As ChiSquareTest

        If expected Is Nothing Then
            chisq = ChiSquareTest.Test(observed)
        Else
            chisq = ChiSquareTest.Test(observed, expected)
        End If

        Return New list(
            slot("statistic") = chisq.chi_square,
            slot("p.value") = chisq.pvalue,
            slot("observed") = chisq.observed.IteratesALL.ToArray,
            slot("expected") = chisq.expected.IteratesALL.ToArray
        )
    End Function

    ''' <summary>
    ''' Calculate Moran's I quickly for point data
    ''' 
    ''' test spatial cluster via moran index
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="alternative">
    ''' a character sring specifying the alternative hypothesis that is
    ''' tested against; must be one of "two.sided", "less", or "greater",
    ''' or any unambiguous abbreviation of these.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("moran.test")>
    <RApiReturn("observed", "expected", "sd", "p.value", "z", "prob2", "t", "df")>
    Public Function moran_test(<RRawVectorArgument> x As Object,
                               Optional sx As Double() = Nothing,
                               Optional sy As Double() = Nothing,
                               Optional alternative As Hypothesis = Hypothesis.TwoSided,
                               Optional throwMaxIterError As Boolean = False,
                               Optional parallel As Boolean = True,
                               Optional env As Environment = Nothing) As Object
        Dim test As MoranTest

        If Not (sx.IsNullOrEmpty OrElse sy.IsNullOrEmpty) Then
            test = MoranTest.moran_test(CLRVector.asNumeric(x), sx, sy, alternative,
                                        throwMaxIterError:=throwMaxIterError,
                                        parallel:=parallel)
        ElseIf TypeOf x Is Rdataframe Then
            Dim df As Rdataframe = x
            Dim v As Double() = CLRVector.asNumeric(df!data)

            sx = CLRVector.asNumeric(df!x)
            sy = CLRVector.asNumeric(df!y)
            test = MoranTest.moran_test(v, sx, sy, alternative,
                                        throwMaxIterError:=throwMaxIterError,
                                        parallel:=parallel)
        Else
            Dim spatial As pipeline = pipeline.TryCreatePipeline(Of Pixel)(x, env)

            If spatial.isError Then
                Return spatial.getError
            End If

            test = MoranTest.moran_test(spatial.populates(Of Pixel)(env), alternative,
                                        throwMaxIterError:=throwMaxIterError,
                                        parallel:=parallel)
        End If

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"observed", test.Observed},
                {"expected", test.Expected},
                {"sd", test.SD},
                {"p.value", test.pvalue},
                {"z", test.z},
                {"prob2", test.prob2},
                {"t", test.t},
                {"df", test.df}
            }
        }
    End Function

    ''' <summary>
    ''' The Mantel test, named after Nathan Mantel, is a statistical test of 
    ''' the correlation between two matrices. The matrices must be of the same
    ''' dimension; in most applications, they are matrices of interrelations 
    ''' between the same vectors of objects. The test was first published by 
    ''' Nathan Mantel, a biostatistician at the National Institutes of Health, 
    ''' in 1967.[1] Accounts of it can be found in advanced statistics books 
    ''' (e.g., Sokal &amp; Rohlf 1995[2]).
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("mantel.test")>
    Public Function mantel_test(<RRawVectorArgument> x As Object,
                                <RRawVectorArgument> y As Object,
                                <RRawVectorArgument>
                                Optional c As Object = Nothing,
                                Optional exact As Boolean = False,
                                Optional raw As Boolean = False,
                                Optional permutation As Integer = 1000,
                                Optional env As Environment = Nothing) As Object

        Dim err As Message = Nothing
        Dim matA = getMatrix(x, env, "source x", err)
        Dim matC As Double()() = Nothing

        If Not err Is Nothing Then
            Return err
        End If

        Dim matB = getMatrix(y, env, "source y", err)

        If Not err Is Nothing Then
            Return err
        End If

        If Not c Is Nothing Then
            matC = getMatrix(c, env, "source c", err)

            If Not err Is Nothing Then
                Return err
            End If
        End If

        Dim model As New Mantel.Model With {
            .[partial] = Not c Is Nothing,
            .exact = exact,
            .raw = raw,
            .matsize = matA.Length,
            .numrand = permutation
        }
        Dim res = Mantel.test(model, matA, matB, matC)
        Dim par As New list With {
            .slots = New Dictionary(Of String, Object) From {
                {NameOf(Mantel.Model.exact), res.exact},
                {NameOf(Mantel.Model.partial), res.partial},
                {NameOf(Mantel.Model.matsize), res.matsize},
                {NameOf(Mantel.Model.numrand), res.numrand},
                {NameOf(Mantel.Model.raw), res.raw}
            }
        }

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"coef", res.coef},
                {"pvalue", res.proba},
                {"par", par}
            }
        }
    End Function

    Private Function getLabels(x As Object, env As Environment, tag As String, <Out> ByRef err As Message, strict As Boolean) As String()
        If TypeOf x Is Rdataframe Then
            Dim df As Rdataframe = x
            Dim labels = df.getRowNames
            Return labels
        ElseIf x.GetType.ImplementInterface(Of ILabeledMatrix) Then
            Return DirectCast(x, ILabeledMatrix).GetLabels.ToArray
        ElseIf strict Then
            err = Message.InCompatibleType(GetType(Rdataframe), x.GetType, env, message:=$"can not extract numeric matrix from {tag}!")
        Else
            err = Nothing
        End If

        Return Nothing
    End Function

    Private Function getMatrix(x As Object, env As Environment, tag As String, <Out> ByRef err As Message) As Double()()
        If TypeOf x Is Rdataframe Then
            Dim mat As New List(Of Double())
            Dim df As Rdataframe = DirectCast(x, Rdataframe)
            Dim v As Array
            Dim nrows As Integer = df.nrows

            For Each i In df.forEachRow
                v = i.ToArray
                v = REnv.TryCastGenericArray(v, env)

                Call mat.Add(v)
            Next

            Return mat.ToArray
        ElseIf x.GetType.ImplementInterface(Of INumericMatrix) Then
            Return DirectCast(x, INumericMatrix).ArrayPack
        Else
            err = Message.InCompatibleType(GetType(Rdataframe), x.GetType, env, message:=$"can not extract numeric matrix from {tag}!")
            Return Nothing
        End If
    End Function

    <ExportAPI("lowess")>
    Public Function Lowess(<RRawVectorArgument> x As Object,
                           Optional f As Double = 2 / 3,
                           Optional nsteps As Integer = 3,
                           Optional env As Environment = Nothing) As Object

        Dim px As Single(), py As Single()

        If TypeOf x Is Rdataframe Then
            With DirectCast(x, Rdataframe)
                px = CLRVector.asFloat(!x)
                py = CLRVector.asFloat(!y)
            End With
        ElseIf TypeOf x Is list Then
            With DirectCast(x, list)
                px = CLRVector.asFloat(!x)
                py = CLRVector.asFloat(!y)
            End With
        Else
            Return RInternal.debug.stop("", env)
        End If

        Dim fit = px.Select(Function(xi, i) New PointF(xi, py(i))).Lowess(f, nsteps)
        Dim result As New Rdataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"x", fit.x},
                {"y", fit.y}
            }
        }

        Return result
    End Function

    ''' <summary>
    ''' ## F Test to Compare Two Variances
    ''' 
    ''' Performs an F test to compare the variances of
    ''' two samples from normal populations.
    ''' </summary>
    ''' <param name="x">
    ''' numeric vectors of data values, or fitted linear model objects 
    ''' (inheriting from class "lm").
    ''' </param>
    ''' <param name="y">
    ''' numeric vectors of data values, or fitted linear model objects 
    ''' (inheriting from class "lm").
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' The null hypothesis is that the ratio of the variances of the 
    ''' populations from which x and y were drawn, or in the data to 
    ''' which the linear models x and y were fitted, is equal to ratio.
    ''' </remarks>
    <ExportAPI("var.test")>
    Public Function varTest(<RRawVectorArgument> x As Object,
                            <RRawVectorArgument> y As Object,
                            Optional env As Environment = Nothing) As Object

        Dim vx As Double() = CLRVector.asNumeric(x)
        Dim vy As Double() = CLRVector.asNumeric(y)
        Dim ftest As New FTest(vx, vy)

        Return ftest
    End Function

    ''' <summary>
    ''' ## Fit an Analysis of Variance Model
    ''' 
    ''' Fit an analysis of variance model by a call to lm for each stratum.
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("aov")>
    Public Function aov(x As Rdataframe,
                        Optional formula As FormulaExpression = Nothing,
                        Optional env As Environment = Nothing) As Object

        Dim anova As New AnovaTest()
        Dim observations As New List(Of Double())

        If formula Is Nothing Then
            For Each name As String In x.colnames
                Dim v As Double() = CLRVector.asNumeric(x.getColumnVector(name))
                Call observations.Add(v)
            Next
        Else
            Dim responseVar = formula.GetResponseSymbol
            Dim responseSymbol As String

            If responseVar Like GetType(Exception) Then
                Return RInternal.debug.stop(responseVar.TryCast(Of Exception), env)
            Else
                responseSymbol = responseVar.TryCast(Of String)
            End If

            Dim factor As Expression = formula.formula

            If TypeOf factor Is SymbolReference Then
                Dim factorName As String = DirectCast(factor, SymbolReference).symbol
                Dim factors As String() = CLRVector.asCharacter(x.getColumnVector(factorName))
                Dim data As Double() = CLRVector.asNumeric(x.getColumnVector(responseSymbol))
                Dim groups = factors.Select(Function(k, i) (k, data(i))).GroupBy(Function(i) i.k).ToArray

                For Each group In groups
                    Call observations.Add(group.Select(Function(i) i.Item2).ToArray)
                Next
            Else
                Return RInternal.debug.stop($"not implemented for the {factor.GetType.FullName}!", env)
            End If
        End If

        anova.populate(observations, type:=AnovaTest.P_FIVE_PERCENT)
        anova.findWithinGroupMeans()
        anova.setSumOfSquaresOfGroups()
        anova.setTotalSumOfSquares()
        anova.divide_by_degrees_of_freedom()

        Dim f_score As Double = anova.fScore_determineIt()
        Dim criticalNumber = anova.criticalNumber
        Dim result As String = "The null hypothesis is supported! There is no especial difference in these groups. "

        If f_score > criticalNumber Then
            result = "The null hypothesis is rejected! These groups are different."
        End If

        Dim output As New list With {.slots = New Dictionary(Of String, Object)}

        Call output.add("Groups degrees of freedom", anova.numenator)
        Call output.add("Observations degrees of freedom", anova.denomenator)
        Call output.add("SSW_sum_of_squares_within_groups", anova.SSW_sum_of_squares_within_groups)
        Call output.add("SSB_sum_of_squares_between_groups", anova.SSB_sum_of_squares_between_groups)
        Call output.add("SSB", anova.SSB)
        Call output.add("SSW", anova.SSW)
        Call output.add("Residual standard error", std.Sqrt(anova.SSW))
        Call output.add("SS_total_sum_of_squares", anova.SS_total_sum_of_squares)
        Call output.add("allObservationsMean", anova.allObservationsMean)
        Call output.add("Critical number", criticalNumber)
        Call output.add("F Score", f_score)
        Call output.add("Pvalue", anova.singlePvalue)
        Call output.add("Pvalue(double_tailed)", anova.doublePvalue)
        Call output.add("level", anova.type)
        Call output.add("hypothesis", result)
        Call output.add("summary", anova.ToString)

        Return output
    End Function

    ''' <summary>
    ''' set the NA, NaN, Inf value to the default value
    ''' </summary>
    ''' <param name="x">
    ''' a numeric vector or a dataframe object of all elements in numeric mode.
    ''' </param>
    ''' <param name="default"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("filterMissing")>
    Public Function filterMissing(<RRawVectorArgument>
                                  x As Object,
                                  Optional [default] As Double = 0.0,
                                  Optional env As Environment = Nothing) As Object

        If TypeOf x Is Rdataframe Then
            ' run for each column 
            Dim d As Rdataframe = DirectCast(x, Rdataframe)
            Dim cols = d.columns _
                .ToDictionary(Function(c) c.Key,
                                Function(c)
                                    Dim v As Double() = CLRVector.asNumeric(c.Value)
                                    Dim m As Double() = v _
                                        .Select(Function(xi) If(xi.IsNaNImaginary, [default], xi)) _
                                        .ToArray

                                    Return DirectCast(m, Array)
                                End Function)

            Return New Rdataframe With {
                .rownames = d.rownames,
                .columns = cols
            }
        Else
            Dim v As Double() = CLRVector.asNumeric(x)
            Dim m As Double() = v _
                .Select(Function(xi) If(xi.IsNaNImaginary, [default], xi)) _
                .ToArray

            Return m
        End If
    End Function

    ''' <summary>
    ''' common method for construct dataset for run pca/plsda/oplsda analysis
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y">Y could be nothing for PCA analysis</param>
    ''' <returns></returns>
    <Extension>
    Private Function GetDataSetCommon(x As Rdataframe, y As Array) As StatisticsObject
        Dim colnames As String() = x.colnames

        If Not y.IsNullOrEmpty Then
            y = REnv.UnsafeTryCastGenericArray(y)
        End If

        Return x.forEachRow _
            .Select(Function(r)
                        Return New NamedCollection(Of Double)(
                            name:=r.name,
                            value:=CLRVector.asNumeric(r.value)
                        )
                    End Function) _
            .CommonDataSet(colnames, labels:=y)
    End Function

    <ExportAPI("opls")>
    Public Function oplsr(x As Rdataframe, <RRawVectorArgument> y As Object,
                          Optional ncomp As Integer? = Nothing,
                          Optional center As Boolean = True,
                          Optional scale As Boolean = False,
                          Optional list As Boolean = True,
                          Optional env As Environment = Nothing) As Object

        If y Is Nothing Then
            Return RInternal.debug.stop({
                "the sample class information should not be nothing",
                "it must be a vector of numeric data for regression or a character vector for classification!"}, env)
        Else
            If TypeOf y Is vec Then
                y = DirectCast(y, vec).data
            End If

            y = REnv.TryCastGenericArray(y, env)
        End If

        Dim ds As StatisticsObject = x.GetDataSetCommon(y)
        Dim opls_mvar = OPLS.OrthogonalProjectionsToLatentStructures(ds, component:=If(ncomp, -1))

        If Not list Then
            Return opls_mvar
        End If

        Dim score = opls_mvar.GetPLSScore(opls:=True)
        Dim loading = opls_mvar.GetPLSLoading(opls:=True)
        Dim components = opls_mvar.GetComponents.ToArray
        Dim scoreMN As Rdataframe = MathDataSet.toDataframe(score, Nothing, env)
        Dim loadingMN As Rdataframe = MathDataSet.toDataframe(loading, Nothing, env)
        Dim componentDf As New Rdataframe With {
            .rownames = components.Select(Function(ci) CStr(ci.Order)).ToArray,
            .columns = New Dictionary(Of String, Array)
        }

        Call componentDf.add("Component", components.Select(Function(ci) ci.Order))
        Call componentDf.add("SSCV", components.Select(Function(ci) ci.SSCV))
        Call componentDf.add("PRESS", components.Select(Function(ci) ci.Press))
        Call componentDf.add("Q2", components.Select(Function(ci) ci.Q2))
        Call componentDf.add("Q2(cum)", components.Select(Function(ci) ci.Q2cum))

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"component", componentDf},
                {"scoreMN", scoreMN},
                {"loadingMN", loadingMN}
            }
        }
    End Function

    ''' <summary>
    ''' ### Classical (Metric) Multidimensional Scaling
    ''' 
    ''' Classical multidimensional scaling (MDS) of a data matrix. Also known as principal coordinates analysis (Gower, 1966).
    ''' </summary>
    ''' <param name="d">a distance structure such as that returned by dist or a full symmetric matrix containing the dissimilarities.</param>
    ''' <param name="k">the maximum dimension of the space which the data are to be represented in; must be in {1,2,…,n−1}.</param>
    ''' <param name="_list">
    ''' logical indicating If a list should be returned Or just the n×k matrix, see 'Value:’.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' If ``.list`` is false (as per default), a matrix with k columns whose rows give the coordinates of the points chosen to represent the dissimilarities.
    ''' Otherwise, a list containing the following components.
    ''' 
    ''' + points: a matrix with up to k columns whose rows give the coordinates of the points chosen to represent the dissimilarities.
    ''' + eig: the n eigenvalues computed during the scaling process if eig is true. NB: versions of R before 2.12.1 returned only k but were documented to return n−1.
    ''' + x: the doubly centered distance matrix if x.ret is true.
    ''' + ac: the additive constant c∗, 0 if add = FALSE.
    ''' + GOF: a numeric vector of length 2, equal to say (g1 ,g2), where gi =(∑j=1,k λj )/(∑j=1,n Ti (λj )), where 
    '''        λj are the eigenvalues (sorted in decreasing order), T1 (v)=∣v∣, and T2 (v)=max(v,0).
    ''' </returns>
    ''' <remarks>
    ''' Multidimensional scaling takes a set of dissimilarities and returns a set of points such that the distances 
    ''' between the points are approximately equal to the dissimilarities. (It is a major part of what ecologists 
    ''' call ‘ordination’.)
    ''' A set of Euclidean distances on n points can be represented exactly in at most 
    ''' n−1 dimensions. cmdscale follows the analysis of Mardia (1978), and returns the best-fitting 
    ''' k-dimensional representation, where k may be less than the argument k.
    ''' The representation is only determined up to location (cmdscale takes the column means of the configuration 
    ''' to be at the origin), rotations and reflections. The configuration returned is given in principal-component 
    ''' axes, so the reflection chosen may differ between R platforms (see prcomp).
    ''' When add = TRUE, a minimal additive constant c∗ is computed such that the dissimilarities 
    ''' dij +c∗ are Euclidean and hence can be represented in n - 1 dimensions. Whereas S (Becker et al, 1988) 
    ''' computes this constant using an approximation suggested by Torgerson, R uses the analytical solution of 
    ''' Cailliez (1983), see also Cox and Cox (2001). Note that because of numerical errors the computed eigenvalues 
    ''' need not all be non-negative, and even theoretically the representation could be in fewer than n - 1 
    ''' dimensions.
    ''' </remarks>
    <ExportAPI("cmdscale")>
    Public Function cmdscale(<RRawVectorArgument> d As Object,
                             Optional k As Integer = 2,
                             Optional _list As Boolean = False,
                             Optional env As Environment = Nothing) As Object

        Dim err As Message = Nothing
        Dim x = getMatrix(d, env, "source d", err)

        If Not err Is Nothing Then
            Return err
        End If

        Dim pco = MDS.fullmds(x, [dim]:=k)
        Dim labels As String() = getLabels(d, env, "source d", Nothing, strict:=False)

        If labels Is Nothing Then
            labels = pco.Sequence(offSet:=1).Select(Function(i) $"r-{i}").ToArray
        End If

        Dim pos As New Rdataframe With {
            .rownames = labels,
            .columns = New Dictionary(Of String, Array)
        }

        For i As Integer = 0 To k - 1
            pos.add("D" & (i + 1), pco(i))
        Next

        If _list Then
            Return New list(slot("points") = pos)
        Else
            Return pos
        End If
    End Function

    ''' <summary>
    ''' ## Partial Least Squares Discriminant Analysis
    ''' 
    ''' ``plsda`` is used to calibrate, validate and use of partial least squares discrimination analysis (PLS-DA) model.
    ''' </summary>
    ''' <param name="x">matrix with predictors.</param>
    ''' <param name="y">vector with class membership (should be either a factor with class
    ''' names/numbers in case of multiple classes Or a vector with logical values in case
    ''' of one class model).</param>
    ''' <param name="ncomp">maximum number Of components To calculate.</param>
    ''' <param name="center">logical, center or not predictors and response values.</param>
    ''' <param name="scale">logical, scale (standardize) or not predictors and response values.</param>
    ''' <param name="list">
    ''' this function will returns a R# list that contains result data of the PLS-DA analysis by default, 
    ''' or the raw .NET clr object of the PLS result if this parameter value set to FALSE.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("plsda")>
    <RApiReturn("component", "scoreMN", "loadingMN")>
    Public Function plsda(x As Rdataframe, <RRawVectorArgument> y As Object,
                          Optional ncomp As Integer? = Nothing,
                          Optional center As Boolean = True,
                          Optional scale As Boolean = False,
                          Optional list As Boolean = True,
                          Optional env As Environment = Nothing) As Object

        If y Is Nothing Then
            Return RInternal.debug.stop({
                "the sample class information should not be nothing",
                "it must be a vector of numeric data for regression or a character vector for classification!"}, env)
        Else
            If TypeOf y Is vec Then
                y = DirectCast(y, vec).data
            End If

            y = REnv.TryCastGenericArray(y, env)
        End If

        Dim ds As StatisticsObject = x.GetDataSetCommon(y)
        Dim pls_mvar = PLS.PartialLeastSquares(ds, component:=If(ncomp, -1))

        If Not list Then
            Return pls_mvar
        End If

        Dim score = pls_mvar.GetPLSScore(opls:=False)
        Dim loading = pls_mvar.GetPLSLoading(opls:=False)
        Dim components = pls_mvar.GetComponents.ToArray
        Dim scoreMN As Rdataframe = MathDataSet.toDataframe(score, Nothing, env)
        Dim loadingMN As Rdataframe = MathDataSet.toDataframe(loading, Nothing, env)
        Dim componentDf As New Rdataframe With {
            .rownames = components.Select(Function(ci) CStr(ci.Order)).ToArray,
            .columns = New Dictionary(Of String, Array)
        }

        Call componentDf.add("Component", components.Select(Function(ci) ci.Order))
        Call componentDf.add("SSCV", components.Select(Function(ci) ci.SSCV))
        Call componentDf.add("PRESS", components.Select(Function(ci) ci.Press))
        Call componentDf.add("Q2", components.Select(Function(ci) ci.Q2))
        Call componentDf.add("Q2(cum)", components.Select(Function(ci) ci.Q2cum))

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"component", componentDf},
                {"scoreMN", scoreMN},
                {"loadingMN", loadingMN}
            }
        }
    End Function

    ''' <summary>
    ''' z-score
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="byrow">
    ''' this parameter works when the data type of the input 
    ''' data <paramref name="x"/> is a dataframe or matrix
    ''' object
    ''' </param>
    ''' <returns>
    ''' NA, NaN, Inf missing value in the matrix will be set
    ''' to the default value zero in the return value of this 
    ''' function
    ''' </returns>
    ''' <remarks>
    ''' #### Standard score(z-score)
    ''' 
    ''' In statistics, the standard score is the signed number of standard deviations by which the value of 
    ''' an observation or data point is above the mean value of what is being observed or measured. Observed 
    ''' values above the mean have positive standard scores, while values below the mean have negative 
    ''' standard scores. The standard score is a dimensionless quantity obtained by subtracting the population 
    ''' mean from an individual raw score and then dividing the difference by the population standard deviation. 
    ''' This conversion process is called standardizing or normalizing (however, "normalizing" can refer to 
    ''' many types of ratios; see normalization for more).
    ''' 
    ''' > https://en.wikipedia.org/wiki/Standard_score
    ''' </remarks>
    <ExportAPI("z")>
    <RApiReturn(GetType(Double))>
    Public Function z_score(<RRawVectorArgument>
                            x As Object,
                            Optional byrow As Boolean = False,
                            Optional env As Environment = Nothing) As Object

        If TypeOf x Is Rdataframe Then
            Dim d As Rdataframe = DirectCast(x, Rdataframe)

            If byrow Then
                Return filterMissing(d.z_scoreByRow, [default]:=0.0, env:=env)
            Else
                Return filterMissing(d.z_scoreByColumn, [default]:=0.0, env:=env)
            End If
        Else
            Dim v As Double() = CLRVector.asNumeric(x)
            Dim z As Double() = New stdVector(v) _
                .Z _
                .ToArray

            Return filterMissing(z, [default]:=0.0, env:=env)
        End If
    End Function

    <Extension>
    Private Function z_scoreByColumn(d As Rdataframe) As Rdataframe
        Dim z As New Rdataframe With {
            .rownames = d.rownames,
            .columns = d.columns _
                .ToDictionary(Function(c) c.Key,
                                Function(c)
                                    Dim dz As Double() = CLRVector.asNumeric(c.Value)
                                    Dim zz As Double() = New stdVector(dz) _
                                        .Z _
                                        .ToArray

                                    Return DirectCast(zz, Array)
                                End Function)
        }

        Return z
    End Function

    <Extension>
    Private Function z_scoreByRow(d As Rdataframe) As Rdataframe
        Dim col As String() = d.colnames
        Dim rows = d.forEachRow(col) _
            .Select(Function(r)
                        Return New NamedCollection(Of Double)(r.name, value:=CLRVector.asNumeric(r.value))
                    End Function) _
            .ToArray
        Dim z = rows _
            .Select(Function(r)
                        Dim zi = New stdVector(r.value) _
                            .Z _
                            .ToArray

                        Return New NamedCollection(Of Double)(r.name, zi)
                    End Function) _
            .ToArray
        Dim dz As New Rdataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = z _
                .Select(Function(i) i.name) _
                .ToArray
        }
        Dim index As Integer

        For i As Integer = 0 To col.Length - 1
            index = i
            dz.columns(col(i)) = z _
                .Select(Function(r) r.value(index)) _
                .ToArray
        Next

        Return dz
    End Function

    ''' <summary>The chiSquare method is used to determine whether there is a significant difference between the expected
    ''' frequencies and the observed frequencies in one or more categories. It takes a double input x and an integer freedom
    ''' for degrees of freedom as inputs. It returns the Chi Squared result.</summary>
    ''' <param name="x">a numeric input.</param>
    ''' <param name="freedom">integer input for degrees of freedom.</param>
    ''' <returns>the Chi Squared result.</returns>
    ''' <remarks>
    ''' ```r
    ''' #' Evaluates the cumulative distribution function (CDF) for a chi-squared 
    ''' #' distribution with degrees of freedom `k` at a value `x`.
    ''' chisquared.cdf = function( x, k ) {
    '''    return gammaCDF(x, k / 2.0, 0.5);
    ''' }
    ''' ```
    ''' </remarks>
    <ExportAPI("chi_square")>
    Public Function ChiSquare(<RRawVectorArgument> x As Object, freedom As Integer, Optional env As Environment = Nothing) As Object
        Return env.EvaluateFramework(Of Double, Double)(x, Function(xi) Distribution.ChiSquare(xi, freedom))
    End Function

    <ExportAPI("gamma.cdf")>
    Public Function gammaCDF(<RRawVectorArgument>
                             x As Object,
                             alpha As Double,
                             beta As Double,
                             Optional env As Environment = Nothing) As Object

        Dim gamma As New Gamma(alpha, beta)
        Dim result = env.EvaluateFramework(Of Double, Double)(x, Function(xi) gamma.GetCDF(xi))

        Return result
    End Function

    <ExportAPI("gamma")>
    Public Function gamma(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Return env.EvaluateFramework(Of Double, Double)(x, Function(xi) SpecialFunctions.gamma(xi))
    End Function

    <ExportAPI("lgamma")>
    Public Function lgamma(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Return env.EvaluateFramework(Of Double, Double)(x, Function(xi) SpecialFunctions.gammaln(xi))
    End Function

    <ExportAPI("beta")>
    Public Function beta(<RRawVectorArgument> a As Object, <RRawVectorArgument> b As Object, Optional env As Environment = Nothing) As Object
        Return Core.BinaryCoreInternal(Of Double, Double, Double)(CLRVector.asNumeric(a), CLRVector.asNumeric(b), Function(x, y, envir) SpecialFunctions.BetaFunction(x, y), env)
    End Function

    <ExportAPI("lbeta")>
    Public Function lbeta(<RRawVectorArgument> a As Object, <RRawVectorArgument> b As Object, Optional env As Environment = Nothing) As Object
        Return Core.BinaryCoreInternal(Of Double, Double, Double)(CLRVector.asNumeric(a), CLRVector.asNumeric(b), Function(x, y, envir) std.Log(SpecialFunctions.BetaFunction(x, y)), env)
    End Function

    ''' <summary>
    ''' check of the outliers via IQR method
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns>
    ''' a numeric vector with outlier data removed, a attribute value which is named ``outliers`` is tagged with
    ''' the generated result numeric vector, you can get the outliers data from this attribute.
    ''' </returns>
    <ExportAPI("iqr_outliers")>
    Public Function iqr_outliers(<RRawVectorArgument> x As Object) As Object
        Dim v = CLRVector.asNumeric(x)

        If v.IsNullOrEmpty Then
            Return Nothing
        End If

        Dim quartile = v.Quartile
        Dim preprocess = quartile.Outlier(v)
        Dim norm As New vec(preprocess.normal)
        norm.setAttribute("outliers", preprocess.outlier)
        Return norm
    End Function

    ''' <summary>
    ''' Fast Poisson Disk Sampling in Arbitrary Dimensions. Robert Bridson. ACM SIGGRAPH 2007
    ''' </summary>
    ''' <param name="minDist">the minimumx distance between any of the two samples.</param>
    ''' <param name="sampleRange">the range of generated samples. From 0[inclusive] to sampleRange[inclusive]</param>
    ''' <param name="k">the time of throw darts. Higher k generate better result but slower.</param>
    ''' <returns></returns>
    <ExportAPI("poisson_disk")>
    Public Function PoissonDiskGenerator_func(Optional minDist As Single = 5.0F,
                                              Optional sampleRange As Single = 256.0F,
                                              Optional k As Integer = 30,
                                              Optional dart As Image = Nothing) As Object

        Dim vx = PoissonDiskGenerator.Generate(minDist, sampleRange, k, dart)
        Dim df As New Rdataframe With {.columns = New Dictionary(Of String, Array)}

        Call df.add("x", vx.Select(Function(vi) vi.x))
        Call df.add("y", vx.Select(Function(vi) vi.y))

        Return df
    End Function

    ''' <summary>
    ''' **Kurtosis** is a statistical measure that describes the "tailedness" of the probability distribution of a
    ''' real-valued random variable. In simpler terms, it indicates the extent to which the tails of the distribution 
    ''' differ from those of a normal distribution.
    ''' 
    ''' ### Key Points about Kurtosis:
    ''' 
    ''' 1. **Definition**:
    ''' 
    '''    - Kurtosis is the fourth standardized moment of a distribution.
    '''    - It is calculated as the average of the squared deviations of the data from its mean, raised to the fourth power, standardized by the standard deviation raised to the fourth power.
    '''    
    ''' 2. **Types of Kurtosis**:
    ''' 
    '''    - **Mesokurtic**: Distributions with kurtosis similar to that of the normal distribution (kurtosis value of 3). The tails of a mesokurtic distribution are neither particularly fat nor particularly thin.
    '''    - **Leptokurtic**: Distributions with positive kurtosis greater than 3. These distributions have "fat tails" and a sharp peak, indicating more frequent large deviations from the mean than a normal distribution.
    '''    - **Platykurtic**: Distributions with kurtosis less than 3. These distributions have "thin tails" and a flatter peak, indicating fewer large deviations from the mean than a normal distribution.
    '''    
    ''' 3. **Excess Kurtosis**:
    ''' 
    '''    - Often, kurtosis is reported as "excess kurtosis," which is the kurtosis value minus 3. This adjustment makes the kurtosis of a normal distribution equal to 0.
    '''    - Positive excess kurtosis indicates a leptokurtic distribution, while negative excess kurtosis indicates a platykurtic distribution.
    '''    
    ''' 4. **Interpretation**:
    ''' 
    '''    - High kurtosis in a data set is an indicator that data has heavy tails or outliers. This can affect the performance of statistical models and methods that assume normality.
    '''    - Low kurtosis indicates that the data has light tails and lacks outliers.
    '''    
    ''' 5. **Applications**:
    ''' 
    '''    - In finance, kurtosis is used to describe the distribution of returns of an investment. A high kurtosis indicates a higher risk of extreme returns.
    '''    - In data analysis, kurtosis helps in understanding the shape of the data distribution and identifying potential outliers.
    '''    
    ''' 6. **Calculation in R**:
    ''' 
    '''    - The `kurtosis()` function in the `e1071` package can be used to calculate kurtosis in R.
    '''    - Alternatively, kurtosis can be calculated manually using the formula:
    '''    
    ''' ```R
    ''' kurtosis &lt;- sum((data - mean(data))^4) / ((length(data) - 1) * sd(data)^4) - 3
    ''' ```
    ''' 
    ''' kurtosis is a statistical measure for understanding the shape of a data distribution, particularly the behavior 
    ''' of its tails. It is widely used in various fields, including finance, data analysis, and statistics.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    ''' <example>
    ''' # Example data
    ''' data &lt;- c(2, 4, 4, 4, 5, 5, 7, 9);
    ''' # Calculate kurtosis using e1071 package
    ''' kurtosis_value &lt;- kurtosis(data);
    ''' print(kurtosis_value);
    ''' 
    ''' # in different algorithm type
    ''' kurtosis(data,type =1);
    ''' # [1] -0.21875
    ''' kurtosis(data,type =2);
    ''' # [1] 0.940625
    ''' kurtosis(data,type =3);
    ''' # [1] -0.8706055
    ''' 
    ''' # Manual calculation of excess kurtosis
    ''' n &lt;- length(data);
    ''' mean_data &lt;- mean(data);
    ''' sd_data &lt;- sd(data);
    ''' kurtosis_manual &lt;- sum((data - mean_data)^4) / ((n - 1) * sd_data^4) - 3;
    ''' print(kurtosis_manual);
    ''' </example>
    ''' <remarks>
    ''' If x contains missings and these are not removed, the kurtosis is NA.
    '''
    ''' Otherwise, write xi for the non-missing elements of x, n for their number, μ for their mean, s for their standard deviation, and 
    ''' mr = ∑i (xi −μ) ^ r /n for the sample moments of order r.
    '''
    ''' Joanes and Gill (1998) discuss three methods for estimating kurtosis:
    '''
    ''' Type 1: g2 = m4/m2 ^ 2 −3. This is the typical definition used in many older textbooks.
    ''' Type 2: G2 = ((n+1)*g2 +6)∗(n−1)/((n−2)(n−3)). Used in SAS and SPSS.
    ''' Type 3: b2 = m4 /s ^ 4 −3 = (g2 +3)(1−1/n) ^ 2 −3. Used in MINITAB and BMDP.
    '''
    ''' Only G2 (corresponding to type = 2) is unbiased under normality.
    ''' </remarks>
    <ExportAPI("kurtosis")>
    Public Function kurtosis(<RRawVectorArgument> x As Object, Optional type As AlgorithmType = AlgorithmType.Classical) As Object
        Return CLRVector.asNumeric(x).Kurtosis(type)
    End Function

    ''' <summary>
    ''' **Skewness**
    ''' 
    ''' Skewness is a fundamental statistical measure used to describe the asymmetry of the probability distribution of a 
    ''' real-valued random variable. It provides insights into the direction and extent of the deviation from a symmetric 
    ''' distribution.
    ''' 
    ''' ### Key Aspects of Skewness:
    ''' 
    ''' 1. **Definition**:
    ''' 
    '''    - Skewness is the third standardized moment of a distribution.
    '''    - It is calculated as the average of the cubed deviations of the data from its mean, standardized by the standard deviation raised to the third power.
    '''    
    ''' 2. **Types of Skewness**:
    ''' 
    '''    - **Zero Skewness**: Indicates a symmetric distribution where the mean, median, and mode are all equal.
    '''    - **Positive Skewness (Right-Skewed)**: The tail on the right side of the distribution is longer or fatter. In this case, the mean is greater than the median.
    '''    - **Negative Skewness (Left-Skewed)**: The tail on the left side of the distribution is longer or fatter. Here, the mean is less than the median.
    '''    
    ''' 3. **Interpretation**:
    ''' 
    '''    - Skewness values close to zero suggest a nearly symmetric distribution.
    '''    - Positive values indicate right-skewed distributions, while negative values indicate left-skewed distributions.
    '''    - The magnitude of the skewness value reflects the degree of asymmetry.
    '''    
    ''' 4. **Applications**:
    ''' 
    '''    - **Finance**: Used to analyze the distribution of returns on investments, helping investors understand the potential for extreme outcomes.
    '''    - **Economics**: Assists in examining income distributions, enabling economists to assess income inequality.
    '''    - **Natural Sciences**: Describes the distribution of experimental data in scientific research.
    '''    
    ''' 5. **Considerations**:
    ''' 
    '''    - Skewness is just one aspect of distribution shape and should be considered alongside other statistical measures like kurtosis for a comprehensive understanding.
    '''    - For small sample sizes, the estimation of skewness can be unreliable.
    '''    
    ''' In essence, skewness is a statistical tool for understanding the asymmetry of data distributions, 
    ''' with wide-ranging applications in various fields such as finance, economics, and the natural 
    ''' sciences.
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    ''' <example>
    ''' # Example data
    ''' data &lt;- c(2, 4, 4, 4, 5, 5, 7, 9);
    ''' # Calculate skewness using e1071 package
    ''' skewness_value &lt;- skewness(data);
    ''' print(skewness_value);
    ''' 
    ''' skewness(data,type = 1);
    ''' # [1] 0.65625
    ''' skewness(data,type = 2);
    ''' # [1] 0.8184876
    ''' skewness(data,type = 3);
    ''' # [1] 0.5371325
    ''' 
    ''' # Manual calculation of skewness
    ''' n &lt;- length(data);
    ''' mean_data &lt;- mean(data);
    ''' sd_data &lt;- sd(data);
    ''' skewness_manual &lt;- sum((data - mean_data)^3) / ((n - 1) * sd_data^3);
    ''' print(skewness_manual);
    ''' </example>
    ''' <remarks>
    ''' If x contains missings and these are not removed, the skewness is NA.
    ''' Otherwise, write xi for the non-missing elements of x, n for their number, μ for their mean, s for their standard deviation, and 
    ''' mr =∑i (xi −μ) ^ r /n for the sample moments of order r.
    '''
    ''' Joanes and Gill (1998) discuss three methods for estimating skewness:
    '''
    ''' Type 1: g1 = m3 / m2 ^ (3/2). This is the typical definition used in many older textbooks.
    ''' Type 2: G1 = g1 * sqrt(n(n−1)) /(n−2). Used in SAS and SPSS.
    ''' Type 3: b1 = m3 /s^3 = g1 * ((n−1)/n) ^ (3/2) . Used in MINITAB and BMDP.
    '''
    ''' All three skewness measures are unbiased under normality.
    ''' </remarks>
    <ExportAPI("skewness")>
    Public Function skewness(<RRawVectorArgument> x As Object, Optional type As AlgorithmType = AlgorithmType.Classical) As Object
        Return CLRVector.asNumeric(x).Skewness(type)
    End Function

    ''' <summary>
    ''' In statistics, moments are a set of numerical characteristics that describe the shape and features of a probability distribution. 
    ''' Sample moments are the same concept applied to a sample of data, rather than an entire population. They are used to estimate 
    ''' the corresponding population moments and to understand the properties of the data distribution.
    ''' 
    ''' Here's a basic introduction to the concept of sample moments:
    ''' 
    ''' ### Definition:
    ''' 
    ''' 1. **Sample Mean (First Moment):**
    '''    The sample mean is the average of the data points in a sample. It is a measure of the central tendency of the data.
    '''      \[
    '''      \bar{x} = \frac{1}{n} \sum_{i=1}^{n} x_i
    '''      \]
    '''    where \( x_i \) are the data points and \( n \) is the number of data points in the sample.
    ''' 2. **Sample Variance (Second Central Moment):**
    '''    The sample variance measures the spread or dispersion of the data points around the sample mean.
    '''      \[
    '''      s^2 = \frac{1}{n-1} \sum_{i=1}^{n} (x_i - \bar{x})^2
    '''      \]
    '''    The denominator \( n-1 \) is used instead of \( n \) to provide an unbiased estimate of the population variance.
    ''' 3. **Sample Standard Deviation:**
    '''    The sample standard deviation is the square root of the sample variance and is also a measure of dispersion.
    '''      \[
    '''      s = \sqrt{s^2}
    '''      \]
    ''' 4. **Higher-Order Sample Moments:**
    '''    Higher-order moments describe the shape of the distribution. For example:
    '''    - **Third Moment:** Measures skewness, which indicates the asymmetry of the data distribution.
    '''    - **Fourth Moment:** Measures kurtosis, which indicates the "tailedness" of the data distribution.
    '''    
    ''' ### Calculation:
    ''' 
    ''' To calculate sample moments, you simply apply the formulas to your data set. For instance, to find the sample mean,
    ''' you add up all the data points and divide by the number of points.
    ''' 
    ''' ### Use:
    ''' 
    ''' Sample moments are used to:
    ''' - Estimate population parameters.
    ''' - Assess the shape of the data distribution (e.g., normality, skewness, kurtosis).
    ''' - Form the basis for many statistical tests and procedures.
    ''' 
    ''' ### Properties:
    ''' - **Unbiasedness:** Some sample moments are designed to be unbiased estimators, meaning that the expected value of the sample moment equals the population moment.
    ''' - **Efficiency:** Different sample moments may have different levels of variability; some are more efficient than others.
    ''' - **Robustness:** Certain moments are more robust to outliers than others.
    ''' 
    ''' ### Example:
    ''' If you have a sample of data: \( \{2, 4, 4, 4, 5, 5, 7, 9\} \), you can calculate the sample mean, variance, 
    ''' and other moments to understand the central tendency, dispersion, and shape of the data distribution.
    ''' 
    ''' sample moments are fundamental tools in statistics for summarizing and understanding the characteristics of
    ''' a data set. They provide a way to quantify features such as location, spread, and shape, which are essential 
    ''' for further statistical analysis.
    ''' 
    ''' @author Will_and_Sara
    ''' </summary>
    ''' <remarks>
    ''' 
    ''' </remarks>
    <ExportAPI("product_moments")>
    Public Function ProductMoments(<RRawVectorArgument> x As Object) As ProductMoments
        Return New ProductMoments(CLRVector.asNumeric(x))
    End Function

    ''' <summary>
    ''' ### Statistical Moments
    ''' 
    ''' This function computes the sample moment of specified order.
    ''' </summary>
    ''' <param name="x">a numeric vector of data.</param>
    ''' <param name="order">order of the moment to be computed</param>
    ''' <param name="central">a logical value - if central moments are to be computed.</param>
    ''' <param name="absolute">a logical value - if absolute moments are to be computed.</param>
    ''' <param name="na_rm">a logical value - remove NA values?</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <example>
    ''' let data = c(2, 4, 4, 4, 5, 5, 7, 9);
    ''' 
    ''' moment(data, 2);
    ''' # [1]  29
    ''' moment(data, 2, central=TRUE);
    ''' # [1]  4
    ''' 
    ''' moment(data, 3);
    ''' # [1]  190.25
    ''' moment(data, 3, central=TRUE);
    ''' # [1]  5.25
    ''' </example>
    <ExportAPI("moment")>
    Public Function moment(<RRawVectorArgument> x As Object,
                           Optional order As Integer = 1,
                           Optional central As Boolean = False,
                           Optional absolute As Boolean = False,
                           Optional na_rm As Boolean = False,
                           Optional env As Environment = Nothing) As Object

        Dim data As Double() = CLRVector.asNumeric(x)
        Dim sample As New ProductMoments(If(na_rm, data.Where(Function(xi) Not xi.IsNaNImaginary).ToArray, data))

        If central Then
            Return sample.CentralMoment(order)
        End If
        If absolute Then
            Return sample.AbsoluteMoment(order)
        End If

        Return sample.Moment(order)
    End Function

    ''' <summary>
    ''' ### Earth Mover's Distance
    ''' 
    ''' Implementation of the Fast Earth Mover's Algorithm by Ofir Pele and Michael Werman.
    ''' </summary>
    ''' <param name="x">the sparse matrices being compared</param>
    ''' <param name="y">the sparse matrices being compared</param>
    ''' <param name="bins"></param>
    ''' <param name="extra_mass_penalty">
    ''' penalty for extra mass. 0 for no penalty, -1 for the default, other positive values to specify the penalty;
    ''' An extraMassPenalty of -1 means that the extra mass penalty is the maximum distance found between two features.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("emd_dist")>
    <RApiReturn(GetType(Double))>
    Public Function emd_dist(<RRawVectorArgument> x As Object,
                             <RRawVectorArgument> y As Object,
                             Optional bins As Integer = 10,
                             Optional extra_mass_penalty As Double = -1,
                             Optional env As Environment = Nothing) As Object

        Dim v1 = CLRVector.asNumeric(x)
        Dim v2 = CLRVector.asNumeric(y)

        Return EMD.emdDist(v1, v2, bins, extra_mass_penalty)
    End Function
End Module

Public Enum SplineAlgorithms
    BSpline
    CubiSpline
    CatmullRom
    Bezier
End Enum
