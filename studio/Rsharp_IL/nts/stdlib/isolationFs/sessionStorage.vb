
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace jsstd.isolationFs

    <Package("sessionStorage")>
    Public Module sessionStorage

        ReadOnly fs As Storage

        Sub New()
            fs = New Storage(TempFileSystem.GetAppSysTempFile("_session", sessionID:=App.PID.ToHexString, prefix:="sessionStorage_"))
            App.JoinVariable("js.sessionStorage", fs.ToString)
        End Sub

        <ExportAPI("clear")>
        Public Function clear() As Object
            Return fs.clear
        End Function

        <ExportAPI("removeItem")>
        Public Function removeItem(keyname As String) As Object
            Return fs.removeItem(keyname)
        End Function

        <ExportAPI("setItem")>
        Public Function setItem(key As String, <RRawVectorArgument> value As Object,
                                Optional env As Environment = Nothing) As Object
            Return fs.setItem(key, value, env)
        End Function

        <ExportAPI("getItem")>
        Public Function getItem(key As String, Optional env As Environment = Nothing) As Object
            Return fs.getItem(key, env)
        End Function
    End Module
End Namespace