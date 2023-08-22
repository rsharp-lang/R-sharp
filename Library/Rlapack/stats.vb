#Region "Microsoft.VisualBasic::060bab263ecbb7435de35e47e1416dd2, D:/GCModeller/src/R-sharp/Library/Rlapack//stats.vb"

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

'   Total Lines: 1274
'    Code Lines: 716
' Comment Lines: 423
'   Blank Lines: 135
'     File Size: 52.27 KB


' Module stats
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: matrixDataFrame, matrixDataFrame2, PCATable, printMatrix, printTtest
'               printTwoSampleTTest
'     Enum p_adjust_methods
' 
'         BH, bonferroni, BY, fdr, hochberg
'         holm, hommel, none
' 
' 
' 
'     Class PCAcalls
' 
'         Properties: labels, pca
' 
'         Function: ECDF, p_adjust, prcomp, spline, tabulateMode
' 
'  
' 
'     Function: aov, asDist, ChiSquare, corr, corr_sign
'               corrTest, dataframeRow, dist, dnorm, filterMissing
'               fisher_test, gammaCDF, getMatrix, getQuantileLevels, mantel_test
'               median, mul, (+2 Overloads) pow, quantile, ttest
'               ttestBatch, ttestImpl, varTest, z_score, z_scoreByColumn
'               z_scoreByRow
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
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Prcomp
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Math.Statistics
Imports Microsoft.VisualBasic.Math.Statistics.Distributions
Imports Microsoft.VisualBasic.Math.Statistics.Distributions.MethodOfMoments
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.FishersExact
Imports Microsoft.VisualBasic.Math.Statistics.Linq
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
Imports stdNum = System.Math
Imports stdVector = Microsoft.VisualBasic.Math.LinearAlgebra.Vector

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
        Internal.ConsolePrinter.AttachConsoleFormatter(Of DistanceMatrix)(AddressOf printMatrix)
        Internal.ConsolePrinter.AttachConsoleFormatter(Of TtestResult)(AddressOf printTtest)
        Internal.ConsolePrinter.AttachConsoleFormatter(Of TwoSampleResult)(AddressOf printTwoSampleTTest)
        Internal.ConsolePrinter.AttachConsoleFormatter(Of FishersExactPvalues)(Function(o) o.ToString)
        Internal.ConsolePrinter.AttachConsoleFormatter(Of FTest)(Function(o) o.ToString)

        Internal.Object.Converts.makeDataframe.addHandler(GetType(DataMatrix), AddressOf matrixDataFrame)
        Internal.Object.Converts.makeDataframe.addHandler(GetType(DistanceMatrix), AddressOf matrixDataFrame)
        Internal.Object.Converts.makeDataframe.addHandler(GetType(CorrelationMatrix), AddressOf matrixDataFrame2)
        Internal.Object.Converts.makeDataframe.addHandler(GetType(PCAcalls), AddressOf PCATable)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="args">
    ''' npc = i32
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Private Function PCATable(x As PCAcalls, args As list, env As Environment) As Rdataframe
        Dim npc As Integer = args.getValue("npc", env, 2)
        Dim data As New Rdataframe With {
            .rownames = x.labels,
            .columns = New Dictionary(Of String, Array)
        }
        Dim components = x.pca.Project(npc)

        For i As Integer = 0 To npc - 1
            Dim index As Integer = i
            Dim name As String = $"dim{i + 1}"

            Call data.add(name, components.Select(Function(r) r(index)))
        Next

        Return data
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

            If stdNum.Abs(cori) > cutoff AndAlso pvali < pvalue_cut Then
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
                Return Internal.debug.stop(New NotImplementedException(method.Description), env)
        End Select
    End Function

    ''' <summary>
    ''' ## Empirical Cumulative Distribution Function
    ''' 
    ''' Compute an empirical cumulative distribution function
    ''' </summary>
    ''' <param name="FUNC"></param>
    ''' <returns></returns>
    <ExportAPI("CDF")>
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

        Return Internal.debug.stop($"unsupported spline algorithm: {algorithm.ToString}", env)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("tabulate.mode")>
    Public Function tabulateMode(<RRawVectorArgument> x As Object) As Double
        Return CLRVector.asNumeric(x) _
            .DoCall(Function(vec)
                        Return Bootstraping.TabulateMode(DirectCast(vec, Double()))
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
    <RApiReturn(GetType(PCA))>
    Public Function prcomp(<RRawVectorArgument>
                           x As Object,
                           Optional scale As Boolean = False,
                           Optional center As Boolean = False,
                           Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Internal.debug.stop("'data' must be of a vector type, was 'NULL'", env)
        End If

        Dim matrix As Double()()
        Dim labels As String()

        If TypeOf x Is Rdataframe Then
            With DirectCast(x, Rdataframe)
                matrix = .nrows _
                    .Sequence _
                    .Select(Function(i)
                                Return CLRVector.asNumeric(.getRowList(i, drop:=False))
                            End Function) _
                    .Select(Function(v) DirectCast(v, Double())) _
                    .ToArray
                labels = .getRowNames
            End With
        Else
            Throw New NotImplementedException
        End If

        Dim PCA As New PCA(matrix, center, scale)
        Dim calls As New PCAcalls With {
            .labels = labels,
            .pca = PCA
        }

        Return calls
    End Function

    Public Class PCAcalls

        Public Property pca As PCA
        Public Property labels As String()

    End Class

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
    ''' <returns></returns>
    <ExportAPI("corr")>
    <RApiReturn(GetType(CorrelationMatrix))>
    Public Function corr(x As Rdataframe, Optional y As Rdataframe = Nothing, Optional spearman As Boolean = False) As Object
        If y Is Nothing Then
            Dim rows As DataSet() = x.getRowNames _
                .Select(Function(id, index)
                            Return x.dataframeRow(Of Double, DataSet)(id, index)
                        End Function) _
                .ToArray
            Dim cor As CorrelationMatrix = rows.Correlation(spearman)

            Return cor
        Else
            Throw New NotImplementedException
        End If
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

    <ExportAPI("dnorm")>
    Public Function dnorm(x As Double(), Optional mean As Double = 0, Optional sd As Double = 1) As Object
        Return Distributions.pnorm.ProbabilityDensity(x.AsVector, mean, sd).Array
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
            Return Internal.debug.stop(New InvalidCastException(x.GetType.FullName), env)
        End If

        Select Case Strings.LCase(method)
            Case "euclidean"
                Return raw.Euclidean
            Case Else
                Return Internal.debug.stop(New NotImplementedException(method), env)
        End Select
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
            Return alternative.ttestImpl(x, y, mu, conf_level, var_equal)
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
        Dim v As New Dictionary(Of String, Double())
        Dim ref As Symbol

        For Each name As String In symbols
            If table.hasName(name) Then
                v.Add(name, CLRVector.asNumeric(table.getColumnVector(name)))
            Else
                ref = env.FindSymbol(name)

                If ref Is Nothing Then
                    Return Internal.debug.stop($"missing required symbol '{name}' for evaluate formula!", env)
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
            x = symbols.Select(Function(r) v(r)(idx)).ToArray
            x = alternative.ttestImpl(x, y, mu, conf_level, var_equal)

            If Program.isException(x) Then
                Return x
            Else
                test(i) = x
            End If
        Next

        Return test
    End Function

    <Extension>
    Private Function ttestImpl(alternative As Hypothesis,
                               x As Object, y As Object,
                               mu As Double,
                               conf_level As Double,
                               var_equal As Boolean) As Object
        Dim test As Object
        Dim vx As Double() = CLRVector.asNumeric(x)
        Dim vy As Double()

        If vx.Length > 0 Then
            vx(Scan0) += 0.00001
        End If

        If y Is Nothing Then
            test = Statistics.Hypothesis.t.Test(vx, alternative, mu, alpha:=1 - conf_level)
        Else
            vy = CLRVector.asNumeric(y)
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

    Private Function getMatrix(x As Object, env As Environment, tag As String, ByRef err As Message) As Double()()
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
            Return Internal.debug.stop("", env)
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
            Dim vec As String = formula.var
            Dim factor As Expression = formula.formula

            If TypeOf factor Is SymbolReference Then
                Dim factorName As String = DirectCast(factor, SymbolReference).symbol
                Dim factors As String() = CLRVector.asCharacter(x.getColumnVector(factorName))
                Dim data As Double() = CLRVector.asNumeric(x.getColumnVector(vec))
                Dim groups = factors.Select(Function(k, i) (k, data(i))).GroupBy(Function(i) i.k).ToArray

                For Each group In groups
                    Call observations.Add(group.Select(Function(i) i.Item2).ToArray)
                Next
            Else
                Return Internal.debug.stop($"not implemented for the {factor.GetType.FullName}!", env)
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
        Call output.add("Residual standard error", stdNum.Sqrt(anova.SSW))
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
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("plsda")>
    Public Function plsda(x As Rdataframe, <RRawVectorArgument> y As Object,
                          Optional ncomp As Integer? = Nothing,
                          Optional center As Boolean = True,
                          Optional scale As Boolean = False,
                          Optional env As Environment = Nothing) As Object

        Dim xm As Double()() = x.forEachRow _
            .Select(Function(ci) CLRVector.asNumeric(ci.value)) _
            .ToArray
        Dim ylabels As String() = Nothing
        Dim yfactors As factor = Nothing
        Dim yval As Double()

        If y Is Nothing Then
            Return Internal.debug.stop("the sample class information should not be nothing, it must be a vector of numeric data for regression or a character vector for classification!", env)
        Else
            y = REnv.TryCastGenericArray(y, env)
        End If

        If DataFramework.IsNumericCollection(y.GetType) Then
            ' regression
            yval = CLRVector.asNumeric(y)
        Else
            ylabels = CLRVector.asCharacter(y)
            yfactors = factor.CreateFactor(ylabels)
            yval = yfactors.asNumeric(ylabels)
        End If

        Dim ds = New StatisticsObject(xm, yval)
        Dim pls_mvar = PLS.PartialLeastSquares(ds, component:=If(ncomp, -1))

        Return pls_mvar
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
End Module

Public Enum SplineAlgorithms
    BSpline
    CubiSpline
    CatmullRom
    Bezier
End Enum
