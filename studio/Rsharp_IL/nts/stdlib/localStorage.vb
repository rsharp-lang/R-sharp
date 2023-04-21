Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace jsstd

    <Package("localStorage")>
    Public Module localStorage

        ReadOnly session_storage As String

        Sub New()
            session_storage = TempFileSystem.GetAppSysTempFile("_session", sessionID:=App.PID.ToHexString, prefix:="localStorage_")
        End Sub

        <ExportAPI("setItem")>
        Public Function setItem(key As String, <RRawVectorArgument> value As Object,
                                Optional env As Environment = Nothing) As Object

            Dim fileName As String = $"{session_storage}/{key.NormalizePathString(False)}.json"
            Dim json As String = jsonlite.toJSON(value, env)

            Return json.SaveTo(fileName)
        End Function

        <ExportAPI("getItem")>
        Public Function getItem(key As String, Optional env As Environment = Nothing) As Object
            Dim fileName As String = $"{session_storage}/{key.NormalizePathString(False)}.json"
            Dim json As String = fileName.ReadAllText
            Dim obj = jsstd.JSON.parse(json, env)

            Return obj
        End Function
    End Module
End Namespace