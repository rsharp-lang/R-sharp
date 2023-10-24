#Region "Microsoft.VisualBasic::06d336d68cd79b34ceaf4375649bfc5c, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/Math/math.vb"

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

'   Total Lines: 944
'    Code Lines: 434
' Comment Lines: 418
'   Blank Lines: 92
'     File Size: 40.47 KB


'     Module math
' 
'         Function: abs, cluster1D, cor_test, cos, diff
'                   exp, fit, getRandom, isFinite, isInfinite
'                   isNaN, log, log10, log2, max
'                   mean, median, min, numericClassTags, pearson
'                   pow, rnorm, round, rsd, runif
'                   (+2 Overloads) sample, sample_int, sd, (+3 Overloads) shuffle, sin
'                   sqrt, sum, var
' 
'         Sub: set_seed
'         Class corTestResult
' 
'             Properties: cor, df, prob2, pvalue, t
'                         z
' 
'             Function: ToString
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports REnv = SMRUCC.Rsharp.Runtime
Imports stdNum = System.Math

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' R# math module
    ''' </summary>
    Module math

        ''' <summary>
        ''' ## Finite, Infinite and NaN Numbers
        ''' 
        ''' is.finite and is.infinite return a vector of the same 
        ''' length as x, indicating which elements are finite 
        ''' (not infinite and not missing) or infinite.
        ''' 
        ''' Inf And -Inf are positive And negative infinity whereas
        ''' NaN means 'Not a Number’. (These apply to numeric values 
        ''' and real and imaginary parts of complex values but not 
        ''' to values of integer vectors.) Inf and NaN are reserved 
        ''' words in the R language.
        ''' </summary>
        ''' <param name="x">
        ''' R object to be tested: the default methods handle atomic 
        ''' vectors.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A logical vector of the same length as x: dim, dimnames 
        ''' and names attributes are preserved.
        ''' </returns>
        ''' <remarks>
        ''' is.nan tests if a numeric value is NaN. Do not test equality 
        ''' to NaN, or even use identical, since systems typically have
        ''' many different NaN values. One of these is used for the 
        ''' numeric missing value NA, and is.nan is false for that value. 
        ''' A complex number is regarded as NaN if either the real or 
        ''' imaginary part is NaN but not NA. All elements of logical, 
        ''' integer and raw vectors are considered not to be NaN.
        '''
        ''' All three functions accept NULL As input And Return a length 
        ''' zero result. The Default methods accept character And raw vectors, 
        ''' And Return False For all entries. Prior To R version 2.14.0 
        ''' they accepted all input, returning False For most non-numeric 
        ''' values; cases which are Not atomic vectors are now signalled 
        ''' As errors.
        '''
        ''' All three functions are generic: you can write methods To handle 
        ''' specific classes Of objects, see InternalMethods.
        ''' </remarks>
        <ExportAPI("is.nan")>
        Public Function isNaN(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Return EvaluateFramework(Of Double, Boolean)(env, x, eval:=Function(xi) xi.IsNaNImaginary)
        End Function

        <ExportAPI("fit")>
        Public Function fit(x As Object, <RListObjectArgument> args As list, Optional env As Environment = Nothing) As Object
            Return generic.invokeGeneric(args, x, env, funcName:="fit")
        End Function

        ''' <summary>
        ''' ## Finite, Infinite and NaN Numbers
        ''' 
        ''' is.finite and is.infinite return a vector of the same length 
        ''' as x, indicating which elements are finite (not infinite and 
        ''' not missing) or infinite.
        ''' </summary>
        ''' <param name="x">
        ''' R object to be tested: the default methods handle atomic vectors.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A logical vector of the same length as x: dim, dimnames and 
        ''' names attributes are preserved.
        ''' </returns>
        <ExportAPI("is.finite")>
        Public Function isFinite(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Return EvaluateFramework(Of Double, Boolean)(env, x, eval:=Function(xi) Not Double.IsInfinity(xi))
        End Function

        ''' <summary>
        ''' ## Finite, Infinite and NaN Numbers
        ''' 
        ''' is.finite and is.infinite return a vector of the same length as 
        ''' x, indicating which elements are finite (not infinite and not 
        ''' missing) or infinite.
        ''' </summary>
        ''' <param name="x">
        ''' R object to be tested: the default methods handle atomic vectors.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A logical vector of the same length as x: dim, dimnames and names 
        ''' attributes are preserved.
        ''' </returns>
        ''' <remarks>
        ''' is.infinite returns a vector of the same length as x the jth 
        ''' element of which is TRUE if x[j] is infinite (i.e., equal to one
        ''' of Inf or -Inf) and FALSE otherwise. This will be false unless x 
        ''' is numeric or complex. Complex numbers are infinite if either the
        ''' real or the imaginary part is.
        ''' </remarks>
        <ExportAPI("is.infinite")>
        Public Function isInfinite(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Return EvaluateFramework(Of Double, Boolean)(env, x, eval:=Function(xi) Double.IsInfinity(xi))
        End Function

        ''' <summary>
        ''' ### Lagged Differences
        ''' 
        ''' Returns suitably lagged and iterated differences.
        ''' </summary>
        ''' <param name="x">
        ''' a numeric vector Or matrix containing the values To be differenced.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' is.finite returns a vector of the same length as x the jth 
        ''' element of which is TRUE if x[j] is finite (i.e., it is not 
        ''' one of the values NA, NaN, Inf or -Inf) and FALSE otherwise. 
        ''' Complex numbers are finite if both the real and imaginary 
        ''' parts are.
        ''' </remarks>
        <ExportAPI("diff")>
        Public Function diff(<RRawVectorArgument> x As Object) As Double()
            Return NumberGroups.diff(CLRVector.asNumeric(x))
        End Function

        ''' <summary>
        ''' rounds the values in its first argument to the specified number of decimal places (default 0). 
        ''' See *'Details'* about "round to even" when rounding off a 5.
        ''' </summary>
        ''' <param name="x">a numeric vector. Or, for ``round`` and ``signif``, a complex vector.</param>
        ''' <param name="decimals">
        ''' integer indicating the number of decimal places (``round``) or significant digits (``signif``) to be used. 
        ''' Negative values are allowed (see *'Details'*).
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("round")>
        Public Function round(<RRawVectorArgument> x As Object, Optional decimals% = 0) As Double()
            Dim rounds = From element As Double
                         In CLRVector.asNumeric(x)
                         Select stdNum.Round(element, decimals)

            Return rounds.ToArray
        End Function

        ''' <summary>
        ''' computes logarithms, by default natural logarithms, log10 computes common (i.e., base 10) logarithms, 
        ''' and log2 computes binary (i.e., base 2) logarithms. 
        ''' The general form log(x, base) computes logarithms with base base.
        ''' </summary>
        ''' <param name="x">a numeric or complex vector.</param>
        ''' <param name="newBase">
        ''' a positive or complex number: the base with respect to which logarithms are computed. 
        ''' Defaults to ``e=exp(1)``.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("log")>
        Public Function log(<RRawVectorArgument> x As Object, Optional newBase As Double = stdNum.E) As Double()
            Return CLRVector.asNumeric(x) _
                .Select(Function(d) stdNum.Log(d, newBase)) _
                .ToArray
        End Function

        ''' <summary>
        ''' ### Logarithms and Exponentials
        ''' 
        ''' log2 computes binary (i.e., base 2) logarithms. 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("log2")>
        Public Function log2(<RRawVectorArgument> x As Object) As Double()
            Return CLRVector.asNumeric(x) _
                .Select(Function(d) stdNum.Log(d, 2)) _
                .ToArray
        End Function

        ''' <summary>
        ''' ### Logarithms and Exponentials
        ''' 
        ''' log10 computes common (i.e., base 10) logarithms
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("log10")>
        Public Function log10(<RRawVectorArgument> x As Object) As Double()
            Return CLRVector.asNumeric(x) _
                .Select(Function(d) stdNum.Log(d, 10)) _
                .ToArray
        End Function

        <ExportAPI("sin")>
        Public Function sin(<RRawVectorArgument> x As Object) As Double()
            Return CLRVector.asNumeric(x) _
                .Select(Function(d) stdNum.Sin(d)) _
                .ToArray
        End Function

        ''' <summary>
        ''' evaluate the cosine alpha
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("cos")>
        Public Function cos(<RRawVectorArgument> x As Object) As Double()
            Return CLRVector.asNumeric(x) _
                .Select(Function(d) stdNum.Cos(d)) _
                .ToArray
        End Function

        ''' <summary>
        ''' Product of Vector Elements
        ''' 
        ''' prod returns the product of all the values present in its arguments.
        ''' </summary>
        ''' <param name="x">numeric or complex or logical vectors.</param>
        ''' <param name="na_rm">
        ''' logical. Should missing values be removed?
        ''' </param>
        ''' <returns>The product, a numeric (of type "double") or complex vector of length one. NB: the product of an empty set is one, by definition.</returns>
        ''' <remarks>
        ''' If na.rm is FALSE an NA value in any of the arguments will cause a value of NA to be returned, otherwise NA values are ignored.
        ''' This is a generic function: methods can be defined for it directly or via the Summary group generic. For this to work properly, the arguments ... should be unnamed, and dispatch is on the first argument.
        ''' Logical true values are regarded as one, false values as zero. For historical reasons, NULL is accepted and treated as if it were numeric(0).
        ''' </remarks>
        <ExportAPI("prod")>
        Public Function prod(<RRawVectorArgument> x As Object, Optional na_rm As Boolean = False) As Double
            Dim vx As Double() = CLRVector.asNumeric(x)
            Dim p As Double = 1

            For Each xi As Double In vx
                If xi.IsNaNImaginary Then
                    If na_rm Then
                        Continue For
                    Else
                        Return Double.NaN
                    End If
                End If

                p = p * xi
            Next

            Return p
        End Function

        ''' <summary>
        ''' #### Sum of Vector Elements
        ''' 
        ''' sum returns the sum of all the values present in its arguments.
        ''' </summary>
        ''' <param name="x">numeric or complex or logical vectors.</param>
        ''' <returns></returns>
        <ExportAPI("sum")>
        Public Function sum(<RRawVectorArgument> x As Object, Optional narm As Boolean = False) As Double
            If x Is Nothing Then
                Return 0
            End If

            Dim array = REnv.asVector(Of Object)(x)
            Dim elementType As Type = Runtime.MeasureArrayElementType(array)

            Select Case elementType
                Case GetType(Boolean)
                    Return CLRVector.asLogical(array).Select(Function(b) If(b, 1, 0)).Sum
                Case GetType(Integer), GetType(Long), GetType(Short), GetType(Byte)
                    Return CLRVector.asLong(x).Sum
                Case Else
                    Return CLRVector.asNumeric(x).Sum
            End Select
        End Function

        <ExportAPI("pow")>
        Public Function pow(x As Array, y As Array, Optional env As Environment = Nothing) As Object
            x = CLRVector.asNumeric(x)
            y = CLRVector.asNumeric(y)

            Return Vectorization.Exponent.f64_op_exponent_f64(x, y, env)
        End Function

        <ExportAPI("sqrt")>
        Public Function sqrt(x As Array) As Double()
            Return CLRVector.asNumeric(x) _
                .Select(AddressOf stdNum.Sqrt) _
                .ToArray
        End Function

        ''' <summary>
        ''' #### Logarithms and Exponentials
        ''' 
        ''' computes the exponential function.
        ''' </summary>
        ''' <param name="x">a numeric or complex vector.</param>
        ''' <returns></returns>
        <ExportAPI("exp")>
        Public Function exp(x As Array) As Double()
            Return CLRVector.asNumeric(x) _
                .Select(AddressOf stdNum.Exp) _
                .ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="na_rm">a logical indicating whether missing values should be removed.</param>
        ''' <returns></returns>
        <ExportAPI("max")>
        Public Function max(<RRawVectorArgument>
                            x As Object,
                            Optional na_rm As Boolean = False,
                            Optional env As Environment = Nothing) As Object

            If TypeOf x Is list Then
                Return Internal.debug.stop("Error in max(x) : invalid 'type' (list) of argument", env)
            End If

            Dim dbl = CLRVector.asNumeric(x)

            If dbl.Length = 0 Then
                Call env.AddMessage({"no non-missing arguments to max; returning -Inf"}, MSG_TYPES.WRN)
                Return Double.NegativeInfinity
            Else
                Return dbl.Max
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="na_rm">
        ''' a logical indicating whether missing values should be removed.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("min")>
        Public Function min(<RRawVectorArgument> x As Object,
                            Optional na_rm As Boolean = False,
                            Optional env As Environment = Nothing) As Object

            If TypeOf x Is list Then
                Return Internal.debug.stop("Error in min(x) : invalid 'type' (list) of argument", env)
            End If

            Dim dbl = CLRVector.asNumeric(x)

            If dbl.Length = 0 Then
                Call env.AddMessage({"no non-missing arguments to min; returning Inf"}, MSG_TYPES.WRN)
                Return Double.PositiveInfinity
            Else
                Return dbl.Min
            End If
        End Function

        ''' <summary>
        ''' Arithmetic Mean
        ''' </summary>
        ''' <param name="x">An R object. Currently there are methods for numeric/logical 
        ''' vectors and date, date-time and time interval objects. Complex vectors are 
        ''' allowed for trim = 0, only.</param>
        ''' <returns></returns>
        <ExportAPI("mean")>
        Public Function mean(<RRawVectorArgument> x As Object, Optional na_rm As Boolean = False) As Double
            Dim array As Double() = CLRVector.asNumeric(x)

            If array.IsNullOrEmpty Then
                Return 0
            End If

            If na_rm Then
                Return array.Where(Function(a) Not a.IsNaNImaginary).Average
            Else
                Return array.Average
            End If
        End Function

        ''' <summary>
        ''' ### Median Value
        ''' 
        ''' Compute the sample median.
        ''' </summary>
        ''' <param name="x">an object for which a method has been defined, 
        ''' or a numeric vector containing the values whose median is to 
        ''' be computed.</param>
        ''' <param name="na_rm">
        ''' a logical value indicating whether NA values should be stripped 
        ''' before the computation proceeds.
        ''' </param>
        ''' <returns>
        ''' The default method returns a length-one object of the same type 
        ''' as x, except when x is logical or integer of even length, when 
        ''' the result will be double.
        ''' 
        ''' If there are no values Or If na.rm = False And there are NA 
        ''' values the result Is NA Of the same type As x (Or more generally 
        ''' the result Of x[FALSE][NA]).
        ''' </returns>
        ''' <remarks>
        ''' This is a generic function for which methods can be written. 
        ''' However, the default method makes use of is.na, sort and mean 
        ''' from package base all of which are generic, and so the default 
        ''' method will work for most classes (e.g., "Date") for which 
        ''' a median is a reasonable concept.
        ''' </remarks>
        <ExportAPI("median")>
        Public Function median(<RRawVectorArgument> x As Object, Optional na_rm As Boolean = False) As Double
            Dim array As Double() = CLRVector.asNumeric(x)

            If array.IsNullOrEmpty Then
                Return 0
            End If

            If na_rm Then
                Return array.Where(Function(a) Not a.IsNaNImaginary).Median
            Else
                Return array.Median
            End If
        End Function

        ''' <summary>
        ''' abs(x) computes the absolute value of x
        ''' </summary>
        ''' <param name="x">a numeric Or complex vector Or array.</param>
        ''' <returns></returns>
        <ExportAPI("abs")>
        Public Function abs(<RRawVectorArgument> x As Object) As Double()
            Return CLRVector.asNumeric(x) _
                .Select(AddressOf stdNum.Abs) _
                .ToArray
        End Function

        ''' <summary>
        ''' ## relative standard deviation
        ''' 
        ''' Relative standard deviation is a common formula 
        ''' used in statistics and probability theory to determine
        ''' a standardized measure of the ratio of the standard
        ''' deviation to the mean. This formula is useful in
        ''' various situations including when comparing your 
        ''' own data to other related data and in financial 
        ''' settings such as the stock market.
        ''' 
        ''' Relative standard deviation, which also may be referred 
        ''' to as RSD or the coefficient of variation, is used 
        ''' to determine if the standard deviation of a set of 
        ''' data is small or large when compared to the mean.
        ''' In other words, the relative standard deviation can
        ''' tell you how precise the average of your results is.
        ''' This formula is most frequently used in chemistry, 
        ''' statistics and other math-related settings but can 
        ''' also be used in the business world when assessing
        ''' finances and the stock market.
        ''' 
        ''' The relative standard deviation Of a Set Of data can be
        ''' depicted As either a percentage Or As a number. The 
        ''' higher the relative standard deviation, the more spread 
        ''' out the results are from the mean Of the data. On the
        ''' other hand, a lower relative standard deviation means 
        ''' that the measurement Of data Is more precise.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("rsd")>
        Public Function rsd(<RRawVectorArgument> x As Object) As Double
            Return CLRVector.asNumeric(x).RSD
        End Function

        ''' <summary>
        ''' ### Standard Deviation
        ''' 
        ''' This function computes the standard deviation of the values in x. 
        ''' If na.rm is TRUE then missing values are removed before computation 
        ''' proceeds.
        ''' </summary>
        ''' <param name="x">
        ''' a numeric vector or an R object but not a factor coercible to numeric by as.double(x)
        ''' </param>
        ''' <param name="sample">
        ''' sample or population
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("sd")>
        Public Function sd(<RRawVectorArgument> x As Object, Optional sample As Boolean = False) As Double
            Dim v = CLRVector.asNumeric(x)

            If v.IsNullOrEmpty Then
                Return 0
            End If

            Dim std As Double = v.SD(isSample:=sample)

            Return std
        End Function

        ''' <summary>
        ''' ## Pearson Correlation Testing in R Programming
        ''' 
        ''' Correlation is a statistical measure that indicates 
        ''' how strongly two variables are related. It involves 
        ''' the relationship between multiple variables as well. 
        ''' For instance, if one is interested to know whether 
        ''' there is a relationship between the heights of fathers 
        ''' and sons, a correlation coefficient can be calculated 
        ''' to answer this question. Generally, it lies between 
        ''' -1 and +1. It is a scaled version of covariance and 
        ''' provides the direction and strength of a relationship. 
        ''' 
        ''' this function measure a Parametric Correlation – Pearson correlation(r): 
        ''' It measures a linear dependence between two variables (x and y) 
        ''' is known as a parametric correlation test because it depends on 
        ''' the distribution of the data.
        ''' 
        ''' Pearson Rank Correlation is a parametric correlation. 
        ''' The Pearson correlation coefficient is probably the most
        ''' widely used measure for linear relationships between two 
        ''' normal distributed variables and thus often just called 
        ''' "correlation coefficient". 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="MAXIT"></param>
        ''' <returns>
        ''' 1. r takes a value between -1 (negative correlation) and 1 (positive correlation).
        ''' 2. r = 0 means no correlation.
        ''' 3. Can Not be applied to ordinal variables.
        ''' 4. The sample size should be moderate (20-30) For good estimation.
        ''' 5. Outliers can lead To misleading values means Not robust With outliers.
        ''' </returns>
        <ExportAPI("pearson")>
        Public Function pearson(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object,
                                Optional MAXIT As Integer = 5000,
                                Optional env As Environment = Nothing) As Object

            Dim data1 As Double() = CLRVector.asNumeric(x)
            Dim data2 As Double() = CLRVector.asNumeric(y)
            Dim p1#
            Dim p2#
            Dim z#
            Dim cor#

            If data1.Length <> data2.Length Then
                Return Internal.debug.stop({
                    "incompatible dimensions!",
                    "dims(x): " & data1.Length,
                    "dims(y): " & data2.Length
                }, env)
            Else
                Beta.MAXIT = MAXIT
            End If

            cor = GetPearson(data1, data2, p1, p2, z, throwMaxIterError:=False)

            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"cor", cor},
                    {"p-value", p1},
                    {"prob2", p2},
                    {"z", z}
                }
            }
        End Function

        ''' <summary>
        ''' ### Test for Association/Correlation Between Paired Samples
        ''' 
        ''' Test for association between paired samples, using one of 
        ''' Pearson's product moment correlation coefficient, Kendall's 
        ''' \tauτ or Spearman's \rhoρ.
        ''' </summary>
        ''' <param name="x">numeric vectors of data values. x and y must have the same length.</param>
        ''' <param name="y">numeric vectors of data values. x and y must have the same length.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("cor.test")>
        Public Function cor_test(x As Double(), y As Double(),
                                 <RRawVectorArgument(GetType(String))>
                                 Optional method As Object = "pearson|kendall|spearman",
                                 Optional env As Environment = Nothing) As Object

            Dim pvalue As Double
            Dim prob2 As Double
            Dim z As Double, t As Double, df As Double
            Dim cor As Double

            Select Case LCase(CStr(REnv.single(method, forceSingle:=True)))
                Case "pearson" : cor = Correlations.GetPearson(x, y)
                Case "kendall" : cor = Correlations.rankKendallTauBeta(x, y)
                Case "spearman" : cor = Correlations.Spearman(x, y)
            End Select

            Call Correlations.TestStats(cor, x.Length, z, pvalue, prob2, t, df, throwMaxIterError:=False)

            Return New corTestResult With {
                .cor = cor,
                .df = df,
                .prob2 = prob2,
                .pvalue = pvalue,
                .t = t,
                .z = z
            }
        End Function

        Public Class corTestResult

            Public Property cor As Double
            Public Property pvalue As Double
            Public Property prob2 As Double
            Public Property z As Double
            Public Property t As Double
            Public Property df As Integer

            Public Overrides Function ToString() As String
                Return $"
	Pearson's product-moment correlation

