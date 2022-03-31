Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema

<CLI>
Module CLI

    <ExportAPI("/export")>
    <Usage("/export /db <database.sqlite3> /table <tableName> [/out <table.csv>]")>
    Public Function exportTable(args As CommandLine) As Integer
        Dim dbFile$ = args <= "/db"
        Dim tableName$ = args <= "/table"
        Dim out$ = args("/out") Or $"{dbFile.TrimSuffix}_{tableName}.csv"
        Dim fields As String() = Nothing
        Dim getFieldNames =
            Function(rid As Integer, names As Object())
                ' CREATE TABLE sqlite_master (type TEXT, name TEXT, tbl_name TEXT, rootpage INTEGER, sql TEXT);
                If CStr(names(Scan0)) = "table" AndAlso CStr(names(1)) = tableName Then
                    fields = New Schema(names(4)).columns.Keys
                    Return True
                Else
                    Return False
                End If
            End Function

        Call Program.SelectQueryTable(dbFile, $"SELECT * FROM sqlite_master;", getFieldNames)

        If fields.IsNullOrEmpty Then
            Call $"Could not found table '{tableName}' in database: {dbFile}".PrintException
            Return 500
        End If

        Dim SQL$ = $"SELECT {fields.JoinBy(", ")} FROM {tableName};"
        Dim export As New TableExport(fields)

        Call Program.SelectQueryTable(dbFile, SQL, AddressOf export.Fill)

        Return export.GetTable.SaveDataSet(out).CLICode
    End Function
End Module