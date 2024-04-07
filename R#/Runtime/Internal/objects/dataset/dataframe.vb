#Region "Microsoft.VisualBasic::176626796e3db5e210f295626c4c4bc7, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/dataset/dataframe.vb"

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

    '   Total Lines: 743
    '    Code Lines: 388
    ' Comment Lines: 276
    '   Blank Lines: 79
    '     File Size: 29.91 KB


    '     Class dataframe
    ' 
    '         Properties: colnames, columns, ncols, nrows, rownames
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+3 Overloads) add, checkColumnNames, (+3 Overloads) CreateDataFrame, forEachRow, GetByRowIndex
    '                   getKeyByIndex, getNames, getRowIndex, getRowList, getRowNames
    '                   GetRowNumbers, (+2 Overloads) getVector, hasName, projectByColumn, setNames
    '                   (+2 Overloads) sliceByRow, subsetColData, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' syntax helper for a readonly dataframe liked clr object
    ''' </summary>
    ''' <remarks>
    ''' get column by index, get row by index, and get row names
    ''' </remarks>
    Public Interface IdataframeReader

        ''' <summary>
        ''' get column feature data from current readonly dataframe liked clr object
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Function getColumn(index As Object, env As Environment) As Object
        ''' <summary>
        ''' get row samples data from current readonly dataframe liked clr object
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Function getRow(index As Object, env As Environment) As Object
        Function getRowNames() As String()

    End Interface

    ''' <summary>
    ''' A data frame, a matrix-like structure whose columns 
    ''' may be of differing types (numeric, logical, factor 
    ''' and character and so on).
    ''' 
    ''' How the names Of the data frame are created Is complex, 
    ''' And the rest Of this paragraph Is only the basic 
    ''' story. If the arguments are all named And simple objects 
    ''' (Not lists, matrices Of data frames) Then the argument 
    ''' names give the column names. For an unnamed simple 
    ''' argument, a deparsed version Of the argument Is used 
    ''' As the name (With an enclosing I(...) removed). For a 
    ''' named matrix/list/data frame argument With more than 
    ''' one named column, the names Of the columns are the name 
    ''' Of the argument followed by a dot And the column name 
    ''' inside the argument: If the argument Is unnamed, the 
    ''' argument's column names are used. For a named or unnamed 
    ''' matrix/list/data frame argument that contains a single 
    ''' column, the column name in the result is the column 
    ''' name in the argument. Finally, the names are adjusted 
    ''' to be unique and syntactically valid unless 
    ''' ``check.names = FALSE``.
    ''' </summary>
    ''' <remarks>
    ''' A data frame is a list of variables of the same number of 
    ''' rows with unique row names, given class "data.frame". If 
    ''' no variables are included, the row names determine the 
    ''' number of rows.
    ''' The column names should be non-empty, And attempts To use 
    ''' empty names will have unsupported results. Duplicate column 
    ''' names are allowed, but you need To use ``check.names = False``
    ''' For data.frame To generate such a data frame. However, 
    ''' Not all operations On data frames will preserve duplicated 
    ''' column names: For example matrix-Like subsetting will 
    ''' force column names in the result To be unique.
    ''' data.frame converts each of its arguments to a data frame 
    ''' by calling ``as.data.frame(optional = TRUE)``. As that Is a 
    ''' generic function, methods can be written to change the 
    ''' behaviour of arguments according to their classes: R comes 
    ''' With many such methods. Character variables passed To 
    ''' data.frame are converted To factor columns unless Protected 
    ''' by I Or argument stringsAsFactors Is False. If a list Or 
    ''' data frame Or matrix Is passed To data.frame it Is As If 
    ''' Each component Or column had been passed As a separate 
    ''' argument (except For matrices Protected by I).
    ''' Objects passed To data.frame should have the same number 
    ''' Of rows, but atomic vectors (see Is.vector), factors And 
    ''' character vectors Protected by I will be recycled a whole 
    ''' number Of times If necessary (including As elements Of 
    ''' list arguments).
    ''' If row names are Not supplied In the Call To data.frame, 
    ''' the row names are taken from the first component that has 
    ''' suitable names, For example a named vector Or a matrix With 
    ''' rownames Or a data frame. (If that component Is subsequently 
    ''' recycled, the names are discarded With a warning.) If 
    ''' row.names was supplied As NULL Or no suitable component was 
    ''' found the row names are the Integer sequence starting at one 
    ''' (And such row names are considered To be 'automatic’, and 
    ''' not preserved by as.matrix).
    ''' If row names are supplied Of length one And the data frame 
    ''' has a Single row, the row.names Is taken To specify the 
    ''' row names And Not a column (by name Or number).
    ''' Names are removed from vector inputs Not Protected by I.
    ''' Default.stringsAsFactors Is a utility that takes 
    ''' ``getOption("stringsAsFactors")`` And ensures the result Is 
    ''' ``TRUE`` Or ``FALSE`` (Or throws an error if the value Is Not
    ''' NULL).
    ''' 
    ''' > Chambers, J. M. (1992) Data for models. Chapter 3 of 
    ''' Statistical Models in S eds J. M. Chambers and T. J. 
    ''' Hastie, Wadsworth &amp; Brooks/Cole.
    ''' </remarks>
    Public Class dataframe : Inherits RsharpDataObject
        Implements RNames

        ''' <summary>
        ''' 长度为1或者长度为n
        ''' </summary>
        ''' <returns></returns>
        Public Property columns As Dictionary(Of String, Array)
        Public Property rownames As String()

        ''' <summary>
        ''' get all keys names of <see cref="columns"/> data
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property colnames As String()
            Get
                Return columns.Keys.ToArray
            End Get
        End Property

        ''' <summary>
        ''' column <see cref="Array.Length"/>
        ''' </summary>
        ''' <returns>
        ''' the row number is tested based on the ncol, if the ncol is zero, then the 
        ''' function GetRowNumbers will returns zero directly.
        ''' </returns>
        Public ReadOnly Property nrows As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetRowNumbers()
            End Get
        End Property

        Public ReadOnly Property ncols As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return columns.TryCount
            End Get
        End Property

        ''' <summary>
        ''' current dataframe object contains any data or not?
        ''' </summary>
        ''' <returns>empty is true means contains no rows data or no columns</returns>
        Public ReadOnly Property empty As Boolean
            Get
                ' due to the reason of work routine of GetRowNumbers() function
                ' we just needs to test nrows is equals to ZERO or not
                Return nrows = 0
            End Get
        End Property

        ''' <summary>
        ''' get column by name
        ''' </summary>
        ''' <returns>
        ''' this property always returns a vector in full size(length is equals to <see cref="nrows"/>),
        ''' orelse the value null if the given <paramref name="columnName"/> is not exists in the 
        ''' dataframe column fields.
        ''' </returns>
        Default Public ReadOnly Property getColumnVector(columnName As String) As Array
            Get
                Return getVector(columnName, fullSize:=True)
            End Get
        End Property

        ''' <summary>
        ''' <see cref="getKeyByIndex"/>
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property getColumnVector(index As Integer) As Array
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return getColumnVector(getKeyByIndex(index))
            End Get
        End Property

        <DebuggerStepThrough>
        Sub New()
        End Sub

        ''' <summary>
        ''' do data clone
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As dataframe)
            rownames = clone.rownames
            columns = New Dictionary(Of String, Array)(clone.columns)
        End Sub

        ''' <summary>
        ''' add or replace a column vector
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function add(Of T)(key As String, value As T()) As dataframe
            columns(key) = value
            Return Me
        End Function

        ''' <summary>
        ''' add or replace a column vector
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function add(Of T)(key As String, value As IEnumerable(Of T)) As dataframe
            columns(key) = value.ToArray
            Return Me
        End Function

        ''' <summary>
        ''' add or replace a column vector
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function add(key As String, value As Array) As dataframe
            columns(key) = value
            Return Me
        End Function

        ''' <summary>
        ''' 将列索引号转换为列名称
        ''' </summary>
        ''' <param name="index">以1为底的列索引号</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getKeyByIndex(index As Integer) As String
            Return columns.Keys.ElementAtOrDefault(index - 1)
        End Function

        ''' <summary>
        ''' get column by name
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="synonym">同意名列表</param>
        ''' <returns>
        ''' function may returns nothing if all of the given name 
        ''' is missing from the dataframe object.
        ''' 
        ''' (这个函数只会返回碰见的第一个同意名的列数据)
        ''' </returns>
        ''' <remarks>
        ''' this function returns a vector in full size always
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getVector(Of T)(ParamArray synonym As String()) As T()
            For Each name As String In synonym
                If columns.ContainsKey(name) Then
                    Return REnv.asVector(Of T)(getVector(name, fullSize:=True))
                End If
            Next

            Return Nothing
        End Function

        Public Function getBySynonym(ParamArray synonym As String()) As Array
            For Each name As String In synonym
                If columns.ContainsKey(name) Then
                    Return columns(name)
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' get a vector from column data
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="fullSize">
        ''' the data vector should be fill with the 
        ''' identical value when deal with the scalar
        ''' value. This function just returns the 
        ''' scalar value by default is the target 
        ''' column contains just one element
        ''' </param>
        ''' <returns>
        ''' this function returns nothing if the given <paramref name="name"/> is not
        ''' exists in the dataframe fields.
        ''' </returns>
        Public Function getVector(name As String, Optional fullSize As Boolean = False) As Array
            Dim col As Array = columns.TryGetValue(name)

            If col Is Nothing Then
                ' undefined column was selected
                Return Nothing
            ElseIf col.IsNullOrEmpty Then
                ' 20240131
                ' is empty column,
                ' make it empty safely
                Return New Object(nrows - 1) {}
            End If

            Dim rowSize As Integer = Me.GetRowNumbers

            If fullSize AndAlso col.Length <> rowSize Then
                If col.Length = 0 Then
                    ' column contains no data?
                    Return Nothing
                End If
                If col.Length <> 1 Then
                    Throw New InvalidProgramException($"target column '{name}' is in different vector size({col.Length} elements) with the table rows({rowSize} data rows)!")
                End If

                Dim nrows As Integer = rowSize
                Dim scalar As Object = col.GetValue(Scan0)
                Dim elementType As Type = If(scalar Is Nothing, GetType(Object), scalar.GetType)
                Dim vec As Array = Array.CreateInstance(elementType, nrows)

                ' fill vector data with the single scalar 
                ' object to make a vector in full size
                For i As Integer = 0 To nrows - 1
                    Call vec.SetValue(scalar, i)
                Next

                Return vec
            Else
                Return col
            End If
        End Function

        Public Function checkColumnNames(names As IEnumerable(Of String), env As Environment) As Message
            Dim allNames = names.ToArray
            Dim missing As New List(Of String)

            For Each name As String In allNames
                If Not _columns.ContainsKey(name) Then
                    missing += name
                End If
            Next

            If missing > 0 Then
                Return Internal.debug.stop({$"missing column names: {missing.GetJson}!"}, env)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' show the dataframe dimension information
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{nrows}x{ncols}] ['{columns.Keys.JoinBy("', '")}']"
        End Function

        ''' <summary>
        ''' ``data[, selector]``
        ''' </summary>
        ''' <param name="selector"></param>
        ''' <param name="reverse">
        ''' only works for the character index
        ''' </param>
        ''' <returns>dataframe</returns>
        Public Function projectByColumn(selector As Array, env As Environment,
                                        Optional fullSize As Boolean = False,
                                        Optional reverse As Boolean = False) As Object

            Dim indexType As Type = MeasureRealElementType(selector)
            Dim projections As New Dictionary(Of String, Array)

            If indexType Like RType.characters Then
                If reverse Then
                    Dim negative As Index(Of String) = CLRVector.asCharacter(selector).Indexing

                    selector = colnames _
                        .Where(Function(c) Not c Like negative) _
                        .ToArray
                End If

                For Each key As String In CLRVector.asCharacter(selector)
                    projections(key) = getVector(key, fullSize)

                    If projections(key) Is Nothing Then
                        Return Internal.debug.stop({
                            $"Error in `[.data.frame`(x, ,'{key}'): undefined columns selected",
                            $"column: {key}"
                        }, env)
                    End If
                Next
            ElseIf indexType Like RType.integers Then
                Dim colnames As String() = Me.colnames
                Dim key As String

                For Each i As Integer In CLRVector.asInteger(selector)
                    key = colnames(i - 1)
                    projections(key) = getVector(key, fullSize)
                Next
            ElseIf indexType Like RType.logicals Then
                Throw New InvalidCastException
            Else
                Throw New InvalidCastException
            End If

            Return New dataframe With {
                .rownames = rownames,
                .columns = projections
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="flags"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' which is true is zero-based by default
        ''' </remarks>
        Public Function sliceByRow(flags As Boolean(), env As Environment) As [Variant](Of dataframe, Message)
            ' checked
            Return GetByRowIndex(index:=which.IsTrue(flags), env)
        End Function

        ''' <summary>
        ''' ``data[selector, ]``
        ''' </summary>
        ''' <param name="selector">
        ''' accepts:
        ''' 
        ''' 1. logical vector as row selector
        ''' 2. integer vector for take rows by row number
        ''' 3. character vector for take rows by row name
        ''' 
        ''' </param>
        ''' <returns></returns>
        Public Function sliceByRow(selector As Array, env As Environment) As [Variant](Of dataframe, Message)
            Dim indexType As Type = MeasureRealElementType(selector)

            If indexType Like RType.logicals Then
                ' which is true is zero-based by default
                Return GetByRowIndex(index:=which.IsTrue(CLRVector.asLogical(selector)), env) ' checked
            ElseIf indexType Like RType.integers Then
                Dim i_raw As Integer() = CLRVector.asInteger(selector)
                Dim i_offset As Integer()

                If i_raw.All(Function(offset) offset < 0) Then
                    ' removes rows
                    Return FilterByRowIndex(i_raw, env)
                End If

                If i_raw.Any(Function(i) i <= 0) Then
                    ' is already zero-based
                    i_offset = i_raw
                Else
                    ' needs offset 1
                    i_offset = i_raw _
                        .Select(Function(i) i - 1) _
                        .ToArray
                End If

                ' questinable
                Return GetByRowIndex(index:=i_offset, env)
            ElseIf indexType Like RType.characters Then
                Dim indexNames As String() = CLRVector.asCharacter(selector)
                Dim rowNames As Index(Of String) = Me.getRowNames
                Dim index As New List(Of Integer)
                Dim i As i32 = 0

                ' 20221207
                ' index is zero-based
                For Each name As String In indexNames
                    If (i = rowNames.IndexOf(name)) = -1 Then
                        Return Internal.debug.stop({$"missing row '{name}' in the given dataframe...", $"rowname: {name}"}, env)
                    Else
                        index.Add(i)
                    End If
                Next

                Return GetByRowIndex(index, env) ' checked
            Else
                Return Internal.debug.stop(New NotImplementedException(indexType.FullName), env)
            End If
        End Function

        ''' <summary>
        ''' 所传递进来这个函数的索引编号应该是以零为底的
        ''' </summary>
        ''' <param name="index">
        ''' index: integer 0 based
        ''' </param>
        ''' <param name="drop">
        ''' 当drop参数为false的时候，返回一个数组向量
        ''' 反之返回一个list
        ''' </param>
        ''' <returns></returns>
        Public Function getRowList(index As Integer, Optional drop As Boolean = False) As Object
            Dim slots As New Dictionary(Of String, Object)
            Dim array As Array

            For Each key As String In columns.Keys
                array = _columns(key)

                If array.Length = 1 Then
                    slots.Add(key, array.GetValue(Scan0))
                Else
                    slots.Add(key, array.GetValue(index))
                End If
            Next

            If drop Then
                Return slots
            Else
                Return columns.Keys _
                    .Select(Function(key) slots(key)) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' 获取得到数据框之中每一行的数据(``[rowname => columns[]]``)
        ''' </summary>
        ''' <param name="colKeys"></param>
        ''' <returns></returns>
        Public Iterator Function forEachRow(Optional colKeys As String() = Nothing) As IEnumerable(Of NamedCollection(Of Object))
            Dim rowIds As String() = getRowNames()
            Dim nrows As Integer = Me.nrows
            Dim array As Array

            If colKeys.IsNullOrEmpty Then
                colKeys = columns.Keys.ToArray
            End If

            For index As Integer = 0 To nrows - 1
                Dim objVec As Object() = New Object(colKeys.Length - 1) {}
                Dim key As String

                For i As Integer = 0 To colKeys.Length - 1
                    key = colKeys(i)
                    array = _columns(key)

                    If array.Length = 1 Then
                        objVec(i) = array.GetValue(Scan0)
                    Else
                        objVec(i) = array.GetValue(index)
                    End If
                Next

                Yield New NamedCollection(Of Object) With {
                    .name = rowIds(index),
                    .value = objVec
                }
            Next
        End Function

        ''' <summary>
        ''' 这个函数返回的是以零为底的索引值
        ''' </summary>
        ''' <param name="any"></param>
        ''' <returns></returns>
        Public Function getRowIndex(any As Object) As Integer
            If TypeOf any Is Array Then
                any = DirectCast(any, Array).GetValue(Scan0)
            ElseIf TypeOf any Is vector Then
                any = DirectCast(any, vector).data.GetValue(Scan0)
            End If

            If any.GetType Like RType.characters Then
                Dim rowname As String = Scripting.ToString(any)
                Return getRowNames.IndexOf(rowname)
            ElseIf any.GetType Like RType.integers Then
                ' R# index integer is from base 1
                Return CInt(any) - 1
            Else
                Return -1
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="skips">1-based index to be filter</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function FilterByRowIndex(skips As Integer(), env As Environment) As [Variant](Of dataframe, Message)
            Dim skipsIndex As Index(Of String) = skips _
                .Select(Function(i)
                            ' convert the negative value to positive row index
                            ' the positive row index is from 1-based
                            ' but used for skip when do subset
                            Return (-i).ToString
                        End Function) _
                .Indexing
            ' convert to zero-based selector index for make dataframe subsets 
            Dim subsetIndex As Integer() = nrows _
                .Sequence(offset:=1) _
                .Where(Function(i) Not i.ToString Like skipsIndex) _
                .Select(Function(a) a - 1) _
                .ToArray

            Return GetByRowIndex(subsetIndex, env)
        End Function

        ''' <summary>
        ''' 所传递进来的索引编号，应该是以零为底的
        ''' </summary>
        ''' <param name="index">
        ''' 以零为底的索引号列表，-1对应的行将会返回空值的行数据
        ''' </param>
        ''' <returns>
        ''' a dataframe object with row subset or an error message
        ''' </returns>
        Public Function GetByRowIndex(index As Integer(), env As Environment) As [Variant](Of dataframe, Message)
            Dim subsetRowNumbers As String() = index _
                .Select(Function(i, j)
                            Return rownames.ElementAtOrDefault(i, j + 1)
                        End Function) _
                .ToArray
            Dim subsetData As New Dictionary(Of String, Array)

            For Each col In columns
                Dim v = subsetColData(col.Value, index, env)

                If TypeOf v Is Message Then
                    Return DirectCast(v, Message)
                Else
                    subsetData.Add(col.Key, v)
                End If
            Next

            Return New dataframe With {
                .rownames = subsetRowNumbers,
                .columns = subsetData
            }
        End Function

        ''' <summary>
        ''' 索引编号，应该是以零为底的
        ''' </summary>
        ''' <param name="c"></param>
        ''' <param name="index">
        ''' 索引编号，应该是以零为底的。-1的元素返回空值
        ''' </param>
        ''' <returns></returns>
        Private Function subsetColData(c As Array, index As Integer(), env As Environment) As Object
            Dim type As Type = MeasureRealElementType(c, defaultType:=GetType(Object))
            Dim V As Array = Array.CreateInstance(type, index.Length)

            If index.Length = 0 Then
                Return V
            ElseIf c.Length = 1 Then
                ' single value
                Call V.SetValue(c.GetValue(Scan0), Scan0)
                Return V
            End If

            For Each i As SeqValue(Of Integer) In index.SeqIterator
                If i.value = -1 Then
                    Call V.SetValue(Nothing, i)
                Else
                    If i.value >= c.Length OrElse c.Length = 0 OrElse i.value < 0 Then
                        ' 20221207
                        '
                        ' System.IndexOutOfRangeException: index was outside the bounds of the array.
                        '   at System.Array.GetFlattenedIndex(ReadOnlySpan`1 indices)
                        '   at System.Array.GetValue(Int32 index)
                        '
                        Return Internal.debug.stop({
                            $"index({i.value}) was outside the bounds of the target column vector size({c.Length}).",
                            $"index offset: {i.value}",
                            $"column size: {c.Length}",
                            $"index: {If(index.Length < 80, index.GetJson, index.Take(80).ToArray.GetJson.Trim("]"c) & "...") }"
                        }, env)
                    Else
                        Call V.SetValue(c.GetValue(i.value), i)
                    End If
                End If
            Next

            Return V
        End Function

        ''' <summary>
        ''' 这个函数会自动处理空值的情况
        ''' </summary>
        ''' <returns>
        ''' this function will generated a index row names automatically if
        ''' the <see cref="rownames"/> data is nothing, or this function will
        ''' make a copy of the <see cref="rownames"/> array.
        ''' </returns>
        Public Function getRowNames() As String()
            If rownames.IsNullOrEmpty Then
                Return nrows _
                    .Sequence(offset:=1) _
                    .Select(Function(r) $"[{r}, ]") _
                    .ToArray
            Else
                Return rownames.ToArray
            End If
        End Function

        ''' <summary>
        ''' get max length of the column vectors
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRowNumbers() As Integer
            If columns.TryCount = 0 Then
                Return 0
            Else
                Return Aggregate col As Array
                       In columns.Values
                       Let len = col.Length
                       Into Max(len)
            End If
        End Function

        ''' <summary>
        ''' Cast a vector of CLR object to a dataframe
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the class object property used as the dataframe column data
        ''' </remarks>
        Public Shared Function CreateDataFrame(Of T)(data As IEnumerable(Of T)) As dataframe
            Dim vec As T() = data.ToArray
            Dim type As Type = GetType(T)
            Dim schema As Dictionary(Of String, PropertyInfo) = DataFramework.Schema(type, PropertyAccess.Readable, PublicProperty, True)
            Dim dataframe As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }
            Dim values As Array

            For Each field As KeyValuePair(Of String, PropertyInfo) In schema
                values = vec _
                    .Select(Function(obj)
                                Return field.Value.GetValue(obj)
                            End Function) _
                    .ToArray

                dataframe.columns.Add(field.Key, values)
            Next

            Return dataframe
        End Function

        Public Shared Function CreateDataFrame(Of T)(data As Dictionary(Of String, T()), Optional rownames As IEnumerable(Of String) = Nothing) As dataframe
            Return New dataframe With {
                .rownames = rownames.ToArray,
                .columns = data _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return DirectCast(a.Value, Array)
                                  End Function)
            }
        End Function

        ''' <summary>
        ''' Create dataframe from row data
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="rows"></param>
        ''' <param name="colNames"></param>
        ''' <returns></returns>
        Public Shared Function CreateDataFrame(Of T)(rows As NamedCollection(Of T)(), colNames As IEnumerable(Of String)) As dataframe
            Dim rowNames = rows.Select(Function(r) r.name).ToArray
            Dim cols As New List(Of Array)
            Dim row As NamedCollection(Of T)
            Dim keys As String() = colNames.ToArray

            For Each key As String In keys
                Call cols.Add(New T(rows.Length - 1) {})
            Next

            For i As Integer = 0 To rows.Length - 1
                row = rows(i)

                For j As Integer = 0 To cols.Count - 1
                    cols(j).SetValue(row(j), i)
                Next
            Next

            Dim fields As New Dictionary(Of String, Array)

            For i As Integer = 0 To keys.Length - 1
                Call fields.Add(keys(i), cols(i))
            Next

            Return New dataframe With {
                .rownames = rowNames,
                .columns = fields
            }
        End Function

        ''' <summary>
        ''' set column names via names function
        ''' </summary>
        ''' <param name="names"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' <example>
        ''' names(df) = x;
        ''' ' equals to
        ''' colnames(df) = x;
        ''' </example>
        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            If names.Length <> columns.Count Then
                ' the given names vector x size is mis-matched with the column number
                Return Internal.debug.stop({
                    $"the given size of the column names character is not match the column numbers in this dataframe object! You probably could check for the function for set dimension names on a dataframe object(names, rownames or colnames)?",
                    $"given: {names.Length}",
                    $"required: {columns.Count}"
                }, envir)
            Else
                Dim setNamesTemp As New Dictionary(Of String, Array)
                Dim oldNames As String() = getNames()

                For i As Integer = 0 To names.Length - 1
                    setNamesTemp.Add(names(i), columns(oldNames(i)))
                Next

                columns = setNamesTemp

                Return Me
            End If
        End Function

        ''' <summary>
        ''' has column name?
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasName(name As String) As Boolean Implements RNames.hasName
            Return columns.ContainsKey(name)
        End Function

        ''' <summary>
        ''' get column names
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getNames() As String() Implements IReflector.getNames
            Return columns.Keys.ToArray
        End Function
    End Class
End Namespace
