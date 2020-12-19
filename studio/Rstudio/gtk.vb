#Region "Microsoft.VisualBasic::fd411e0332de9b5047d83555ac31bb4a, studio\Rstudio\gtk.vb"

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

    ' Module gtk
    ' 
    '     Function: selectFiles, selectFolder
    ' 
    ' /********************************************************************************/

#End Region

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
