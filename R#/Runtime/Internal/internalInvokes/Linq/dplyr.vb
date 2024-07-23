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

            If _id Is Nothing Then
                get_id = Function(df) Nothing
            Else
                Dim type As RType = RType.TypeOf(_id)

                If type.mode.IsNumeric Then
                    get_id = Function(df) df(df.colnames(CLRVector.asInteger(_id).First))
                Else
                    get_id = Function(df) df(CLRVector.asCharacter(_id).First)
                End If
            End If

            Dim columns As New Dictionary(Of String, List(Of Object))
            Dim nrows As Integer = 0

            For Each df_obj As Object In x.data
                If df_obj Is Nothing Then
                    Continue For
                End If
                If Not TypeOf df_obj Is dataframe Then
                    Return Message.InCompatibleType(GetType(dataframe), df_obj.GetType, env)
                End If

                Dim df As dataframe = df_obj

                For Each col In df.columns
                    If Not columns.ContainsKey(col.Key) Then
                        columns(col.Key) = Replicate(Of Object)(Nothing, nrows).AsList
                    End If

                    columns(col.Key).AddRange(col.Value.AsObjectEnumerator)
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