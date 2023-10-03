Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Utils
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports File = Microsoft.VisualBasic.Data.csv.IO.File
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.[Object].dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

Public Module dataframeWriter

    ''' <summary>
    ''' create R dataframe object as sciBASIC csv table file model
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="row_names"></param>
    ''' <param name="env"></param>
    ''' <param name="formatNumber">
    ''' Nothing or the .net clr numeric format string
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Friend Function DataFrameRows(x As Rdataframe, row_names As Object, formatNumber As String, env As Environment) As [Variant](Of Message, File)
        Dim inputRowNames As String() = Nothing

        If row_names Is Nothing Then
            row_names = True
        End If
        If row_names.GetType Is GetType(vector) Then
            row_names = DirectCast(row_names, vector).data
        End If
        If row_names.GetType Is GetType(Array) Then
            If DirectCast(row_names, Array).Length = 1 Then
                row_names = DirectCast(row_names, Array).GetValue(Scan0)
            End If
        End If
        If Not TypeOf row_names Is Boolean Then
            inputRowNames = CLRVector.asCharacter(row_names)
            row_names = False
        End If

        If inputRowNames IsNot Nothing AndAlso inputRowNames.Length <> x.nrows Then
            Return Internal.debug.stop({
                $"The given row.names size({inputRowNames.Length}) from the function parameter is not matched with the dataframe row counts({x.nrows})!",
                $"Please check of the dataframe value object or the row.names parameter!",
                $"input_rownames_size: {inputRowNames.Length}",
                $"nrows_dataframe: {x.nrows}"
            }, env)
        End If

        x = New Rdataframe(x)

        For Each name As String In x.colnames
            Dim v As Array = x.columns(name)

            If TypeOf v Is Double() Then
                v = DirectCast(v, Double()) _
                    .Select(Function(d) d.ToString(formatNumber)) _
                    .ToArray
            ElseIf TypeOf v Is Object() Then
                v = REnv.TryCastGenericArray(v, env)

                If TypeOf v Is Double() Then
                    v = DirectCast(v, Double()) _
                        .Select(Function(d) d.ToString(formatNumber)) _
                        .ToArray
                End If
            End If

            x.columns(name) = v
        Next

        ' 20230209
        ' row.names = TRUE
        ' then row names generates from this helper function
        ' and the inputRowNames is set to nothing
        Dim matrix As String()() = TableFormatter _
            .GetTable(
                df:=x,
                env:=env.globalEnvironment,
                printContent:=False,
                showRowNames:=row_names
            )
        Dim rows As IEnumerable(Of RowObject) = matrix _
            .Select(Function(r, i)
                        If inputRowNames Is Nothing Then
                            ' row name is already includes in r!
                            ' the table formatter handing the row names value
                            Return New RowObject(r)
                        ElseIf i = 0 Then
                            ' header row
                            Return New RowObject({""}.JoinIterates(r))
                        Else
                            Return New RowObject({inputRowNames(i - 1)}.JoinIterates(r))
                        End If
                    End Function) _
            .ToArray
        Dim dataframe As New File(rows)

        Return dataframe
    End Function
End Module
