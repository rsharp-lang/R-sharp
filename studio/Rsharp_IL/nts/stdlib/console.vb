Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace jsstd

    <Package("console")>
    Public Module console

        <ExportAPI("log")>
        Public Function log(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing)
            If TypeOf x Is String Then
                Call base.cat(CStr(x) & vbCrLf, env:=env)
            Else
                Dim json As Object = jsonlite.toJSON(x, env)

                If Program.isException(json) Then
                    Return json
                Else
                    Call base.cat(CStr(json) & vbCrLf, env:=env)
                End If
            End If

            Return Nothing
        End Function

        <ExportAPI("table")>
        Public Function table(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing)
            Dim data = pipeline.TryCreatePipeline(Of list)(x, env)

            If data.isError Then
                Return data.getError
            End If

            Dim listArray As list() = data.populates(Of list)(env).ToArray
            Dim union_names As String() = listArray.Select(Function(l) l.getNames).IteratesALL.Distinct.ToArray
            Dim df As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = Enumerable _
                    .Range(1, listArray.Length) _
                    .Select(Function(i) i.ToString) _
                    .ToArray
            }

            For Each name As String In union_names
                Call df.add(name, listArray.Select(Function(l) l.getByName(name)))
            Next

            Return base.print(df, env:=env)
        End Function
    End Module
End Namespace