data:  a and b
t = {t}, df = {df}, p-value = {pvalue}
alternative hypothesis: true correlation is not equal to 0
95 percent confidence interval:
 -0.14908034  0.01559428
sample estimates:
        cor 
{cor}
"
            End Function
        End Class

        ''' <summary>
        ''' set.seed is the recommended way to specify seeds.
        ''' </summary>
        ''' <param name="seed">
        ''' a single value, interpreted as an integer, 
        ''' or NULL (see ‘Details’).
        ''' </param>
        ''' <remarks>
        ''' set.seed returns NULL, invisibly.
        ''' </remarks>
        <ExportAPI("set.seed")>
        Public Sub set_seed(seed As Integer)
            randf.SetSeed(seed)
        End Sub

        ''' <summary>
        ''' get a random number value between ``[0,1]``.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("rnd")>
        Public Function getRandom() As Double
            Return randf.seeds.NextDouble
        End Function

        ''' <summary>
        ''' runif generates random deviates.
        ''' </summary>
        ''' <param name="n">
        ''' number of observations. If length(n) > 1, the length is taken to be the number required.
        ''' </param>
        ''' <param name="min">lower And upper limits of the distribution. Must be finite.</param>
        ''' <param name="max">lower And upper limits of the distribution. Must be finite.</param>
        ''' <returns></returns>
        <ExportAPI("runif")>
        Public Function runif(Optional n As Integer = 1, Optional min# = 0, Optional max# = 1) As Double()
            Dim rnd As Random = randf.seeds
            Dim [if] As New List(Of Double)

            For i As Integer = 0 To n - 1
                [if].Add(rnd.NextDouble(min, max))
            Next

            Return [if].ToArray
        End Function

        <ExportAPI("rnorm")>
        Public Function rnorm(n%, Optional mean# = 0, Optional sd# = 1) As Double()
            Dim rnd As Random = randf.seeds
            Dim gauss As New List(Of Double)

            For i As Integer = 0 To n - 1
                gauss.Add(rnd.NextGaussian(mean, sd))
            Next

            Return gauss.ToArray
        End Function

        ''' <summary>
        ''' ### Random Samples and Permutations
        ''' 
        ''' ``sample`` takes a sample of the specified size from the elements 
        ''' of x using either with or without replacement.
        ''' </summary>
        ''' <param name="x">
        ''' either a vector Of one Or more elements from which To choose, Or a positive Integer. See 'Details.’
        ''' </param>
        ''' <param name="size">a non-negative integer giving the number of items to choose.</param>
        ''' <param name="replace">should sampling be with replacement?</param>
        ''' <param name="prob">
        ''' a vector Of probability weights For obtaining the elements Of the vector being sampled.
        ''' </param>
        ''' <remarks>
        ''' If x has length 1, is numeric (in the sense of is.numeric) and ``x >= 1``, sampling 
        ''' via sample takes place from ``1:x``. Note that this convenience feature may lead to 
        ''' undesired behaviour when x is of varying length in calls such as sample(x). 
        ''' See the examples.
        '''
        ''' Otherwise x can be any R Object For which length And subsetting by integers make sense: 
        ''' S3 Or S4 methods for these operations will be dispatched as appropriate.
        '''
        ''' For sample the default for size Is the number of items inferred from the first argument, 
        ''' so that sample(x) generates a random permutation of the elements of x (Or 1:x).
        '''
        ''' It Is allowed to ask for size = 0 samples with n = 0 Or a length-zero x, but otherwise 
        ''' ``n > 0`` Or positive length(x) Is required.
        '''
        ''' Non-integer positive numerical values of n Or x will be truncated to the next smallest 
        ''' integer, which has to be no larger than ``.Machine$integer.max``.
        '''
        ''' The optional prob argument can be used to give a vector of weights for obtaining the elements 
        ''' of the vector being sampled. They need Not sum to one, but they should be non-negative And 
        ''' Not all zero. If replace Is true, Walker's alias method (Ripley, 1987) is used when there 
        ''' are more than 200 reasonably probable values: this gives results incompatible with those 
        ''' from ``R &lt; 2.2.0``.
        '''
        ''' If replace Is False, these probabilities are applied sequentially, that Is the probability 
        ''' Of choosing the Next item Is proportional To the weights amongst the remaining items. The 
        ''' number Of nonzero weights must be at least size In this Case.
        ''' </remarks>
        ''' <returns>
        ''' For sample a vector of length size with elements drawn from either ``x`` or from the 
        ''' integers ``1:x``.
        ''' </returns>
        <ExportAPI("sample")>
        Public Function sample(<RRawVectorArgument>
                               x As Object,
                               Optional size As Object = Nothing,
                               Optional replace As Boolean = False,
                               Optional prob As Object = Nothing,
                               Optional env As Environment = Nothing) As Object

            If size Is Nothing Then
                ' 20230629
                ' return collection shuffle index
                '
                If TypeOf x Is list Then
                    ' get list shuffle index
                    Return DirectCast(x, list).shuffle(replace, prob, env)
                ElseIf TypeOf x Is dataframe Then
                    ' get dataframe row shuffle
                    Return DirectCast(x, dataframe).shuffle(replace, prob, env)
                Else
#Disable Warning
                    ' get vector index shuffle
                    Return REnv.asVector(Of Object)(x).shuffle(replace, prob, env)
#Enable Warning
                End If
            Else
                Dim size_int As Integer = CLRVector.asInteger(size).FirstOrDefault
                Dim result = sample(x, size_int, replace, prob, env)

                Return result
            End If
        End Function

        <Extension>
        Private Function shuffle(x As Array, replace As Boolean, prob As Object, env As Environment) As Object
            Dim index As Integer() = x.Length.SeqIterator(offset:=1).ToArray
            Dim shuffles As Object = sample(index, size:=x.Length, replace, prob, env)
            Return shuffles
        End Function

        <Extension>
        Private Function shuffle(x As dataframe, replace As Boolean, prob As Object, env As Environment) As Object
            Dim names As String() = x.rownames

            If names Is Nothing Then
                Return Internal.debug.stop("the required dataframe row names index could not be nothing!", env)
            End If

            Dim shuffles As Object = sample(names, size:=x.nrows, replace, prob, env)
            Return shuffles
        End Function

        <Extension>
        Private Function shuffle(x As list, replace As Boolean, prob As Object, env As Environment) As Object
            Dim names As String() = x.getNames
            Dim shuffles As Object = sample(names, size:=x.length, replace, prob, env)
            Return shuffles
        End Function

        Private Function sample(x As Object, size As Integer, replace As Boolean, prob As Object, env As Environment) As Object
#Disable Warning
            Dim data As Array = REnv.asVector(Of Object)(x)
#Enable Warning

            'If data.Length <= size AndAlso replace = False Then
            '    Call env.AddMessage("data size of x is less than sample size, returns the original data vector.")
            '    Return data
            'End If

            ' 20230629 index vector is base from 1
            ' so we needs index-1 when get element value from the data array
            Dim index As Integer() = sample_int(n:=data.Length, size, replace, prob)
            Dim takeSamples As New List(Of Object)

            For Each i As Integer In index
                Call takeSamples.Add(data(i - 1))
            Next

            Return takeSamples.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="size"></param>
        ''' <param name="replace"></param>
        ''' <param name="prob"></param>
        ''' <returns>
        ''' returns an integer vector that could be used for represents the element index
        ''' the generated integer vector in this function is base from 1 
        ''' </returns>
        <ExportAPI("sample.int")>
        <RApiReturn(GetType(Integer))>
        Public Function sample_int(n As Integer,
                                   Optional size As Object = "n",
                                   Optional replace As Boolean = False,
                                   Optional prob As Object = Nothing) As Object

            Dim i As New List(Of Integer)(n.Sequence(offset:=1))
            Dim list As New List(Of Integer)
            Dim seeds As Random = randf.seeds

            If size.ToString <> "n" Then
                n = size
            End If

            If replace Then
                ' 有重复的采样
                For j As Integer = 0 To n - 1
                    list.Add(i(seeds.Next(0, i.Count)))
                Next
            Else
                Dim index As Integer

                For j As Integer = 0 To n - 1
                    index = seeds.Next(0, i.Count)
                    list.Add(i(index))
                    i.RemoveAt(index)
                Next
            End If

            Return list.ToArray
        End Function

        ''' <summary>
        ''' grouping data input by given numeric tolerance
        ''' </summary>
        ''' <param name="sequence"></param>
        ''' <param name="eval">
        ''' this parameter should be a lambda function which 
        ''' evaluate a numeric value for each elements in 
        ''' the given sequence data.
        ''' </param>
        ''' <param name="offset">
        ''' the max tolerance error of the cluster data
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("cluster_1D")>
        Public Function cluster1D(<RRawVectorArgument>
                                  sequence As Object,
                                  eval As Object,
                                  Optional offset As Double = 0,
                                  Optional env As Environment = Nothing) As Object

            Dim data As pipeline = pipeline.TryCreatePipeline(Of Object)(sequence, env)
            Dim evalFUNC As Evaluate(Of Object)

            If data.isError Then
                Return data.getError
            End If

            If eval Is Nothing Then
                Return Internal.debug.stop("the evaluation delegate function can not be nothing, i'm unsure about how to evaluate the data object as numeric value...", env)
            ElseIf TypeOf eval Is Func(Of Object, Double) Then
                evalFUNC = New Evaluate(Of Object)(AddressOf DirectCast(eval, Func(Of Object, Double)).Invoke)
            ElseIf TypeOf eval Is DeclareLambdaFunction Then
                evalFUNC = AddressOf DirectCast(eval, DeclareLambdaFunction).CreateLambda(Of Object, Double)(env).Invoke
            Else
                Return Internal.debug.stop("unsupport type for call as evaluator function!", env)
            End If

            Dim groups = data _
                .populates(Of Object)(env) _
                .GroupBy(AddressOf evalFUNC.Invoke, Function(a, b) stdNum.Abs(a - b) <= offset) _
                .ToArray

            Return groups _
                .Select(Function(a)
                            Return New Group With {
                                .key = a.name,
                                .group = a.value
                            }
                        End Function) _
                .ToArray
        End Function

        <ExportAPI("numeric_tags")>
        Public Function numericClassTags(<RRawVectorArgument> x As Object, offset As Double) As Integer()
            Dim data As Double() = CLRVector.asNumeric(x)
            Dim groups = data.GroupBy(offset).ToArray
            Dim tags As New List(Of Integer)
            Dim means As Double() = groups.Select(Function(a) a.Average).ToArray

            For Each xj As Double In data
                Dim d = means.Select(Function(xi) stdNum.Abs(xi - xj)).ToArray
                Dim i As Integer = which.Min(d)

                Call tags.Add(i + 1)
            Next

            Return tags.ToArray
        End Function

        ''' <summary>
        ''' ### Correlation, Variance and Covariance (Matrices)
        ''' 
        ''' var, cov and cor compute the variance of x and the covariance or 
        ''' correlation of x and y if these are vectors. If x and y are 
        ''' matrices then the covariances (or correlations) between the columns 
        ''' of x and the columns of y are computed.
        ''' </summary>
        ''' <param name="x">a numeric vector, matrix or data frame.</param>
        ''' <param name="y">
        ''' NULL (default) or a vector, matrix or data frame with compatible dimensions to x. 
        ''' The default is equivalent to y = x (but more efficient).
        ''' </param>
        ''' <param name="na_rm">logical. Should missing values be removed?</param>
        ''' <param name="use">
        ''' an optional character string giving a method for computing covariances 
        ''' in the presence of missing values. This must be (an abbreviation of) 
        ''' one of the strings "everything", "all.obs", "complete.obs", "na.or.complete", 
        ''' or "pairwise.complete.obs".</param>
        ''' <returns></returns>
        <ExportAPI("var")>
        Public Function var(<RRawVectorArgument> x As Object,
                            <RRawVectorArgument>
                            Optional y As Object = Nothing,
                            Optional na_rm As Boolean = False,
                            Optional use As varUseMethods = varUseMethods.everything) As Object

            Dim vx As Double() = CLRVector.asNumeric(x)
            Dim vy As Double()

            If y Is Nothing Then
                vy = vx
            Else
                vy = CLRVector.asNumeric(y)
            End If

            If na_rm Then
                vx = vx.Where(Function(xi) Not xi.IsNaNImaginary).ToArray
                vy = vy.Where(Function(yi) Not yi.IsNaNImaginary).ToArray
            End If

            Throw New NotImplementedException
        End Function
    End Module
End Namespace
