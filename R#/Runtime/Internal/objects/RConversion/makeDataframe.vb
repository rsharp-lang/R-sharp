#Region "Microsoft.VisualBasic::81ddeb7487e75d2beee1fa2e75e0d70e, R#\Runtime\Internal\objects\RConversion\makeDataframe.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Module makeDataframe
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CheckDimension, createColumnVector, createDataframe, fromList, is_ableConverts
    '                   PopulateDataSet, (+2 Overloads) RDataframe, TracebackDataFrmae, tryTypeLineage
    ' 
    '         Sub: [addHandler]
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.Converts

    Public Delegate Function IMakeDataFrame(x As Object, args As list, env As Environment) As dataframe

    ''' <summary>
    ''' ``as.data.frame``
    ''' </summary>
    Public Module makeDataframe

        ReadOnly makesDataframe As New Dictionary(Of Type, IMakeDataFrame)

        Sub New()
            makesDataframe(GetType(ExceptionData)) = AddressOf TracebackDataFrmae
        End Sub

        ''' <summary>
        ''' Public <see cref="Global.System.Delegate"/> Function IMakeDataFrame(
        '''     x As <see cref="Object"/>, 
        '''     args As <see cref="list"/>, 
        '''     env As <see cref="Environment"/>
        ''' ) As <see cref="dataframe"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="handler"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub [addHandler](type As Type, handler As IMakeDataFrame)
            makesDataframe(type) = handler
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function is_ableConverts(type As Type) As Boolean
            Return makesDataframe.ContainsKey(type)
        End Function

        Public Function tryTypeLineage(type As Type) As Type
            For Each base As Type In makesDataframe.Keys
                If type.IsInheritsFrom(base) Then
                    Return base
                End If
            Next

            Return Nothing
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function createDataframe(type As Type, x As Object, args As list, env As Environment) As dataframe
            Return makesDataframe(type)(x, args, env)
        End Function

        Private Function TracebackDataFrmae(data As Object, args As list, env As Environment) As dataframe
            Dim stacktrace As StackFrame()

            If TypeOf data Is ExceptionData Then
                stacktrace = DirectCast(data, ExceptionData).StackTrace
            ElseIf TypeOf data Is StackFrame() Then
                stacktrace = DirectCast(data, StackFrame())
            Else
                Throw New NotImplementedException
            End If

            Dim package As Array = stacktrace.Select(Function(a) a.Method.Namespace).ToArray
            Dim [module] As Array = stacktrace.Select(Function(a) a.Method.Module).ToArray
            Dim name As Array = stacktrace.Select(Function(a) a.Method.Method).ToArray
            Dim file As Array = stacktrace.Select(Function(a) a.File).ToArray
            Dim line As Array = stacktrace.Select(Function(a) a.Line).ToArray
            Dim dataframe As New dataframe With {
                .columns = New Dictionary(Of String, Array) From {
                    {NameOf(package), package},
                    {NameOf([module]), [module]},
                    {NameOf(name), name},
                    {NameOf(file), file},
                    {NameOf(line), line}
                }
            }

            Return dataframe
        End Function

        <Extension>
        Public Function CheckDimension(data As dataframe, env As Environment) As Object
            If data.columns.IsNullOrEmpty OrElse data.columns.Count = 1 Then
                Return data
            End If

            Dim max As Integer = data.nrows
            Dim noneAgree As (key$, size%)() = data.columns _
                .Where(Function(col)
                           Return col.Value.Length <> max AndAlso col.Value.Length <> 1
                       End Function) _
                .Select(Function(col) (col.Key, col.Value.Length)) _
                .ToArray

            If noneAgree.Length > 0 Then
                Dim msg As New List(Of String) From {"arguments imply differing number of rows"}

                For Each col In data.columns
                    Call msg.Add($"{col.Key}: {col.Value.Length}")
                Next

                Return Internal.debug.stop(msg.ToArray, env)
            Else
                Return data
            End If
        End Function

        <Extension>
        Public Iterator Function PopulateDataSet(Of T As {New, INamedValue, DynamicPropertyBase(Of String)})(data As dataframe) As IEnumerable(Of T)
            Dim allNames As String() = data.colnames
            Dim vecRow As Dictionary(Of String, String)
            Dim obj As T

            For Each row In data.forEachRow(colKeys:=allNames)
                vecRow = New Dictionary(Of String, String)

                For i As Integer = 0 To allNames.Length - 1
                    vecRow(allNames(i)) = any.ToString(row(i))
                Next

                obj = New T With {.Key = row.name}
                obj.Properties = vecRow

                Yield obj
            Next
        End Function

        <Extension>
        Public Function RDataframe(Of T As {INamedValue, DynamicPropertyBase(Of String)})(rows As IEnumerable(Of T)) As dataframe
            Dim columns As New Dictionary(Of String, List(Of String))
            Dim all As T() = rows.ToArray
            Dim allProps As String() = all _
                .Select(Function(t1) t1.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray
            Dim names As New List(Of String)

            For Each name As String In allProps
                columns.Add(name, New List(Of String))
            Next

            For Each row As T In all
                For Each name As String In allProps
                    Call columns(name).Add(row(name))
                Next

                Call names.Add(row.Key)
            Next

            Return New dataframe With {
                .columns = columns _
                    .ToDictionary(Function(c) c.Key,
                                  Function(c)
                                      Return DirectCast(c.Value.ToArray, Array)
                                  End Function),
                .rownames = names.ToArray
            }
        End Function

        ''' <summary>
        ''' Internal implementation for <see cref="base.RDataframe"/> api
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RDataframe(values As IEnumerable(Of NamedValue(Of Object)), env As Environment) As Object
            Dim columns As New List(Of NamedValue(Of Object))

            For Each item In values
                If Program.isException(item.Value) Then
                    Return item.Value
                Else
                    columns.Add(item)
                End If
            Next

            If columns.Count > 1 Then
                Return New dataframe With {
                    .columns = columns _
                        .ToDictionary(Function(a) a.Name,
                                      Function(a)
                                          Return env.createColumnVector(a.Value)
                                      End Function)
                }.CheckDimension(env)
            ElseIf columns.Count = 1 Then
                Dim first As NamedValue(Of Object) = columns.First

                If first.IsEmpty Then
                    Return Nothing
                ElseIf TypeOf first.Value Is list Then
                    Return DirectCast(first.Value, list).fromList(env)
                Else
                    Return New dataframe With {
                        .columns = New Dictionary(Of String, Array) From {
                            {first.Name, env.createColumnVector(first.Value)}
                        }
                    }
                End If
            Else
                ' create a new empty dataframe object
                Return New dataframe With {
                    .columns = New Dictionary(Of String, Array)
                }
            End If
        End Function

        <Extension>
        Friend Function fromList(firstList As list, env As Environment) As Object
            ' passing parameter by a list
            Dim matrix As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }
            Dim colName$
            Dim colVal As Object
            Dim listColumns As New List(Of NamedValue(Of list))
            Dim vectorArray As Array

            ' set null to a column is delete a column data in R
            ' so we skip all of the null field value at here
            For Each col In firstList.slots.Where(Function(a) Not a.Value Is Nothing)
                colName = col.Key
                colVal = col.Value

                If TypeOf colVal Is list Then
                    listColumns += New NamedValue(Of list) With {
                        .Name = colName,
                        .Value = DirectCast(colVal, list)
                    }
                Else
                    vectorArray = env.createColumnVector(colVal)
                    matrix.columns(colName) = vectorArray
                End If
            Next

            If listColumns = firstList.slots.Count Then
                Dim rownames As String() = listColumns _
                    .Values _
                    .Select(Function(c) c.slots.Keys) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray
                Dim vector As New List(Of Object)
                Dim listData As Dictionary(Of String, Object)

                For Each col In listColumns
                    listData = col.Value.slots

                    For Each rId As String In rownames
                        vector.Add(listData.TryGetValue(rId))
                    Next

                    vectorArray = env.createColumnVector(vector.PopAll)
                    matrix.columns(col.Name) = vectorArray
                Next

                matrix.rownames = rownames
            ElseIf listColumns > 0 AndAlso matrix.columns.Count > 0 Then
                Return Internal.debug.stop(New InvalidCastException, env)
            End If

            Return matrix.CheckDimension(env)
        End Function

        <Extension>
        Private Function createColumnVector(env As Environment, a As Object) As Array
            ' 假设dataframe之中每一列数据的类型都是相同的
            ' 则我们直接使用第一个元素的类型作为列的数据类型
            Dim array As Array = REnv.asVector(Of Object)(a)
            Dim generic As Array = REnv.TryCastGenericArray(array, env)

            Return generic
        End Function
    End Module
End Namespace
