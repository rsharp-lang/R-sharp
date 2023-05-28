#Region "Microsoft.VisualBasic::c0905e6ab8babef912c13c1b57368574, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/Math/ranking.vb"

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

    '   Total Lines: 144
    '    Code Lines: 47
    ' Comment Lines: 88
    '   Blank Lines: 9
    '     File Size: 7.44 KB


    '     Module ranking
    ' 
    '         Function: order, orderNumbers, rank
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Correlations.Ranking
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes

    Module ranking

        ''' <summary>
        ''' ### Sample Ranks
        ''' 
        ''' Returns the sample ranks of the values in a vector. Ties 
        ''' (i.e., equal values) and missing values can be handled in
        ''' several ways.
        ''' </summary>
        ''' <param name="x">a numeric, complex, character Or logical vector.</param>
        ''' <param name="na_last">	
        ''' For controlling the treatment of NAs. If TRUE, missing values in the 
        ''' data are put last; if FALSE, they are put first; if NA, they are 
        ''' removed; if "keep" they are kept with rank NA.</param>
        ''' <param name="ties_method">
        ''' a character string specifying how ties are treated, see ‘Details’; 
        ''' can be abbreviated.
        ''' </param>
        ''' <returns>
        ''' A numeric vector of the same length as x with names copied from x 
        ''' (unless na.last = NA, when missing values are removed). The vector is 
        ''' of integer type unless x is a long vector or ties.method = "average" when 
        ''' it is of double type (whether or not there are any ties).
        ''' </returns>
        ''' <remarks>
        ''' If all components are different (and no NAs), the ranks are well defined, 
        ''' with values in seq_along(x). With some values equal (called ‘ties’), 
        ''' the argument ties.method determines the result at the corresponding indices. 
        ''' The "first" method results in a permutation with increasing values at each 
        ''' index set of ties, and analogously "last" with decreasing values. The 
        ''' "random" method puts these in random order whereas the default, "average", 
        ''' replaces them by their mean, and "max" and "min" replaces them by their 
        ''' maximum and minimum respectively, the latter being the typical sports 
        ''' ranking.
        '''
        ''' NA values are never considered to be equal: for na.last = TRUE and ``na.last = FALSE``
        ''' they are given distinct ranks in the order in which they occur in x.
        '''
        ''' NB: rank is not itself generic but xtfrm is, and rank(xtfrm(x), ....) will have 
        ''' the desired result if there is a xtfrm method. Otherwise, rank will make use 
        ''' of ==, >, is.na and extraction methods for classed objects, possibly rather 
        ''' slowly.
        ''' </remarks>
        <ExportAPI("rank")>
        Public Function rank(x As Double(),
                             Optional na_last As Boolean = True,
                             Optional ties_method As Strategies = Strategies.OrdinalRanking) As Double()

            Return x.Ranking(ties_method)
        End Function

        ''' <summary>
        ''' ### Ordering Permutation
        ''' 
        ''' order returns a permutation which rearranges its first argument 
        ''' into ascending or descending order, breaking ties by further 
        ''' arguments. sort.list is the same, using only one argument.
        ''' </summary>
        ''' <param name="x">a sequence of numeric, complex, character or 
        ''' logical vectors, all of the same length, or a classed R object.
        ''' </param>
        ''' <param name="decreasing">
        ''' logical. Should the sort order be increasing or decreasing? 
        ''' For the "radix" method, this can be a vector of length equal to 
        ''' the number of arguments in .... For the other methods, it must 
        ''' be length one.</param>
        ''' <returns>An integer vector unless any of the inputs has 2^31 or 
        ''' more elements, when it is a double vector.</returns>
        ''' <remarks>
        ''' In the case of ties in the first vector, values in the second are 
        ''' used to break the ties. If the values are still tied, values in 
        ''' the later arguments are used to break the tie (see the first 
        ''' example). The sort used is stable (except for method = "quick"), 
        ''' so any unresolved ties will be left in their original ordering.
        '''
        ''' Complex values are sorted first by the real part, then the imaginary 
        ''' part.
        '''
        ''' Except for method "radix", the sort order for character vectors will 
        ''' depend on the collating sequence of the locale in use: see 
        ''' Comparison.
        '''
        ''' The "shell" method is generally the safest bet and is the default 
        ''' method, except for short factors, numeric vectors, integer vectors 
        ''' and logical vectors, where "radix" is assumed. Method "radix" stably 
        ''' sorts logical, numeric and character vectors in linear time. It 
        ''' outperforms the other methods, although there are caveats (see sort). 
        ''' Method "quick" for sort.list is only supported for numeric x with 
        ''' na.last = NA, is not stable, and is slower than "radix".
        '''
        ''' partial = NULL is supported for compatibility with other implementations 
        ''' of S, but no other values are accepted and ordering is always complete.
        '''
        ''' For a classed R object, the sort order is taken from xtfrm: as its help 
        ''' page notes, this can be slow unless a suitable method has been defined 
        ''' or is.numeric(x) is true. For factors, this sorts on the internal 
        ''' codes, which is particularly appropriate for ordered factors.
        ''' </remarks>
        <ExportAPI("order")>
        <RApiReturn(GetType(Integer))>
        Public Function order(x As Array,
                              Optional decreasing As Boolean = False,
                              Optional env As Environment = Nothing) As Object

            Dim generic As Object = REnv.TryCastGenericArray(x, env)

            If generic Is Nothing Then
                Return Internal.debug.stop("Error in order(NULL) : argument 1 is not a vector.", env)
            End If
            If Program.isException(generic) Then
                Return Internal.debug.stop("the input vector should be generic!", env)
            End If

            If TypeOf generic Is String() Then
                Return DirectCast(generic, String()).orderNumbers(decreasing)
            Else
                Return CLRVector.asNumeric(generic).orderNumbers(decreasing)
            End If
        End Function

        <Extension>
        Private Function orderNumbers(Of T As IComparable(Of T))(x As T(), decreasing As Boolean) As Integer()
            Return RankOrder(Of T).Input(x) _
                .DoCall(Function(v)
                            Return RankOrder(Of T).Ranking(v, desc:=decreasing)
                        End Function) _
                .Select(Function(d) d.i + 1) _
                .ToArray
        End Function
    End Module
End Namespace
