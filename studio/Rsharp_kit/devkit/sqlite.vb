#Region "Microsoft.VisualBasic::49914c6e71ff317372047dd31953dd57, F:/GCModeller/src/R-sharp/studio/Rsharp_kit/devkit//sqlite.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 111
    '    Code Lines: 66
    ' Comment Lines: 32
    '   Blank Lines: 13
    '     File Size: 4.18 KB


    ' Module sqlite
    ' 
    '     Function: fetchTable, list, open
    ' 
    ' /********************************************************************************/

#End Region

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

    ''' <summary>
    ''' Open a connection to a local sqlite database file
    ''' </summary>
    ''' <param name="file">
    ''' the file path to the target sqlite database file or the stream 
    ''' data of the sqlite file itself.
    ''' </param>
    ''' <param name="blobAsBase64">
    ''' 
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("open")>
    <RApiReturn(GetType(Sqlite3Database))>
    Public Function open(<RRawVectorArgument> file As Object,
                         Optional blobAsBase64 As Boolean = True,
                         Optional env As Environment = Nothing) As Object

        Dim con = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)
        Dim configs As New Sqlite3Settings With {
            .blobAsBase64 = blobAsBase64
        }

        If con Like GetType(Message) Then
            Return con.TryCast(Of Message)
        End If

        Return New Sqlite3Database(
            file:=con.TryCast(Of Stream),
            settings:=configs
        )
    End Function

    ''' <summary>
    ''' get all data object that stored in the target sqlite database.
    ''' </summary>
    ''' <param name="con">a connection to the sqlite database</param>
    ''' <param name="type">
    ''' the object type in the sqlite database
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("ls")>
    Public Function list(con As Sqlite3Database, Optional type As String = "table") As dataframe
        Dim tables As Sqlite3SchemaRow() = con.GetTables.ToArray
        Dim summary As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        If (Not type.StringEmpty) AndAlso type <> "*" Then
            tables = (From item In tables Where item.type = type).ToArray
        End If

        summary.columns("name") = tables.Select(Function(t) t.name).ToArray
        summary.columns("rootPage") = tables.Select(Function(t) t.rootPage).ToArray
        summary.columns("tableName") = tables.Select(Function(t) t.tableName).ToArray
        summary.columns("type") = tables.Select(Function(t) t.type).ToArray
        summary.columns("sql") = tables.Select(Function(t) t.Sql.TrimNewLine.Trim).ToArray

        Return summary
    End Function

    ''' <summary>
    ''' export target table data
    ''' </summary>
    ''' <param name="con">a connection to the sqlite database</param>
    ''' <param name="tableName">
    ''' the data table name
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("load")>
    Public Function fetchTable(con As Sqlite3Database, tableName As String, Optional env As Environment = Nothing) As dataframe
        Dim rawRef As Sqlite3Table = con.GetTable(tableName)
        Dim rows As Sqlite3Row() = rawRef.EnumerateRows.ToArray
        Dim schema As Schema = rawRef.SchemaDefinition.ParseSchema
        Dim colnames As String() = schema.columns _
            .Select(Function(c) c.Name) _
            .ToArray
        Dim table As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }
        Dim index As Integer

        For i As Integer = 0 To colnames.Length - 1
            index = i
            table.columns(colnames(i)) = rows.Select(Function(r) r.ColumnData(index)).ToArray
            table.columns(colnames(i)) = REnv.TryCastGenericArray(table.columns(colnames(i)), env)
        Next

        Return table
    End Function

End Module
