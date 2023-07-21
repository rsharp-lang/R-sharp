#Region "Microsoft.VisualBasic::710d6b9392f273cde3afb963f34c43bc, D:/GCModeller/src/R-sharp/Library/Rlapack//RMatrix.vb"

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

    '   Total Lines: 199
    '    Code Lines: 68
    ' Comment Lines: 116
    '   Blank Lines: 15
    '     File Size: 7.89 KB


    ' Module RMatrix
    ' 
    '     Function: eigen, Matrix
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("Matrix")>
Module RMatrix

    ''' <summary>
    ''' ## Matrices
    ''' 
    ''' matrix creates a matrix from the given set of values.
    ''' </summary>
    ''' <param name="data">
    ''' an optional data vector (including a list or expression vector).
    ''' Non-atomic classed R objects are coerced by as.vector and all
    ''' attributes discarded.
    ''' </param>
    ''' <param name="nrow">
    ''' the desired number of rows.
    ''' </param>
    ''' <param name="ncol">
    ''' the desired number of columns.
    ''' </param>
    ''' <param name="byrow">
    ''' logical. If FALSE (the default) the matrix Is filled by columns, 
    ''' otherwise the matrix Is filled by rows.
    ''' </param>
    ''' <param name="dimnames">
    ''' A dimnames attribute For the matrix: NULL Or a list of length 2 
    ''' giving the row And column names respectively. An empty list Is
    ''' treated as NULL, And a list of length one as row names. The 
    ''' list can be named, And the list names will be used as names for
    ''' the dimensions.
    ''' </param>
    ''' <param name="sparse"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' If one of nrow or ncol is not given, an attempt is made to infer
    ''' it from the length of data and the other parameter. If neither 
    ''' is given, a one-column matrix is returned.
    ''' 
    ''' If there are too few elements In data To fill the matrix, Then 
    ''' the elements In data are recycled. If data has length zero, NA 
    ''' Of an appropriate type Is used For atomic vectors (0 For raw 
    ''' vectors) And NULL For lists.
    ''' </remarks>
    <ExportAPI("matrix")>
    Public Function Matrix(<RRawVectorArgument>
                           Optional data As Object = Nothing,
                           Optional nrow As Integer = 1,
                           Optional ncol As Integer = 1,
                           Optional byrow As Boolean = False,
                           Optional dimnames As String() = Nothing,
                           Optional sparse As Boolean = False) As GeneralMatrix

        If TypeOf data Is vector Then
            data = DirectCast(data, vector).data
        End If

        If TypeOf data Is Double() Then
            If byrow Then
            Else

            End If
        End If

        If sparse Then
        Else
            Throw New NotImplementedException
        End If

        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' ### Spectral Decomposition of a Matrix
    ''' 
    ''' Computes eigenvalues and eigenvectors of numeric 
    ''' (double, integer, logical) or complex matrices.
    ''' </summary>
    ''' <param name="x">
    ''' a numeric Or complex matrix whose spectral
    ''' decomposition Is To be computed. Logical matrices 
    ''' are coerced To numeric.
    ''' </param>
    ''' <param name="symmetric">
    ''' If True, the matrix Is assumed To be symmetric 
    ''' (Or Hermitian If complex) And only its lower
    ''' triangle (diagonal included) Is used. If symmetric
    ''' Is Not specified, isSymmetric(x) Is used.
    ''' </param>
    ''' <param name="only_values">
    ''' If True, only the eigenvalues are computed And 
    ''' returned, otherwise both eigenvalues And 
    ''' eigenvectors are returned.
    ''' </param>
    ''' <param name="EISPACK">
    ''' logical. Defunct And ignored.
    ''' </param>
    ''' <remarks>
    ''' If symmetric is unspecified, isSymmetric(x) determines
    ''' if the matrix is symmetric up to plausible numerical 
    ''' inaccuracies. It is surer and typically much faster to
    ''' set the value yourself.
    ''' 
    ''' Computing the eigenvectors Is the slow part For large 
    ''' matrices.
    ''' 
    ''' Computing the eigendecomposition Of a matrix Is subject 
    ''' To errors On a real-world computer: the definitive 
    ''' analysis Is Wilkinson (1965). All you can hope For Is a 
    ''' solution To a problem suitably close To x. So even 
    ''' though a real asymmetric x may have an algebraic solution
    ''' With repeated real eigenvalues, the computed solution 
    ''' may be Of a similar matrix With complex conjugate pairs
    ''' Of eigenvalues.
    ''' 
    ''' Unsuccessful results from the underlying LAPACK code will 
    ''' result In an Error giving a positive Error code (most 
    ''' often 1): these can only be interpreted by detailed study
    ''' Of the FORTRAN code.
    ''' </remarks>
    ''' <returns>
    ''' The spectral decomposition of x is returned as a list 
    ''' with components
    '''
    ''' 1. values: a vector containing the p eigenvalues Of x, sorted
    '''            In decreasing order, according To Mod(values) In 
    '''            the asymmetric Case When they might be complex (even 
    '''            For real matrices). For real asymmetric matrices 
    '''            the vector will be complex only If complex conjugate
    '''            pairs Of eigenvalues are detected.
    ''' 2. vectors: either a p * p matrix whose columns contain the
    '''             eigenvectors Of x, Or NULL If only.values Is True. 
    '''             The vectors are normalized To unit length.
    '''
    ''' Recall that the eigenvectors are only defined up To a constant: 
    ''' even when the length Is specified they are still only defined
    ''' up to a scalar of modulus one (the sign for real matrices).
    '''
    ''' When only.values Is Not true, as by default, the result Is of 
    ''' S3 class "eigen".
    '''
    ''' If r &lt;- eigen(A), And V &lt;- r$vectors; lam &lt;- r$values, Then
    '''
    ''' ```
    ''' A = V Lmbd V^(-1)
    ''' ```
    '''
    ''' (up to numerical fuzz), where Lmbd = diag(lam).
    ''' </returns>
    <ExportAPI("eigen")>
    Public Function eigen(x As Object,
                          Optional symmetric As Boolean = Nothing,
                          Optional only_values As Boolean = False,
                          Optional EISPACK As Boolean = False,
                          Optional env As Environment = Nothing) As Object

        Dim data As NumericMatrix

        If TypeOf x Is dataframe Then
            Dim cols As New List(Of Double())
            Dim df As dataframe = DirectCast(x, dataframe)

            For Each name As String In df.colnames
                cols.Add(CLRVector.asNumeric(df.getColumnVector(name)))
            Next

            data = New NumericMatrix(cols)
        ElseIf x.GetType.ImplementInterface(Of GeneralMatrix) Then
            data = New NumericMatrix(DirectCast(x, GeneralMatrix).RowVectors)
        Else
            Return Internal.debug.stop("input data should be a dataframe or matrix object", env)
        End If

        Dim result = New EigenvalueDecomposition(data)
        Dim val As Double() = result.RealEigenvalues
        Dim vec As New dataframe With {.columns = New Dictionary(Of String, Array)}
        Dim i As i32 = 1

        For Each col In result.V.Transpose.RowVectors
            Call vec.add((++i).ToString, col.ToArray)
        Next

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"val", val},
                {"vec", vec}
            }
        }
    End Function
End Module
