Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Utils
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
    ''' <returns></returns>
    <Extension>
    Friend Function DataFrameRows(x As Rdataframe, row_names As Object, formatNumber As String, env As Environment) As File
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
            inputRowNames = REnv.asVector(Of String)(row_names)
            row_names = False
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
