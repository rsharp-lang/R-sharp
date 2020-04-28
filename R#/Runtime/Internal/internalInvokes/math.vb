#Region "Microsoft.VisualBasic::85cad10822f43003e54063a23fe7a39d, R#\Runtime\Internal\internalInvokes\math.vb"

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

    '     Module math
    ' 
    '         Function: exp, log, max, mean, min
    '                   pearson, pow, rnorm, round, rsd
    '                   runif, sqrt, sum
    ' 
    '         Sub: set_seed
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports stdNum = System.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' R# math module
    ''' </summary>
    Module math

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
        Public Function round(x As Array, Optional decimals% = 0) As Double()
            If x Is Nothing OrElse x.Length = 0 Then
                Return Nothing
            Else
                Dim rounds = From element As Double
                             In Runtime.asVector(Of Double)(x)
                             Select stdNum.Round(element, decimals)

                Return rounds.ToArray
            End If
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
        Public Function log(x As Array, Optional newBase As Double = stdNum.E) As Double()
            Return Runtime.asVector(Of Double)(x) _
                .AsObjectEnumerator(Of Double) _
                .Select(Function(d) stdNum.Log(d, newBase)) _
                .ToArray
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

            Dim array = Runtime.asVector(Of Object)(x)
            Dim elementType As Type = Runtime.MeasureArrayElementType(array)

            Select Case elementType
                Case GetType(Boolean)
                    Return Runtime.asLogical(array).Select(Function(b) If(b, 1, 0)).Sum
                Case GetType(Integer), GetType(Long), GetType(Short), GetType(Byte)
                    Return Runtime.asVector(Of Long)(x).AsObjectEnumerator(Of Long).Sum
                Case Else
                    Return Runtime.asVector(Of Double)(x).AsObjectEnumerator(Of Double).Sum
            End Select
        End Function

        <ExportAPI("pow")>
        Public Function pow(x As Array, y As Array) As Object
            x = Runtime.asVector(Of Double)(x)
            y = Runtime.asVector(Of Double)(y)

            Return Runtime.Core.Power(Of Double, Double, Double)(x, y).ToArray
        End Function

        <ExportAPI("sqrt")>
        Public Function sqrt(x As Array) As Double()
            Return Runtime.asVector(Of Double)(x) _
                .AsObjectEnumerator(Of Double) _
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
            Return Runtime.asVector(Of Double)(x) _
                .AsObjectEnumerator(Of Double) _
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
        Public Function max(x As Array, Optional na_rm As Boolean = False) As Double
            Return Runtime.asVector(Of Double)(x).AsObjectEnumerator(Of Double).Max
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="na_rm">a logical indicating whether missing values should be removed.</param>
        ''' <returns></returns>
        <ExportAPI("min")>
        Public Function min(x As Array, Optional na_rm As Boolean = False) As Double
            Return Runtime.asVector(Of Double)(x).AsObjectEnumerator(Of Double).Min
        End Function

        ''' <summary>
        ''' Arithmetic Mean
        ''' </summary>
        ''' <param name="x">An R object. Currently there are methods for numeric/logical 
        ''' vectors and date, date-time and time interval objects. Complex vectors are 
        ''' allowed for trim = 0, only.</param>
        ''' <returns></returns>
        <ExportAPI("mean")>
        Public Function mean(x As Array) As Double
            If x Is Nothing OrElse x.Length = 0 Then
                Return 0
            Else
                Return Runtime.asVector(Of Double)(x).AsObjectEnumerator(Of Double).Average
            End If
        End Function

        ''' <summary>
        ''' abs(x) computes the absolute value of x
        ''' </summary>
        ''' <param name="x">a numeric Or complex vector Or array.</param>
        ''' <returns></returns>
        <ExportAPI("abs")>
        Public Function abs(x As Array) As Double()
            Return asVector(Of Double)(x) _
                .AsObjectEnumerator(Of Double) _
                .Select(AddressOf stdNum.Abs) _
                .ToArray
        End Function

        <ExportAPI("RSD")>
        Public Function rsd(x As Array) As Double
            Return Runtime.asVector(Of Double)(x).AsObjectEnumerator(Of Double).RSD
        End Function

        <ExportAPI("pearson")>
        Public Function pearson(x As Array, y As Array, Optional MAXIT As Integer = 5000) As list
            Dim data1 As Double() = Runtime.asVector(Of Double)(x)
            Dim data2 As Double() = Runtime.asVector(Of Double)(y)
            Dim p1#
            Dim p2#
            Dim z#
            Dim cor#

            Beta.MAXIT = MAXIT

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
        ''' set.seed is the recommended way to specify seeds.
        ''' </summary>
        ''' <param name="seed">a single value, interpreted as an integer, or NULL (see ‘Details’).</param>
        ''' <remarks>
        ''' set.seed returns NULL, invisibly.
        ''' </remarks>
        <ExportAPI("set.seed")>
        Public Sub set_seed(seed As Integer)
            randf.SetSeed(seed)
        End Sub

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
        Public Function runif(n$, Optional min# = 0, Optional max# = 1) As Double()
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
    End Module
End Namespace
