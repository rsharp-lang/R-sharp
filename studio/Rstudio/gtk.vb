#Region "Microsoft.VisualBasic::04dd12e76fd9d378dd63e5ce6610d580, F:/GCModeller/src/R-sharp/studio/Rstudio//gtk.vb"

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

    '   Total Lines: 120
    '    Code Lines: 81
    ' Comment Lines: 31
    '   Blank Lines: 8
    '     File Size: 4.63 KB


    ' Module gtk
    ' 
    '     Function: Dialog, selectFiles, selectFolder
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Environment
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' WinForm controls api for ``R#`` programming.
''' </summary>
<Package("gtk", Category:=APICategories.UtilityTools, Publisher:="I@xieguigang.me")>
Module gtk

    ''' <summary>
    ''' Show a messagebox dialog
    ''' </summary>
    ''' <param name="message$"></param>
    ''' <param name="title$"></param>
    ''' <param name="buttons"></param>
    ''' <param name="icon"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("dialog")>
    Public Function Dialog(message$,
                           Optional title$ = Nothing,
                           Optional buttons As MessageBoxButtons = MessageBoxButtons.OK,
                           Optional icon As MessageBoxIcon = MessageBoxIcon.Information,
                           Optional env As Environment = Nothing) As DialogResult

        Return MessageBox.Show(message, title Or env.parent.stackFrame.File.AsDefault, buttons, icon)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="default"></param>
    ''' <param name="desc"></param>
    ''' <param name="newFolder"></param>
    ''' <param name="root"></param>
    ''' <param name="throwCancel"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("selectFolder")>
    <RApiReturn(GetType(String))>
    Public Function selectFolder(Optional default$ = "",
                                 Optional desc As String = Nothing,
                                 Optional newFolder As Boolean = False,
                                 Optional root As SpecialFolder = SpecialFolder.MyComputer,
                                 Optional throwCancel As Boolean = True,
                                 Optional env As Environment = Nothing) As Object

        Using folder As New FolderBrowserDialog With {
            .ShowNewFolderButton = newFolder,
            .RootFolder = root,
            .Description = desc,
            .SelectedPath = [default]
        }
            If folder.ShowDialog = DialogResult.OK Then
                Return folder.SelectedPath
            ElseIf throwCancel Then
                Return Internal.debug.stop("user cancel!", env)
            Else
                Return Nothing
            End If
        End Using
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="title"></param>
    ''' <param name="filter"></param>
    ''' <param name="forSave"></param>
    ''' <param name="throwCancel"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("selectFiles")>
    <RApiReturn(GetType(String))>
    Public Function selectFiles(Optional title$ = Nothing,
                                <RRawVectorArgument>
                                Optional filter As Object = "any file(*.*)|*.*",
                                Optional forSave As Boolean = False,
                                Optional multiple As Boolean = True,
                                Optional throwCancel As Boolean = True,
                                Optional env As Environment = Nothing) As Object

        Dim filters As String() = CLRVector.asCharacter(filter)

        If forSave Then
            Using file As New SaveFileDialog With {
                .Title = title,
                .Filter = filters.JoinBy("|")
            }
                If file.ShowDialog = DialogResult.OK Then
                    Return file.FileName
                ElseIf throwCancel Then
                    Return Internal.debug.stop("user cancel!", env)
                Else
                    Return Nothing
                End If
            End Using
        Else
            Using file As New OpenFileDialog With {
                .Title = title,
                .Filter = filters.JoinBy("|"),
                .Multiselect = multiple
            }
                If file.ShowDialog = DialogResult.OK Then
                    Return file.FileNames
                ElseIf throwCancel Then
                    Return Internal.debug.stop("user cancel!", env)
                Else
                    Return Nothing
                End If
            End Using
        End If
    End Function
End Module
