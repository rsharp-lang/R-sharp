#Region "Microsoft.VisualBasic::cb3401c6746ef9ec1ff421515ab5ac85, R#\Runtime\Internal\internalInvokes\math.vb"

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
    '         Function: exp, log, max, min, pearson
    '                   pow, round, rsd, sqrt, sum
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

        <ExportAPI("max")>
        Public Function max(x As Array) As Double
            Return Runtime.asVector(Of Double)(x).AsObjectEnumerator(Of Double).Max
        End Function

        <ExportAPI("min")>
        Public Function min(x As Array) As Double
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
    End Module
End Namespace
