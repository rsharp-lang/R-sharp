
Imports System.Environment
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("gtk")>
Module gtk

    <ExportAPI("selectFolder")>
    Public Function selectFolder(Optional default$ = "",
                                 Optional desc As String = Nothing,
                                 Optional newFolder As Boolean = False,
                                 Optional root As SpecialFolder = SpecialFolder.MyDocuments) As String

        Using folder As New FolderBrowserDialog With {
            .ShowNewFolderButton = newFolder,
            .RootFolder = root,
            .Description = desc,
            .SelectedPath = [default]
        }
            If folder.ShowDialog = DialogResult.OK Then
                Return folder.SelectedPath
            Else
                Return Nothing
            End If
        End Using
    End Function

    <ExportAPI("selectFiles")>
    Public Function selectFiles(Optional title$ = Nothing,
                                <RRawVectorArgument>
                                Optional filter As Object = "*.*|*.*",
                                Optional forSave As Boolean = False) As String()
        Dim filters As String() = REnv.asVector(Of String)(filter)

        If forSave Then
            Using file As New SaveFileDialog With {.Title = title, .Filter = filters.JoinBy("|")}
                If file.ShowDialog = DialogResult.OK Then
                    Return file.FileNames
                Else
                    Return Nothing
                End If
            End Using
        Else
            Using file As New OpenFileDialog With {.Title = title, .Filter = filters.JoinBy("|")}
                If file.ShowDialog = DialogResult.OK Then
                    Return file.FileNames
                Else
                    Return Nothing
                End If
            End Using
        End If
    End Function
End Module
