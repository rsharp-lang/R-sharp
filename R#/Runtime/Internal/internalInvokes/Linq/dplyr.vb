#Region "Microsoft.VisualBasic::444970d24d9dd70c5d610b7e84792a4d, R#\Runtime\Internal\internalInvokes\Linq\dplyr.vb"

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

    '   Total Lines: 204
    '    Code Lines: 127 (62.25%)
    ' Comment Lines: 44 (21.57%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 33 (16.18%)
    '     File Size: 8.08 KB


    '     Module dplyr
    ' 
    '         Function: bind_rows
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports renv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes.LinqPipeline

    <Package("dplyr")>
    Module dplyr

        ''' <summary>
        ''' Bind multiple data frames by row
        ''' 
        ''' Bind any number of data frames by row, making a longer result. 
        ''' This is similar to do.call(rbind, dfs), but the output will 
        ''' contain all columns that appear in any of the inputs.
        ''' </summary>
        ''' <param name="x">
        ''' Data frames To combine. Each argument can either be a data frame, 
        ''' a list that could be a data frame, Or a list Of data frames.
        ''' Columns are matched by name, And any missing columns will be
        ''' filled With NA.
        ''' </param>
        ''' <param name="_id">
        ''' The name Of an Optional identifier column. Provide a String To 
        ''' create an output column that identifies Each input. The column 
        ''' will use names If available, otherwise it will use positions.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A data frame the same type as the first element of ....
        ''' </returns>
        ''' <example>
        ''' df1 &lt;- tibble(x = 1:2, y = letters[1:2])
        ''' df2 &lt;- tibble(x = 4:5, z = 1:2)
        ''' 
        ''' # You can supply individual data frames as arguments:
        ''' bind_rows(df1, df2)
        ''' 
        ''' # Or a list of data frames:
        ''' bind_rows(list(df1, df2))
        ''' 
        ''' # When you supply a column name with the `.id` argument, a new
        ''' # column is created to link each row to its original data frame
        ''' bind_rows(list(df1, df2), .id = "id")
        ''' bind_rows(list(a = df1, b = df2), .id = "id")
        ''' </example>
        <ExportAPI("bind_rows")>
        Public Function bind_rows(<RListObjectArgument>
                                  x As list,
                                  Optional _id As Object = Nothing,
                                  Optional env As Environment = Nothing) As Object

            Dim get_id As Func(Of dataframe, String())

            If x.hasName(".id") AndAlso Not TypeOf x.getByName(".id") Is dataframe Then
                _id = x.getByName(".id")
                x.setByName(".id", Nothing, env)
            End If

            If x.length = 1 Then
                If TypeOf x.data.First Is list Then
                    x = x.data.First
                Else
                    Throw New InvalidCastException("invalid data type for the required input dataframe list!")
                End If
            ElseIf x.length = 0 Then
                ' no dataframe to combine
                Return Nothing
            End If

            If _id Is Nothing Then
                get_id = Function(df) Nothing
            Else
                Dim type As RType = RType.TypeOf(_id)

                ' get rownames for the new generated dataframe object
                If type.mode.IsNumeric Then
                    get_id = Function(df) CLRVector.asCharacter(df(df.colnames(CLRVector.asInteger(_id).First)))
                Else
                    get_id = Function(df) CLRVector.asCharacter(df(CLRVector.asCharacter(_id).First))
                End If
            End If

            ' check value is all vector data
            ' which means combine all vector as dataframe, each vector is a row inside the dataframe
            If x.data.All(Function(a) (a Is Nothing) OrElse (TypeOf a Is vector) OrElse a.GetType.IsArray) Then
                Dim vector_combines As New List(Of Array)
                Dim rownameSet As New List(Of String)

                For Each rowTuple In x.slots
                    Dim row = rowTuple.Value

                    If row Is Nothing Then
                        Continue For
                    End If
                    If TypeOf row Is vector Then
                        row = DirectCast(row, vector).data
                    End If

                    Call rownameSet.Add(rowTuple.Key)
                    Call vector_combines.Add(row)
                Next

                ' cast the row array collection as matrix
                Dim mat As New dataframe With {
                    .columns = New Dictionary(Of String, Array),
                    .rownames = rownameSet.ToArray
                }
                Dim maxCols As Integer = Aggregate arr In vector_combines Into Max(arr.Length)

                For i As Integer = 0 To maxCols - 1
                    Dim offset As Integer = i
                    Dim v = vector_combines _
                        .Select(Function(vi)
                                    If offset < vi.Length Then
                                        Return vi(offset)
                                    Else '
                                        Return Nothing
                                    End If
                                End Function) _
                        .ToArray

                    mat.columns("V" & (i + 1)) = renv.UnsafeTryCastGenericArray(v)
                Next

                Return mat
            End If

            Dim all_dfs As New List(Of dataframe)

            For Each df_obj As Object In x.data
                If df_obj Is Nothing Then
                    Continue For
                End If

                If TypeOf df_obj Is list Then
                    ' is a row?
                    ' cast to a dataframe with only one row
                    Dim row As New dataframe With {.columns = New Dictionary(Of String, Array)}

                    For Each col In DirectCast(df_obj, list).slots
                        Call row.add(col.Key, CLRVector.asCharacter(col.Value))
                    Next

                    df_obj = row
                ElseIf Not TypeOf df_obj Is dataframe Then
                    Return Message.InCompatibleType(GetType(dataframe), df_obj.GetType, env)
                End If

                ' empty dataframe also treated as null
                If DirectCast(df_obj, dataframe).empty Then
                    Continue For
                End If

                Call all_dfs.Add(df_obj)
            Next

            Dim columns As New Dictionary(Of String, List(Of Object))
            Dim nrows As Integer = 0
            Dim allcols As String() = all_dfs _
                .Select(Function(d) d.colnames) _
                .IteratesALL _
                .Distinct _
                .ToArray

            For Each col As String In allcols
                Call columns.Add(col, New List(Of Object))
            Next

            For Each df As dataframe In all_dfs
                Dim num_rows As Integer = df.nrows

                For Each col As String In allcols
                    If Not df.hasName(col) Then
                        columns(col).AddRange(Replicate(Of Object)(Nothing, num_rows))
                    Else
                        columns(col).AddRange(df(col).AsObjectEnumerator)
                    End If
                Next

                nrows += df.nrows
            Next

            Dim binds As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }

            For Each col In columns
                Call binds.add(col.Key, renv.UnsafeTryCastGenericArray(col.Value.ToArray))
            Next

            Dim rownames As String() = get_id(binds)

            If Not rownames.IsNullOrEmpty Then
                binds.rownames = rownames.UniqueNames
            End If

            Return binds
        End Function
    End Module
End Namespace
