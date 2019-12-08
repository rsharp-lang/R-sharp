#Region "Microsoft.VisualBasic::27e23e05fec2b281699a16087176a463, R#\Runtime\Internal\internalInvokes\file.vb"

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

'     Module file
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: exists, normalizeFileName, readLines, saveImage, setwd
'                   writeLines
' 
'         Sub: pushEnvir
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' ## File Manipulation
    ''' 
    ''' These functions provide a low-level interface to the computer's file system.
    ''' </summary>
    Module file

        <ExportAPI("normalizePath")>
        Public Function normalizePath(fileNames$(), envir As Environment) As Object
            If fileNames.IsNullOrEmpty Then
                Return Internal.stop("no file names provided!", envir)
            Else
                Return fileNames _
                    .Select(Function(path)
                                If path.DirectoryExists Then
                                    Return path.GetDirectoryFullPath
                                Else
                                    Return path.GetFullPath
                                End If
                            End Function) _
                    .ToArray
            End If
        End Function

        <ExportAPI("R.home")>
        Public Function Rhome() As Object
            Return GetType(file).Assembly.Location.ParentPath
        End Function

        <ExportAPI("dirname")>
        Public Function dirname(fileNames As String(), envir As Environment) As Object
            Return fileNames.Select(AddressOf ParentPath).ToArray
        End Function

        <ExportAPI("list.files")>
        Public Function listFiles(dir$, Optional pattern$() = Nothing, Optional envir As Environment = Nothing) As Object
            If pattern.IsNullOrEmpty Then
                pattern = {"*.*"}
            End If

            Return (ls - l - r - pattern <= dir).ToArray
        End Function

        <ExportAPI("list.dirs")>
        Public Function listDirs(Optional dir$ = "./", Optional envir As Environment = Nothing) As Object
            Dim dirs$() = dir _
                .ListDirectory(SearchOption.SearchAllSubDirectories) _
                .ToArray

            Return dirs
        End Function

        <ExportAPI("basename")>
        Public Function basename(fileNames$(), Optional withExtensionName As Boolean = False, Optional envir As Environment = Nothing) As Object
            If withExtensionName Then
                ' get fileName
                Return fileNames.Select(AddressOf FileName).ToArray
            Else
                Return fileNames _
                    .Select(Function(file)
                                If file.DirectoryExists Then
                                    Return file.DirectoryName
                                Else
                                    Return file.BaseName
                                End If
                            End Function) _
                    .ToArray
            End If
        End Function

        <ExportAPI("normalize.filename")>
        Public Function normalizeFileName(strings$()) As String()
            Return strings _
                .Select(Function(file)
                            Return file.NormalizePathString(False)
                        End Function) _
                .ToArray
        End Function

        <ExportAPI("file.exists")>
        Public Function exists(files$()) As Boolean()
            Return files.Select(AddressOf FileExists).ToArray
        End Function

        <ExportAPI("readLines")>
        Public Function readLines(file As String) As String()
            Return file.ReadAllLines
        End Function

        ' writeLines(text, con = stdout(), sep = "\n", useBytes = FALSE)
        <ExportAPI("writeLines")>
        Public Function writeLines(text$(), Optional con$ = Nothing, Optional sep$ = vbCrLf) As Object
            If con.StringEmpty Then
                Call text.AsObjectEnumerator _
                    .JoinBy(sep) _
                    .DoCall(AddressOf Console.WriteLine)
            Else
                Call text.AsObjectEnumerator _
                    .JoinBy(sep) _
                    .SaveTo(con)
            End If

            Return text
        End Function

        <ExportAPI("getwd")>
        Public Function getwd() As String
            Return App.CurrentDirectory
        End Function

        <ExportAPI("setwd")>
        Public Function setwd(dir$(), envir As Environment) As Object
            If dir.Length = 0 Then
                Return invoke.missingParameter(NameOf(setwd), "dir", envir)
            ElseIf dir(Scan0).StringEmpty Then
                Return invoke.invalidParameter("cannot change working directory due to the reason of NULL value provided!", NameOf(setwd), "dir", envir)
            Else
                App.CurrentDirectory = PathMapper.GetMapPath(dir(Scan0))
            End If

            Return App.CurrentDirectory
        End Function
    End Module
End Namespace
