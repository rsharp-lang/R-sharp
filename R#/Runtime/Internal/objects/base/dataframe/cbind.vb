#Region "Microsoft.VisualBasic::564c4f40b4b66a299616a73c4fbca0e7, R#\Runtime\Internal\objects\base\dataframe\cbind.vb"

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

    '   Total Lines: 235
    '    Code Lines: 166 (70.64%)
    ' Comment Lines: 38 (16.17%)
    '    - Xml Docs: 63.16%
    ' 
    '   Blank Lines: 31 (13.19%)
    '     File Size: 9.88 KB


    '     Module cbindOp
    ' 
    '         Function: castPartData, (+2 Overloads) cbind, columnCombine, dataframeJoinListAsColumns, strictColumnAppend
    ' 
    '         Sub: safeAddColumn
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].baseOp.dataframeOp
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.baseOp.dataframeOp

    Public Module cbindOp

        ''' <summary>
        ''' just try to cast the given part of data as dataframe
        ''' </summary>
        ''' <param name="nameKey"></param>
        ''' <param name="col"></param>
        ''' <param name="strict"></param>
        ''' <param name="[default]"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function castPartData(nameKey As String,
                                         col As Object,
                                         strict As Boolean,
                                         [default] As Object,
                                         env As Environment) As [Variant](Of Message, dataframe)
            If col Is Nothing Then
                Return Nothing
            ElseIf TypeOf col Is dataframe Then
                Return DirectCast(col, dataframe)
            ElseIf TypeOf col Is vector OrElse col.GetType.IsArray Then
                ' cast the vector as dataframe
                ' a dataframe with just single column,
                ' and the column name is the parameter name
                Return New dataframe With {
                    .rownames = Nothing,
                    .columns = New Dictionary(Of String, Array) From {
                        {nameKey, REnv.asVector(Of Object)(col)}
                    }
                }
            ElseIf TypeOf col Is list Then
                ' list names as row names
                Return New dataframe With {
                    .rownames = DirectCast(col, list).getNames,
                    .columns = New Dictionary(Of String, Array) From {
                        {nameKey, DirectCast(col, list).GetVector(.rownames)}
                    }
                }
            Else
                Return Message.InCompatibleType(GetType(dataframe), col.GetType, env)
            End If
        End Function

        <Extension>
        Private Function dataframeJoinListAsColumns(d As dataframe, cols As list) As dataframe
            Dim value As Object

            For Each name As String In cols.slots.Keys
                value = cols.slots(name)

                If Not value Is Nothing Then
                    If TypeOf value Is Array Then
                        Call safeAddColumn(d, name, DirectCast(value, Array))
                    Else
                        Call safeAddColumn(d, name, {value})
                    End If
                End If
            Next

            Return d
        End Function

        ''' <summary>
        ''' try to combine two dataframe by columns
        ''' </summary>
        ''' <param name="d"></param>
        ''' <param name="nameKey"></param>
        ''' <param name="col"></param>
        ''' <param name="strict"></param>
        ''' <param name="[default]">the default fill value for the missing row data if 
        ''' the row numbers between two dataframe is mis-matched, and safe mode is
        ''' enabled.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the duplicated column name will be renamed
        ''' </remarks>
        <Extension>
        Private Function columnCombine(d As dataframe, nameKey As String,
                                       col As Object,
                                       strict As Boolean,
                                       [default] As Object,
                                       env As Environment) As [Variant](Of Message, dataframe)

            If TypeOf col Is list Then
                Return d.dataframeJoinListAsColumns(col)
            ElseIf TypeOf col Is Array Then
                Call safeAddColumn(d, nameKey, DirectCast(col, Array))
            ElseIf TypeOf col Is vector Then
                Call safeAddColumn(d, nameKey, DirectCast(col, vector).data)
            ElseIf TypeOf col Is dataframe Then
                Return cbind(d, DirectCast(col, dataframe), strict, [default], env)
            Else
                ' add a scalar value as new dataframe column
                ' the new column is scalar
                Call safeAddColumn(d, nameKey, {col})
            End If

            Return d
        End Function

        Private Function cbind(d As dataframe, append As dataframe, strict As Boolean, [default] As Object, env As Environment) As dataframe
            If Not strict Then
                Dim colnames As String() = d.columns.Keys.ToArray
                Dim oldColNames = append.columns.Keys.ToArray
                ' handling of the possible duplicated names
                Dim newNames As String() = colnames _
                    .JoinIterates(oldColNames) _
                    .UniqueNames

                ' dataframe will be merged directly
                ' this needs the row order between the 
                ' two dataframe object keeps the same
                For i As Integer = 0 To append.columns.Count - 1
                    d.columns.Add(newNames(i + colnames.Length), append.columns(oldColNames(i)))
                Next
            Else
                d = strictColumnAppend(d, append, [default], env)
            End If

            Return d
        End Function

        Private Function strictColumnAppend(df As dataframe, y As dataframe, [default] As Object, env As Environment) As dataframe
            If y Is Nothing Then
                Return df
            End If

            Dim df_names = df.colnames
            Dim y_names = y.colnames
            Dim union_names = df_names.JoinIterates(y_names).UniqueNames.ToArray
            Dim df_rows = df.forEachRow(df_names).ToDictionary
            Dim y_rows = y.forEachRow(y_names).ToArray

            For Each row As NamedCollection(Of Object) In y_rows
                Dim a As Object() = Nothing
                Dim b As Object() = row.value
                Dim v As Object() = Replicate([default], union_names.Length).ToArray

                If df_rows.ContainsKey(row.name) Then
                    a = df_rows(row.name).value

                    If a.Length = v.Length AndAlso b.Length > 0 Then
                        ' the duplicated name cause this problem
                        ' ignores?
                        Call env.AddMessage($"Found a duplicated key('{row.name}') while do dataframe cbind!")

                        GoTo SKIP
                    End If
                Else
                    a = Replicate([default], df_names.Length).ToArray

                    Call env.AddMessage($"Missing key('{row.name}') while do dataframe cbind!")
                End If

                Call Array.ConstrainedCopy(a, Scan0, v, Scan0, a.Length)
                Call Array.ConstrainedCopy(b, Scan0, v, a.Length, b.Length)
