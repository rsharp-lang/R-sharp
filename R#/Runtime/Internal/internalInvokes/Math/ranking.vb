Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Math.Correlations.Ranking

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
    End Module
End Namespace