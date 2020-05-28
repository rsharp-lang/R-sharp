#Region "Microsoft.VisualBasic::a8cf3c0486c12d96c8c4f92d819c8a76, R#\Runtime\Internal\objects\dataset\dataframe.vb"

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
'         Properties: columns, ncols, nrows, rownames
' 
'         Function: GetByRowIndex, (+2 Overloads) getColumnVector, getKeyByIndex, getRowList, GetTable
'                   projectByColumn, sliceByRow
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    Public Class dataframe

        ''' <summary>
        ''' 长度为1或者长度为n
        ''' </summary>
        ''' <returns></returns>
        Public Property columns As Dictionary(Of String, Array)
        Public Property rownames As String()

        Public ReadOnly Property nrows As Integer
            Get
                Return Aggregate col As Array
                       In columns.Values
                       Let len = col.Length
                       Into Max(len)
            End Get
        End Property

        Public ReadOnly Property ncols As Integer
            Get
                Return columns.Count
            End Get
        End Property

        Public Function getColumnVector(columnName As String) As Array
            Dim n As Integer = nrows
            Dim col As Array = columns.TryGetValue(columnName)

            If col Is Nothing Then
                Return Nothing
            ElseIf col.Length = n Then
                Return col
            Else
                Return VectorExtensions _
                    .Replicate(col.GetValue(Scan0), n) _
                    .ToArray
            End If
        End Function

        Public Function getKeyByIndex(index As Integer) As String
            Return columns.Keys.ElementAtOrDefault(index - 1)
        End Function

        Public Function getColumnVector(index As Integer) As Array
            Return getColumnVector(getKeyByIndex(index))
        End Function

        Public Overrides Function ToString() As String
            Return $"[{nrows}, {ncols}] {columns.Keys.GetJson}"
        End Function

        ''' <summary>
        ''' ``data[, selector]``
        ''' </summary>
        ''' <param name="selector"></param>
        ''' <returns></returns>
        Public Function projectByColumn(selector As Array) As dataframe
            Dim indexType As Type = MeasureRealElementType(selector)

            If indexType Like RType.characters Then
                Return New dataframe With {
                    .rownames = rownames,
                    .columns = DirectCast(asVector(Of String)(selector), String()) _
                        .ToDictionary(Function(colName) colName,
                                      Function(key)
                                          Return columns(key)
                                      End Function)
                }
            ElseIf indexType Like RType.integers Then
                Throw New InvalidCastException
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
        Public Function sliceByRow(selector As Array) As dataframe
            Dim indexType As Type = MeasureRealElementType(selector)

            If indexType Like RType.logicals Then
                Dim bools As Integer() = Which.IsTrue(asLogical(selector))
                Dim subset As New dataframe With {
                    .rownames = bools _
                        .Select(Function(i, j) rownames.ElementAtOrDefault(i, j + 1)) _
                        .ToArray,
                    .columns = New Dictionary(Of String, Array)
                }

                For Each col In columns
                    subset.columns(col.Key) =
                        Function() As Array
                            Dim a As Array
                            Dim vec As Array = col.Value

                            If vec.Length = 1 Then
                                a = Array.CreateInstance(vec.GetValue(Scan0).GetType, 1)
                                a.SetValue(vec.GetValue(Scan0), Scan0)
                            Else
                                a = Array.CreateInstance(MeasureRealElementType(vec), bools.Length)

                                For Each i In bools.SeqIterator
                                    a.SetValue(vec.GetValue(i.value), i)
                                Next
                            End If

                            Return a
                        End Function()
                Next

                Return subset
            Else
                Throw New NotImplementedException(indexType.FullName)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="index"></param>
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

        Public Function GetByRowIndex(index As Integer()) As dataframe
            Dim subsetRowNumbers As String() = index _
                .Select(Function(i, j)
                            Return rownames.ElementAtOrDefault(i, j)
                        End Function) _
                .ToArray
            Dim subsetData As Dictionary(Of String, Array) = columns _
                .ToDictionary(Function(c) c.Key,
                              Function(c)
                                  If c.Value.Length = 1 Then
                                      ' single value
                                      Return DirectCast(c.Value.GetValue(Scan0), Array)
                                  End If

                                  Dim vec = index _
                                    .Select(Function(i)
                                                Return c.Value.GetValue(i)
                                            End Function) _
                                    .ToArray

                                  Return DirectCast(vec, Array)
                              End Function)

            Return New dataframe With {
                .rownames = subsetRowNumbers,
                .columns = subsetData
            }
        End Function

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

        ''' <summary>
        ''' Each element in a return result array is a row in table matrix
        ''' </summary>
        ''' <returns></returns>
        Public Function GetTable(env As GlobalEnvironment, Optional printContent As Boolean = True, Optional showRowNames As Boolean = True) As String()()
            Dim table As String()() = New String(nrows)() {}
            Dim rIndex As Integer
            Dim colNames$() = columns.Keys.ToArray
            Dim col As Array
            Dim row As String() = {""}.Join(colNames)
            Dim rownames = getRowNames()

            If showRowNames Then
                table(Scan0) = row.ToArray
            Else
                table(Scan0) = row.Skip(1).ToArray
            End If

            Dim elementTypes As Type() = colNames _
                .Select(Function(key)
                            Return columns(key).GetType.GetElementType
                        End Function) _
                .ToArray
            Dim formatters As IStringBuilder() = elementTypes _
                .Select(Function(type)
                            Return printer.ToString(type, env, printContent)
                        End Function) _
                .ToArray

            For i As Integer = 1 To table.Length - 1
                rIndex = i - 1
                row(Scan0) = rownames(rIndex)

                For j As Integer = 0 To columns.Count - 1
                    col = columns(colNames(j))

                    If col.Length = 1 Then
                        row(j + 1) = formatters(j)(col.GetValue(Scan0))
                    Else
                        row(j + 1) = formatters(j)(col.GetValue(rIndex))
                    End If
                Next

                If showRowNames Then
                    table(i) = row.ToArray
                Else
                    table(i) = row.Skip(1).ToArray
                End If
            Next

            Return table
        End Function

    End Class
End Namespace
