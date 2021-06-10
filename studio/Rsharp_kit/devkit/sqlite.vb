Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

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

    <ExportAPI("ls")>
    Public Function list(con As Sqlite3Database) As dataframe
        Dim tables As Sqlite3SchemaRow() = con.GetTables.ToArray
        Dim summary As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        summary.columns("name") = tables.Select(Function(t) t.name).ToArray
        summary.columns("rootPage") = tables.Select(Function(t) t.rootPage).ToArray
        summary.columns("tableName") = tables.Select(Function(t) t.tableName).ToArray
        summary.columns("type") = tables.Select(Function(t) t.type).ToArray
        summary.columns("sql") = tables.Select(Function(t) t.Sql.TrimNewLine.Trim).ToArray

        Return summary
    End Function

    <ExportAPI("load")>
    Public Function fetchTable(con As Sqlite3Database, tableName As String, Optional env As Environment = Nothing) As dataframe
        Dim rawRef As Sqlite3Table = con.GetTable(tableName)
        Dim rows As Sqlite3Row() = rawRef.EnumerateRows.ToArray
        Dim schema As Schema = rawRef.SchemaDefinition.ParseSchema
        Dim colnames As String() = schema.columns.Select(Function(c) c.Name).ToArray
        Dim table As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        For i As Integer = 0 To colnames.Length - 1
            table.columns(colnames(i)) = rows.Select(Function(r) r.ColumnData(i)).ToArray
            table.columns(colnames(i)) = REnv.TryCastGenericArray(table.columns(colnames(i)), env)
        Next

        Return table
    End Function

End Module
