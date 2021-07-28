﻿#Region "Microsoft.VisualBasic::a1d3349b423505ddb4d83be4474266c0, R#\Runtime\Internal\objects\dataset\dataframe.vb"

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

    '     Class dataframe
    ' 
    '         Properties: colnames, columns, ncols, nrows, rownames
    ' 
    '         Function: checkColumnNames, CreateDataFrame, forEachRow, GetByRowIndex, getKeyByIndex
    '                   getNames, getRowIndex, getRowList, getRowNames, (+2 Overloads) getVector
    '                   hasName, projectByColumn, setNames, sliceByRow, subsetColData
    '                   ToString
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
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object

    Public Class dataframe : Implements RNames

        ''' <summary>
        ''' 长度为1或者长度为n
        ''' </summary>
        ''' <returns></returns>
        Public Property columns As Dictionary(Of String, Array)
        Public Property rownames As String()

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
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getVector(Of T)(name As String) As T()
            Return REnv.asVector(Of T)(columns(name))
        End Function

        Public Function getVector(name As String, Optional fullSize As Boolean = False) As Array
            Dim col As Array = columns.TryGetValue(name)

            If col Is Nothing Then
                ' undefined column was selected
                Return Nothing
            End If

            If fullSize Then
                Dim nrows As Integer = Me.nrows
                Dim vec As Array = Array.CreateInstance(col.GetType.GetElementType, nrows)
                Dim getter = New GetVectorElement(col).Getter

                For i As Integer = 0 To nrows - 1
                    vec.SetValue(getter(i), i)
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
        ''' <returns></returns>
        Public Function projectByColumn(selector As Array, Optional fullSize As Boolean = False) As dataframe
            Dim indexType As Type = MeasureRealElementType(selector)

            If indexType Like RType.characters Then
                Return New dataframe With {
                    .rownames = rownames,
                    .columns = DirectCast(asVector(Of String)(selector), String()) _
                        .ToDictionary(Function(colName) colName,
                                      Function(key)
                                          Return getVector(key, fullSize)
                                      End Function)
                }
            ElseIf indexType Like RType.integers Then
                Dim colnames As String() = Me.colnames

                Return New dataframe With {
                    .rownames = rownames,
                    .columns = DirectCast(asVector(Of Integer)(selector), Integer()) _
                        .ToDictionary(Function(i) colnames(i - 1),
                                      Function(key)
                                          Return getVector(colnames(key - 1), fullSize)
                                      End Function)
                }
            ElseIf indexType Like RType.logicals Then
                Throw New InvalidCastException
            Else
                Throw New InvalidCastException
            End If
        End Function

        ''' <summary>
        ''' ``data[selector, ]``
        ''' </summary>
        ''' <param name="selector"></param>
        ''' <returns></returns>
        Public Function sliceByRow(selector As Array, env As Environment) As [Variant](Of dataframe, Message)
            Dim indexType As Type = MeasureRealElementType(selector)

            If indexType Like RType.logicals Then
                Return GetByRowIndex(index:=which.IsTrue(asLogical(selector)))
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
                Throw New NotImplementedException(indexType.FullName)
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
        ''' <param name="index"></param>
        ''' <returns></returns>
        Public Function GetByRowIndex(index As Integer()) As dataframe
            Dim subsetRowNumbers As String() = index _
                .Select(Function(i, j)
                            Return rownames.ElementAtOrDefault(i, j)
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

        Private Function subsetColData(c As Array, index As Integer()) As Array
            Dim type As Type = MeasureRealElementType(c)
            Dim a As Array = Array.CreateInstance(type, index.Length)

            If c.Length = 1 Then
                ' single value
                Call a.SetValue(c.GetValue(Scan0), Scan0)
            Else
                For Each i As SeqValue(Of Integer) In index.SeqIterator
                    Call a.SetValue(c.GetValue(i.value), i)
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
