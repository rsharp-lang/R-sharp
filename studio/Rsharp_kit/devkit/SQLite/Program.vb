Imports System.Data.SQLite
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema
Imports Microsoft.VisualBasic.CommandLine

Module Program

    ' 1. print all table names in database
    ' SqliteExport file.db
    '
    ' 2. export by table name
    ' SqliteExport /export /db <database.sqlite3> /table <tableName> 

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine, executeFile:=AddressOf printTableNames)
    End Function

    Private Function printTableNames(dbFile As String, args As CommandLine) As Integer
        Dim names = GetAllTableNames(dbFile)

        Call names.DoEach(AddressOf Console.WriteLine)

        Return 0
    End Function

    Public Sub SelectQueryTable(dbFile$, selectQuery$, rowAction As Func(Of Integer, Object(), Boolean))
        Dim connStr$ = $"data source={dbFile.GetFullPath};version=3;"
        Dim cn As New SQLiteConnection(connStr)
        Call cn.Open()

        Dim cmd = cn.CreateCommand()
        cmd.CommandText = selectQuery

        Dim row As New List(Of Object)
        Dim rid As i32 = Scan0

        Using reader As SQLiteDataReader = cmd.ExecuteReader()
            Do While reader.Read()
                For i As Integer = 0 To reader.FieldCount - 1
                    Call row.Add(reader(i))
                Next

                If rowAction(++rid, row.ToArray) Then
                    Exit Do
                Else
                    Call row.Clear()
                End If
            Loop
        End Using
    End Sub

    Public Function GetAllTableNames(dbFile As String) As String()
        Dim tableNames As New List(Of String)
        Dim trygetTableName =
            Function(rid As Integer, names As Object())
                ' CREATE TABLE sqlite_master (type TEXT, name TEXT, tbl_name TEXT, rootpage INTEGER, sql TEXT);
                If CStr(names(Scan0)) = "table" Then
                    tableNames.Add(names(2))
                End If

                Return False
            End Function

        Call Program.SelectQueryTable(dbFile, $"SELECT * FROM sqlite_master;", trygetTableName)

        Return tableNames _
            .OrderBy(Function(s) s) _
            .ToArray
    End Function
End Module