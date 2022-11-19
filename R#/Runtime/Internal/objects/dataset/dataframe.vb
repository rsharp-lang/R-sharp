#Region "Microsoft.VisualBasic::ff27ab8a366de7201972b107271d3b48, R-sharp\R#\Runtime\Internal\objects\dataset\dataframe.vb"

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

    '   Total Lines: 585
    '    Code Lines: 323
    ' Comment Lines: 199
    '   Blank Lines: 63
    '     File Size: 22.96 KB


    '     Class dataframe
    ' 
    '         Properties: colnames, columns, ncols, nrows, rownames
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+3 Overloads) add, checkColumnNames, CreateDataFrame, forEachRow, GetByRowIndex
    '                   getKeyByIndex, getNames, getRowIndex, getRowList, getRowNames
    '                   (+2 Overloads) getVector, hasName, projectByColumn, setNames, sliceByRow
    '                   subsetColData, ToString
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
    Public Class dataframe : Implements RNames

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
        ''' <returns></returns>
        Public ReadOnly Property nrows As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If columns.Count = 0 Then
                    Return 0
                Else
                    Return Aggregate col As Array
                           In columns.Values
                           Let len = col.Length
                           Into Max(len)
                End If
            End Get
        End Property

        Public ReadOnly Property ncols As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return columns.Count
            End Get
        End Property

        ''' <summary>
        ''' get column by name
        ''' </summary>
        ''' <returns>
        ''' this property always returns a vector in full size(length is equals to <see cref="nrows"/>)
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

        Public Function add(Of T)(key As String, value As T()) As dataframe
            columns(key) = value
            Return Me
        End Function

        Public Function add(Of T)(key As String, value As IEnumerable(Of T)) As dataframe
            columns(key) = value.ToArray
            Return Me
        End Function

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
        ''' 这个函数只会返回碰见的第一个同意名的列数据
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
        ''' <returns></returns>
        Public Function getVector(name As String, Optional fullSize As Boolean = False) As Array
            Dim col As Array = columns.TryGetValue(name)

            If col Is Nothing Then
                ' undefined column was selected
                Return Nothing
            End If

            If fullSize AndAlso col.Length <> nrows Then
                If col.Length <> 1 Then
                    Throw New InvalidProgramException
                End If

                Dim nrows As Integer = Me.nrows
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{nrows}x{ncols}] ['{columns.Keys.JoinBy("', '")}']"
        End Function

        ''' <summary>
        ''' ``data[, selector]``
        ''' </summary>
        ''' <param name="selector"></param>
        ''' <returns>dataframe</returns>
        Public Function projectByColumn(selector As Array, env As Environment, Optional fullSize As Boolean = False) As Object
            Dim indexType As Type = MeasureRealElementType(selector)
            Dim projections As New Dictionary(Of String, Array)

            If indexType Like RType.characters Then
                For Each key As String In DirectCast(asVector(Of String)(selector), String())
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

                For Each i As Integer In DirectCast(asVector(Of Integer)(selector), Integer())
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
                Return GetByRowIndex(index:=which.IsTrue(Vectorization.asLogical(selector)))
            ElseIf indexType Like RType.integers Then
                Return GetByRowIndex(index:=DirectCast(asVector(Of Integer)(selector), Integer()).Select(Function(i) i - 1).ToArray)
            ElseIf indexType Like RType.characters Then
                Dim indexNames As String() = asVector(Of String)(selector)
                Dim rowNames As Index(Of String) = Me.getRowNames
                Dim index As New List(Of Integer)
                Dim i As i32 = 0

                For Each name As String In indexNames
                    If (i = rowNames.IndexOf(name)) = -1 Then
                        Return Internal.debug.stop({$"missing row '{name}' in the given dataframe...", $"rowname: {name}"}, env)
                    Else
                        index.Add(i)
                    End If
                Next

                Return GetByRowIndex(index)
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
        ''' 所传递进来的索引编号，应该是以零为底的
        ''' </summary>
        ''' <param name="index">
        ''' 以零为底的索引号列表，-1对应的行将会返回空值的行数据
        ''' </param>
        ''' <returns></returns>
        Public Function GetByRowIndex(index As Integer()) As dataframe
            Dim subsetRowNumbers As String() = index _
                .Select(Function(i, j)
                            Return rownames.ElementAtOrDefault(i, j + 1)
                        End Function) _
                .ToArray
            Dim subsetData As Dictionary(Of String, Array) = columns _
                .ToDictionary(Function(c) c.Key,
                              Function(c)
                                  Return subsetColData(c.Value, index)
                              End Function)

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
        Private Function subsetColData(c As Array, index As Integer()) As Array
            Dim type As Type = MeasureRealElementType(c)
            Dim a As Array = Array.CreateInstance(type, index.Length)

            If index.Length = 0 Then
                Return a
            ElseIf c.Length = 1 Then
                ' single value
                Call a.SetValue(c.GetValue(Scan0), Scan0)
            Else
                For Each i As SeqValue(Of Integer) In index.SeqIterator
                    If i.value = -1 Then
                        Call a.SetValue(Nothing, i)
                    Else
                        Call a.SetValue(c.GetValue(i.value), i)
                    End If
                Next
            End If

            Return a
        End Function

        ''' <summary>
        ''' 这个函数会自动处理空值的情况
        ''' </summary>
        ''' <returns></returns>
        Public Function getRowNames() As String()
            If rownames.IsNullOrEmpty Then
                Return nrows _
                    .Sequence(offset:=1) _
                    .Select(Function(r) $"[{r}, ]") _
                    .ToArray
            Else
                Return rownames
            End If
        End Function

        Public Shared Function CreateDataFrame(Of T)(data As IEnumerable(Of T)) As dataframe
            Dim vec As T() = data.ToArray
            Dim type As Type = GetType(T)
            Dim schema As Dictionary(Of String, PropertyInfo) = DataFramework.Schema(type, PropertyAccess.Readable, PublicProperty, True)
            Dim dataframe As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }
            Dim values As Array

            For Each field In schema
                values = vec _
                    .Select(Function(obj)
                                Return field.Value.GetValue(obj)
                            End Function) _
                    .ToArray

                dataframe.columns.Add(field.Key, values)
            Next

            Return dataframe
        End Function

        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            If names.Length <> columns.Count Then
                Return Internal.debug.stop({
                    $"the given size of the column names character is not match the column numbers in this dataframe object!",
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
