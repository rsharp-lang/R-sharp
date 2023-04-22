Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace jsstd.isolationFs

    Public Class Storage

        ReadOnly fs As String

        Sub New(dir As String)
            fs = dir.GetDirectoryFullPath
        End Sub

        Public Overrides Function ToString() As String
            Return fs
        End Function

        Private Function getStoragePath(key As String) As String
            Return $"{fs}/{key.NormalizePathString(False)}_{key.MD5}.json"
        End Function

        Public Function clear() As Object
            For Each file As String In fs.ListFiles("*.json")
                Call file.DeleteFile
            Next

            Return True
        End Function

        Public Function removeItem(keyname As String) As Object
            Return getStoragePath(keyname).DeleteFile
        End Function

        <ExportAPI("setItem")>
        Public Function setItem(key As String, <RRawVectorArgument> value As Object,
                                Optional env As Environment = Nothing) As Object

            Dim fileName As String = getStoragePath(key)
            Dim json As String = jsonlite.toJSON(value, env)

            Return json.SaveTo(fileName)
        End Function

        <ExportAPI("getItem")>
        Public Function getItem(key As String, Optional env As Environment = Nothing) As Object
            Dim fileName As String = getStoragePath(key)
            Dim json As String = fileName.ReadAllText
            Dim obj = jsstd.JSON.parse(json, env)

            Return obj
        End Function
    End Class
End Namespace