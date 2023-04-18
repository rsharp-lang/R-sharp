Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
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
            Throw New NotImplementedException
        End Function
    End Module
End Namespace