Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' table reader for sqlite 3 database file
''' </summary>
<Package("sqlite")>
Module sqlite

    <ExportAPI("open")>
    <RApiReturn(GetType(Sqlite3Database))>
    Public Function open(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim con = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If con Like GetType(Message) Then
            Return con.TryCast(Of Message)
        End If

        Return New Sqlite3Database(con.TryCast(Of Stream))
    End Function
End Module
