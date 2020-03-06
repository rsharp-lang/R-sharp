#Region "Microsoft.VisualBasic::275bfb5b410fed7c84007887752c5d4c, R#\Runtime\Internal\objects\RConversion\makeDataframe.vb"

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
    '         Function: createColumnVector, createDataframe, is_ableConverts, RDataframe, TracebackDataFrmae
    ' 
    '         Sub: [addHandler]
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Runtime.Internal.Object.Converts

    Public Delegate Function IMakeDataFrame(x As Object, args As list, env As Environment) As dataframe

    Public Module makeDataframe

        ReadOnly makesDataframe As New Dictionary(Of Type, IMakeDataFrame)

        Sub New()
            makesDataframe(GetType(ExceptionData)) = AddressOf TracebackDataFrmae
        End Sub

        ''' <summary>
        ''' Public <see cref="Global.System.Delegate"/> Function IMakeDataFrame(x As <see cref="Object"/>, args As <see cref="list"/>, env As <see cref="Environment"/>) As <see cref="dataframe"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="handler"></param>
        Public Sub [addHandler](type As Type, handler As IMakeDataFrame)
            makesDataframe(type) = handler
        End Sub

        Public Function is_ableConverts(type As Type) As Boolean
            Return makesDataframe.ContainsKey(type)
        End Function

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
                }
            ElseIf columns.Count = 1 Then
                Dim first As NamedValue(Of Object) = columns.First

                If first.IsEmpty Then
                    Return Nothing
                ElseIf TypeOf first.Value Is list Then
                    ' passing parameter by a list
                    Dim matrix As New dataframe With {
                        .columns = New Dictionary(Of String, Array)
                    }
                    Dim colName$
                    Dim colVal As Object
                    Dim listColumns As New List(Of NamedValue(Of list))

                    ' set null to a column is delete a column data in R
                    ' so we skip all of the null field value at here
                    For Each col In DirectCast(first.Value, list).slots.Where(Function(a) Not a.Value Is Nothing)
                        colName = col.Key
                        colVal = col.Value

                        If TypeOf colVal Is list Then
                            listColumns += New NamedValue(Of list) With {
                                .Name = colName,
                                .Value = DirectCast(colVal, list)
                            }
                        Else
                            matrix.columns.Add(colName, env.createColumnVector(colVal))
                        End If
                    Next

                    If listColumns = DirectCast(first.Value, list).slots.Count Then
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

                            Call matrix.columns.Add(col.Name, env.createColumnVector(vector.PopAll))
                        Next

                        matrix.rownames = rownames
                    ElseIf listColumns > 0 AndAlso matrix.columns.Count > 0 Then
                        Return Internal.stop(New InvalidCastException, env)
                    End If

                    Return matrix
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
        Private Function createColumnVector(env As Environment, a As Object) As Array
            ' 假设dataframe之中每一列数据的类型都是相同的
            ' 则我们直接使用第一个元素的类型作为列的数据类型
            Dim first As Object = Runtime.getFirst(a, nonNULL:=True)
            Dim colVec As Array = Runtime.asVector(a, first.GetType, env)

            Return colVec
        End Function
    End Module
End Namespace