SKIP:
                df_rows(row.name) = New NamedCollection(Of Object) With {
                    .name = row.name,
                    .value = v.ToArray
                }
            Next

            For Each name As String In df_rows.Keys.ToArray
                If df_rows(name).Length <> union_names.Length Then
                    ' append null
                    Dim v = Replicate([default], union_names.Length).ToArray
                    Array.ConstrainedCopy(df_rows(name).value, Scan0, v, Scan0, df_rows(name).Length)
                    df_rows(name) = New NamedCollection(Of Object) With {
                        .name = name,
                        .value = v
                    }
                End If
            Next

            ' cast back to dataframe
            Dim union_data As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = df_rows.Keys.ToArray
            }

            For i As Integer = 0 To union_names.Length - 1
                Dim key As String = union_names(i)
                Dim idx As Integer = i
                Dim v As Object() = union_data.rownames _
                    .Select(Function(n)
                                Return df_rows(n).value(idx)
                            End Function) _
                    .ToArray
                Dim vec As Array = REnv.TryCastGenericArray(v, env)

                Call union_data.columns.Add(key, vec)
            Next

            Return union_data
        End Function

        Private Sub safeAddColumn(d As dataframe, namekey As String, col As Array)
            If d.hasName(namekey) Then
                namekey = $"X{d.columns.Count + 2}"
            End If

            Call d.columns.Add(namekey, col)
        End Sub

        <Extension>
        Public Function cbind(d As dataframe, nameKey As String,
                              col As Object,
                              strict As Boolean,
                              [default] As Object,
                              env As Environment) As [Variant](Of Message, dataframe)

            If d Is Nothing Then
                Return castPartData(nameKey, col, strict, [default], env)
            Else
                Return d.columnCombine(nameKey, col, strict, [default], env)
            End If
        End Function

    End Module
End Namespace
