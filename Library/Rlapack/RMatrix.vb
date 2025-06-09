#Region "Microsoft.VisualBasic::768193542945c569d3d523a51f96934c, Library\Rlapack\RMatrix.vb"

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

'   Total Lines: 770
'    Code Lines: 442 (57.40%)
' Comment Lines: 228 (29.61%)
'    - Xml Docs: 87.72%
' 
'   Blank Lines: 100 (12.99%)
'     File Size: 29.65 KB


' Module RMatrix
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: (+3 Overloads) add, asInteger, asMatrix, createTable, (+3 Overloads) division
'               dot, eigen, fromGraph, gauss, gauss_solve
'               Matrix, matrix_extractor, (+5 Overloads) multiply, nmf_decompose, nmf_matrix
'               one, parse, readMatrix, rowPack, saveMatrix
'               (+3 Overloads) substract, sum_all, zero
' 
'     Sub: extractVector
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Solvers
Imports Microsoft.VisualBasic.Math.SignalProcessing.HungarianAlgorithm
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports std = System.Math
Imports vec = Microsoft.VisualBasic.Math.LinearAlgebra.Vector

''' <summary>
''' The numeric matrix
''' </summary>
<Package("Matrix")>
<RTypeExport("LA_mat", GetType(NumericMatrix))>
Module RMatrix

    Sub New()
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(NumericMatrix), AddressOf createTable)
        Call RInternal.ConsolePrinter.AttachInternalConsoleFormatter(Of NumericMatrix)(
            Function(print, env)
                Return Function(o) As String
                           Dim df As Rdataframe = createTable(o, New list With {.slots = New Dictionary(Of String, Object)}, env)
                           Dim sb As New StringBuilder
                           Dim file As New StringWriter(sb)

                           Call file.WriteLine()
                           Call tablePrinter.PrintTable(df, maxPrint:=13, maxWidth:=80, output:=file, env.globalEnvironment)
                           Call file.Flush()

                           Return sb.ToString
                       End Function
            End Function)

        Call RInternal.generic.add("writeBin", GetType(NumericMatrix), AddressOf saveMatrix)
        Call RInternal.generic.add("readBin.LA_mat", GetType(Stream), AddressOf readMatrix)
    End Sub

    ''' <summary>
    ''' save matrix to binary file
    ''' </summary>
    ''' <param name="m"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' 
    <RGenericOverloads("writeBin")>
    Public Function saveMatrix(m As NumericMatrix, args As list, env As Environment) As Object
        Dim con As Stream = args!con
        Call m.Save(con)
        Call con.Flush()
        Return True
    End Function

    <RGenericOverloads("readBin")>
    Public Function readMatrix(s As Stream, args As list, env As Environment) As Object
        Return Serialization.Load(s)
    End Function

    Private Function createTable(m As GeneralMatrix, args As list, env As Environment) As Object
        Dim df As New Rdataframe With {.columns = New Dictionary(Of String, Array)}
        Dim ncols As Integer = m.ColumnDimension

        For i As Integer = 0 To ncols - 1
            Call df.add($"x{i + 1}", m.ColumnVector(i).ToArray)
        Next

        Return df
    End Function

    <ExportAPI("one")>
    <RApiReturn(GetType(NumericMatrix))>
    Public Function one(m As Integer, n As Integer) As Object
        Return NumericMatrix.One(m, n)
    End Function

    <ExportAPI("zero")>
    <RApiReturn(GetType(NumericMatrix))>
    Public Function zero(m As Integer, n As Integer) As Object
        Return NumericMatrix.Zero(m, n)
    End Function

    <ExportAPI("gauss")>
    <RApiReturn(GetType(NumericMatrix))>
    Public Function gauss(m As Integer, n As Integer) As Object
        Return NumericMatrix.Gauss(m, n)
    End Function

    ''' <summary>
    ''' create matrix from a graph
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("fromGraph")>
    <RApiReturn(GetType(NumericMatrix))>
    Public Function fromGraph(g As Object, keys As String(), Optional env As Environment = Nothing) As Object
        If g Is Nothing Then
            Return Nothing
        End If

        If TypeOf g Is SparseGraph Then
            Return DirectCast(g, SparseGraph).CreateMatrix(keys)
        ElseIf g.GetType.ImplementInterface(Of SparseGraph.ISparseGraph) Then
            Return SparseGraph.CreateMatrix(DirectCast(g, SparseGraph.ISparseGraph).GetGraph, keys)
        Else
            Return Message.InCompatibleType(GetType(SparseGraph), g.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x">
    ''' A numeric matrix of the cost for the assignment, for this input data type function returns an integer vector for indicates the assignments,
    ''' or a dataframe that contains the problems for make the assignment, the parameter a and b should not be nothing.
    ''' </param>
    ''' <param name="a">the field name for get assign target.</param>
    ''' <param name="b">the field name for get another assign target.</param>
    ''' <param name="cost">
    ''' the field name for the cost value(lower is better). if the dataframe contains the score data(higher is better), 
    ''' then you can convert the score to cost via formula: 10/(score+0.1)
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("hungarian_assignments")>
    <RApiReturn(TypeCodes.integer)>
    Public Function HungarianAssignments(<RRawVectorArgument> x As Object,
                                         Optional a As String = Nothing,
                                         Optional b As String = Nothing,
                                         Optional cost As String = Nothing,
                                         Optional env As Environment = Nothing) As Object

        If TypeOf x Is dataframe AndAlso a IsNot Nothing AndAlso b IsNot Nothing Then
        Else
            Dim m = matrix_extractor(x, env)
            Dim data As NumericMatrix

            If m Like GetType(Message) Then
                Return m.TryCast(Of Message)
            Else
                data = m.TryCast(Of NumericMatrix)
            End If

            Dim assignments = HungarianAlgorithm.FindAssignments(data)
            Return assignments
        End If
    End Function

    ''' <summary>
    ''' Parse the given base64 string as matrix
    ''' </summary>
    ''' <param name="base64">
    ''' should be network byte order encoded numeric vector in double type
    ''' </param>
    ''' <param name="dims">
    ''' the dimension size of the parsed matrix data, value should be an integer vector of [w x h].
    ''' value of this parameter also could be a scalar integer value, stands for dimension size of [N x N].
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parse")>
    <RApiReturn(GetType(NumericMatrix))>
    Public Function parse(base64 As String, <RRawVectorArgument> dims As Object,
                          Optional gzip As NetworkByteOrderBuffer.Compression = NetworkByteOrderBuffer.Compression.none,
                          Optional no_magic As Boolean = False,
                          Optional env As Environment = Nothing) As Object

        Dim dim_size As Integer() = CLRVector.asInteger(dims)

        If dim_size.Length = 1 Then
            ' N x N
            dim_size = {dim_size(0), dim_size(0)}
        ElseIf dim_size.Length < 1 Then
            Return RInternal.debug.stop("missing dimension size for create matrix!", env)
        End If

        Static network As New NetworkByteOrderBuffer

        Dim vec As Double() = network.ParseDouble(base64, gzip, noMagic:=no_magic)
        Dim m As Double()() = vec.Split(dim_size(0))

        ' check height
        If m(0).Length <> dim_size(1) Then
            Return RInternal.debug.stop({
                $"invalid dimension size of the matrix: {dim_size(0)}x{dim_size(1)}!",
                $"parsed matrix size: {dim_size(0)}x{m(0).Length}"
            }, env)
        End If

        Return New NumericMatrix(m)
    End Function

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
                           Optional sparse As Boolean = False,
                           Optional env As Environment = Nothing) As GeneralMatrix

        If data Is Nothing Then
            Return Nothing
        End If
        If TypeOf data Is vector Then
            data = DirectCast(data, vector).data
        End If

        data = TryCastGenericArray(data, env)

        If DataFramework.IsNumericCollection(data.GetType) Then
            Dim v As Double() = CLRVector.asNumeric(data)

            If byrow Then
                ncol = v.Length / nrow
            End If

            Return New NumericMatrix(v.Split(ncol))
        End If

        If sparse Then
        Else
            Throw New NotImplementedException
        End If

        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.integer")>
    Public Function asInteger(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Nothing
        End If

        If x.GetType.ImplementInterface(Of GeneralMatrix) Then
            Dim m As Double()() = DirectCast(x, GeneralMatrix).ArrayPack
            Dim i As Double()() = m _
                .Select(Function(r)
                            Return r.Select(Function(vi) std.Round(vi)).ToArray
                        End Function) _
                .ToArray

            Return New NumericMatrix(i)
        Else
            Return RConversion.asInteger(x, env)
        End If
    End Function

    ''' <summary>
    ''' ### NMF Model fitting function
    ''' 
    ''' Fit a non-negative matrix factorization model to a given target matrix. 
    ''' The function allows for different update rules and initialization 
    ''' methods, and can fit different NMF variants.
    ''' </summary>
    ''' <param name="x">
    ''' target data to fit, i.e. a matrix-like objec
    ''' </param>
    ''' <param name="rank">
    '''  specification of the factorization rank. It is usually a single numeric value, but
    ''' other type Of values are possible (e.g. matrix), For which specific methods are
    ''' implemented. See for example methods nmf,matrix,matrix,ANY.
    ''' If rank Is a numeric vector With more than one element, e.g. a range Of ranks,
    ''' then nmf performs the estimation procedure described in nmfEstimateRank.
    ''' </param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("nmf")>
    Public Function nmf_matrix(<RRawVectorArgument> x As Object, rank As Integer,
                               Optional max_iterations As Integer = 1000,
                               Optional tolerance As Double = 0.001,
                               Optional epsilon As Double = 1.0E-20,
                               Optional clr_obj As Boolean = False,
                               Optional env As Environment = Nothing) As Object

        Dim m = matrix_extractor(x, env)
        Dim data As NumericMatrix

        If m Like GetType(Message) Then
            Return m.TryCast(Of Message)
        Else
            data = m.TryCast(Of NumericMatrix)
        End If

        Dim nmf_result As NMF = NMF.Factorisation(data, rank, max_iterations, tolerance, epsilon)

        If clr_obj Then
            Return nmf_result
        End If

        Dim result As New list(
            slot("W") = nmf_result.W,
            slot("H") = nmf_result.H,
            slot("cost") = nmf_result.errors
        )

        Return result
    End Function

    <ExportAPI("nmf_decompose")>
    Public Function nmf_decompose(<RRawVectorArgument> x As Object, nmf As NMF, Optional env As Environment = Nothing) As Object
        Dim m = matrix_extractor(x, env)
        Dim data As NumericMatrix

        If m Like GetType(Message) Then
            Return m.TryCast(Of Message)
        Else
            data = m.TryCast(Of NumericMatrix)
        End If

        Dim decompose As list = list.empty
        Dim i As i32 = 1

        For Each xi As NumericMatrix In nmf.Decompose(data)
            Call decompose.add($"x{++i}", xi)
        Next

        Return decompose
    End Function

    ''' <summary>
    ''' as.matrix attempts to turn its argument into a matrix.
    ''' </summary>
    ''' <param name="x">an R Object.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.matrix")>
    Public Function asMatrix(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim m = matrix_extractor(x, env)

        If m Like GetType(Message) Then
            Return m.TryCast(Of Message)
        Else
            Return m.TryCast(Of NumericMatrix)
        End If
    End Function

    ''' <summary>
    ''' matrix to vector by extract from the row dimensions
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("row_pack")>
    Public Function rowPack(x As Object, Optional env As Environment = Nothing) As Object
        Dim pack As New List(Of Double)

        If x Is Nothing Then
            Return Nothing
        ElseIf x.GetType.ImplementInterface(Of GeneralMatrix) Then
            Dim m As GeneralMatrix = x
            m.RowVectors.DoEach(Sub(v) pack.AddRange(v))
            Return pack.ToArray
        Else
            Return Message.InCompatibleType(GetType(GeneralMatrix), x.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' sum all matrix
    ''' </summary>
    ''' <param name="x">a collection of the data matrix</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("sum_all")>
    <RApiReturn(GetType(NumericMatrix))>
    Public Function sum_all(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim sum As NumericMatrix = Nothing
        Dim pull = ObjectSet.GetObjectSet(x, env)

        For Each xi As Object In pull
            If xi Is Nothing Then
                Continue For
            End If

            If xi.GetType.ImplementInterface(Of GeneralMatrix) Then
                If sum Is Nothing Then
                    sum = xi
                Else
                    sum = sum + DirectCast(xi, GeneralMatrix)
                End If
            Else
                Return Message.InCompatibleType(GetType(NumericMatrix), xi.GetType, env)
            End If
        Next

        Return sum
    End Function

    Private Function matrix_extractor(x As Object, env As Environment) As [Variant](Of NumericMatrix, Message)
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
        ElseIf TypeOf x Is list Then
            data = New NumericMatrix(rows:=
                From row
                In DirectCast(x, list).slots
                Select CLRVector.asNumeric(row.Value)
            )
        Else
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If

            ' is array of array?
            If x.GetType.IsArray AndAlso DirectCast(x, Array) _
                .AsObjectEnumerator _
                .All(Function(v)
                         Return v IsNot Nothing AndAlso (v.GetType.IsArray OrElse TypeOf v Is vector)
                     End Function) Then

                Dim rows As New List(Of Double())

                For Each r As Object In DirectCast(x, Array)
                    Call rows.Add(CLRVector.asNumeric(r))
                Next

                Return New NumericMatrix(rows)
            End If

            Return RInternal.debug.stop("input data should be a dataframe or matrix object", env)
        End If

        Return data
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
    Public Function eigen(<RRawVectorArgument> x As Object,
                          Optional symmetric As Boolean = Nothing,
                          Optional only_values As Boolean = False,
                          Optional EISPACK As Boolean = False,
                          Optional env As Environment = Nothing) As Object

        Dim m = matrix_extractor(x, env)
        Dim data As NumericMatrix

        If m Like GetType(Message) Then
            Return m.TryCast(Of Message)
        Else
            data = m.TryCast(Of NumericMatrix)
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

    ''' <summary>
    ''' ### Gaussian elimination
    ''' 
    ''' In mathematics, Gaussian elimination, also known as row reduction, is an algorithm 
    ''' for solving systems of linear equations. It consists of a sequence of row-wise 
    ''' operations performed on the corresponding matrix of coefficients. This method can 
    ''' also be used to compute the rank of a matrix, the determinant of a square matrix, 
    ''' and the inverse of an invertible matrix. The method is named after Carl Friedrich 
    ''' Gauss (1777–1855). To perform row reduction on a matrix, one uses a sequence of
    ''' elementary row operations to modify the matrix until the lower left-hand corner of 
    ''' the matrix is filled with zeros, as much as possible. There are three types of 
    ''' elementary row operations:
    ''' 
    ''' + Swapping two rows,
    ''' + Multiplying a row by a nonzero number,
    ''' + Adding a multiple Of one row To another row.
    ''' 
    ''' Using these operations, a matrix can always be transformed into an upper triangular
    ''' matrix, And In fact one that Is In row echelon form. Once all Of the leading coefficients 
    ''' (the leftmost nonzero entry In Each row) are 1, And every column containing a leading
    ''' coefficient has zeros elsewhere, the matrix Is said To be In reduced row echelon form.
    ''' This final form Is unique; In other words, it Is independent Of the sequence Of row
    ''' operations used. For example, In the following sequence Of row operations (where two 
    ''' elementary operations On different rows are done at the first And third steps), the 
    ''' third And fourth matrices are the ones In row echelon form, And the final matrix Is 
    ''' the unique reduced row echelon form.
    ''' </summary>
    ''' <param name="problem"></param>
    ''' <param name="y"></param>
    ''' <param name="env"></param>
    ''' <returns>a vector of the result x</returns>
    <ExportAPI("gauss_solve")>
    Public Function gauss_solve(<RRawVectorArgument>
                                problem As Object,
                                <RRawVectorArgument>
                                Optional y As Object = Nothing,
                                Optional env As Environment = Nothing) As Object
        Dim a As NumericMatrix
        Dim b As vec

        If TypeOf problem Is Rdataframe Then
            Dim m = DirectCast(problem, Rdataframe) _
                .forEachRow() _
                .Select(Function(r) CLRVector.asNumeric(r.value)) _
                .ToArray

            a = New NumericMatrix(m)
            b = CLRVector.asNumeric(y)
        ElseIf TypeOf problem Is VectorLiteral Then
            Dim equ As ValueAssignExpression() = DirectCast(problem, VectorLiteral) _
                .Select(Function(exp) TryCast(exp, ValueAssignExpression)) _
                .ToArray

            ' check expression type:
            ' all expression should be the value assign expression?
            If Not equ.All(Function(exp) Not exp Is Nothing) Then
                Return RInternal.debug.stop("all of the input problem equation expression must be value assign expression, example like: a + b = y", env)
            End If

            Dim bvec As Object() = equ _
                .Select(Function(v) v.value.Evaluate(env)) _
                .ToArray
            Dim rows As New List(Of Double())

            For Each item As Object In bvec
                If TypeOf item Is Message Then
                    Return item
                End If
            Next

            ' get matrix from equations
            For Each eq As ValueAssignExpression In equ
                ' left should be binary expression 
                Dim math As BinaryExpression = TryCast(eq.targetSymbols(0), BinaryExpression)

                If math Is Nothing Then
                    Return RInternal.debug.stop("the equation expression should be a math binary expression!", env)
                Else
                    Dim row As New List(Of Double)

                    Call extractVector(math, row)
                    Call rows.Add(row.ToArray)
                End If

                If rows.Last.Any(Function(d) d.IsNaNImaginary) Then
                    Return RInternal.debug.stop("syntax error when parse the equation!", env)
                End If
            Next

            b = CLRVector.asNumeric(bvec)
            a = New NumericMatrix(rows.Select(Function(r) r.ToArray))
        Else
            Return Message.InCompatibleType(GetType(dataframe), problem.GetType, env)
        End If

        Dim solve As vec = GaussianElimination.Solve(a, b)

        Return New vector(solve.ToArray)
    End Function

    Private Sub extractVector(exp As BinaryExpression, ByRef row As List(Of Double))
        If TypeOf exp.left Is Literal Then
            row.Add(CDbl(exp.left.Evaluate(Nothing)))
        ElseIf TypeOf exp.left Is SymbolReference Then
            ' do nothing
        Else
            Call extractVector(exp.left, row)
        End If

        If exp.operator <> "*" Then
            Dim sign As Double = If(exp.operator = "-", -1, 1)

            If TypeOf exp.right Is Literal Then
                Call row.Add(sign * CDbl(exp.right.Evaluate(Nothing)))
            ElseIf TypeOf exp.right Is SymbolReference Then
                ' do nothing 
                Call row.Add(sign * 1)
            Else
                Call extractVector(exp.right, row)
            End If
        End If
    End Sub

    <ROperator(".*")>
    <ExportAPI("dot")>
    Public Function dot(a As NumericMatrix, b As NumericMatrix) As NumericMatrix
        Return a.DotProduct(b)
    End Function

    <ROperator("*")>
    Public Function multiply(a As NumericMatrix, b As NumericMatrix) As NumericMatrix
        Return a * b
    End Function

    <ROperator("/")>
    Public Function division(a As NumericMatrix, b As NumericMatrix) As NumericMatrix
        Return a / b
    End Function

    <ROperator("+")>
    Public Function add(a As NumericMatrix, b As NumericMatrix) As NumericMatrix
        Return a + b
    End Function

    <ROperator("-")>
    Public Function substract(a As NumericMatrix, b As NumericMatrix) As NumericMatrix
        Return a - b
    End Function

    <ROperator("*")>
    Public Function multiply(a As NumericMatrix, b As Double) As NumericMatrix
        Return a * b
    End Function

    <ROperator("*")>
    Public Function multiply(a As NumericMatrix, b As Long) As NumericMatrix
        Return a * CDbl(b)
    End Function

    <ROperator("*")>
    Public Function multiply(a As NumericMatrix, b As Integer) As NumericMatrix
        Return a * CDbl(b)
    End Function

    <ROperator("/")>
    Public Function division(a As NumericMatrix, b As Double) As NumericMatrix
        Return a / b
    End Function

    <ROperator("+")>
    Public Function add(a As NumericMatrix, b As Double) As NumericMatrix
        Return a + b
    End Function

    <ROperator("-")>
    Public Function substract(a As NumericMatrix, b As Double) As NumericMatrix
        Return a - b
    End Function

    <ROperator("*")>
    Public Function multiply(a As Double, b As NumericMatrix) As NumericMatrix
        Return a * b
    End Function

    <ROperator("/")>
    Public Function division(a As Double, b As NumericMatrix) As NumericMatrix
        Return a / b
    End Function

    <ROperator("+")>
    Public Function add(a As Double, b As NumericMatrix) As NumericMatrix
        Return a + b
    End Function

    <ROperator("-")>
    Public Function substract(a As Double, b As NumericMatrix) As NumericMatrix
        Return a - b
    End Function
End Module
