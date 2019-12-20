#Region "Microsoft.VisualBasic::ed82fb99cc0546debbea71e2beedda35, R#\Runtime\Internal\internalInvokes\file.vb"

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
'         Function: basename, dirname, exists, getwd, listDirs
'                   listFiles, normalizeFileName, normalizePath, readLines, Rhome
'                   setwd, writeLines
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
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' ## File Manipulation
    ''' 
    ''' These functions provide a low-level interface to the computer's file system.
    ''' </summary>
    Module file

        ''' <summary>
        ''' Express File Paths in Canonical Form
        ''' 
        ''' Convert file paths to canonical form for the platform, to display them in a 
        ''' user-understandable form and so that relative and absolute paths can be 
        ''' compared.
        ''' </summary>
        ''' <param name="fileNames">character vector of file paths.</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
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

        ''' <summary>
        ''' ``dirname`` returns the part of the ``path`` up to but excluding the last path separator, 
        ''' or "." if there is no path separator.
        ''' </summary>
        ''' <param name="fileNames">character vector, containing path names.</param>
        ''' <returns></returns>
        <ExportAPI("dirname")>
        Public Function dirname(fileNames As String()) As Object
            Return fileNames.Select(AddressOf ParentPath).ToArray
        End Function

        ''' <summary>
        ''' List the Files in a Directory/Folder
        ''' </summary>
        ''' <param name="dir">
        ''' a character vector of full path names; the default corresponds to the working directory, ``getwd()``. 
        ''' Tilde expansion (see path.expand) is performed. Missing values will be ignored.
        ''' </param>
        ''' <param name="pattern">
        ''' an optional regular expression. Only file names which match the regular expression will be returned.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("list.files")>
        Public Function listFiles(Optional dir$ = "./",
                                  Optional pattern$() = Nothing,
                                  Optional recursive As Boolean = False) As Object

            If pattern.IsNullOrEmpty Then
                pattern = {"*.*"}
            End If

            If recursive Then
                Return (ls - l - r - pattern <= dir).ToArray
            Else
                Return (ls - l - pattern <= dir).ToArray
            End If
        End Function

        ''' <summary>
        ''' List the Files in a Directory/Folder
        ''' </summary>
        ''' <param name="dir">
        ''' a character vector of full path names; the default corresponds to the working directory, ``getwd()``. 
        ''' Tilde expansion (see path.expand) is performed. Missing values will be ignored.
        ''' </param>
        ''' <param name="fullNames"></param>
        ''' <param name="recursive"></param>
        ''' <returns></returns>
        <ExportAPI("list.dirs")>
        Public Function listDirs(Optional dir$ = "./",
                                 Optional fullNames As Boolean = True,
                                 Optional recursive As Boolean = True) As Object

            If Not dir.DirectoryExists Then
                Return {}
            Else
                Dim level As SearchOption = If(recursive, SearchOption.SearchAllSubDirectories, SearchOption.SearchTopLevelOnly)
                Dim dirs$() = dir _
                    .ListDirectory(level, fullNames) _
                    .ToArray

                Return dirs
            End If
        End Function

        ''' <summary>
        ''' removes all of the path up to and including the last path separator (if any).
        ''' </summary>
        ''' <param name="fileNames">character vector, containing path names.</param>
        ''' <param name="withExtensionName"></param>
        ''' <returns></returns>
        <ExportAPI("basename")>
        Public Function basename(fileNames$(), Optional withExtensionName As Boolean = False) As Object
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

        ''' <summary>
        ''' ``file.exists`` returns a logical vector indicating whether the files named by its 
        ''' argument exist. (Here ‘exists’ is in the sense of the system's stat call: a file 
        ''' will be reported as existing only if you have the permissions needed by stat. 
        ''' Existence can also be checked by file.access, which might use different permissions 
        ''' and so obtain a different result. Note that the existence of a file does not 
        ''' imply that it is readable: for that use file.access.) What constitutes a ‘file’ 
        ''' is system-dependent, but should include directories. (However, directory names 
        ''' must not include a trailing backslash or slash on Windows.) Note that if the file 
        ''' is a symbolic link on a Unix-alike, the result indicates if the link points to 
        ''' an actual file, not just if the link exists. Lastly, note the different function 
        ''' exists which checks for existence of R objects.
        ''' </summary>
        ''' <param name="files">
        ''' character vectors, containing file names or paths.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("file.exists")>
        Public Function exists(files$()) As Boolean()
            Return files.Select(AddressOf FileExists).ToArray
        End Function

        ''' <summary>
        ''' Read Text Lines from a Connection
        ''' 
        ''' Read some or all text lines from a connection.
        ''' </summary>
        ''' <param name="con">a connection object or a character string.</param>
        ''' <returns></returns>
        <ExportAPI("readLines")>
        Public Function readLines(con As String) As String()
            Return con.ReadAllLines
        End Function

        ' writeLines(text, con = stdout(), sep = "\n", useBytes = FALSE)

        ''' <summary>
        ''' Write Lines to a Connection
        ''' 
        ''' Write text lines to a connection.
        ''' </summary>
        ''' <param name="text">A character vector</param>
        ''' <param name="con">A connection Object Or a character String.</param>
        ''' <param name="sep">character string. A string to be written to the connection after each line of text.</param>
        ''' <returns></returns>
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

        ''' <summary>
        ''' getwd returns an absolute filepath representing the current working directory of the R process;
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("getwd")>
        Public Function getwd() As String
            Return App.CurrentDirectory
        End Function

        ''' <summary>
        ''' setwd(dir) is used to set the working directory to dir.
        ''' </summary>
        ''' <param name="dir">A character String: tilde expansion will be done.</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
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

        ''' <summary>
        ''' Save a R# object list in json file format
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="file$"></param>
        ''' <returns></returns>
        <ExportAPI("save.list")>
        Public Function saveList(list As Object, file$, Optional envir As Environment = Nothing) As Object
            If list Is Nothing Then
                Return False
            End If

            Dim json$

            If list.GetType Is GetType(list) Then
                json = DirectCast(list, list).slots.GetJson
            ElseIf list.GetType Is GetType(Dictionary(Of String, String)) Then
                json = JsonContract.GetObjectJson(list.GetType, list, False)
            Else
                Return Internal.stop(New NotSupportedException(list.GetType.FullName), envir)
            End If

            Return json.SaveTo(file)
        End Function

        ''' <summary>
        ''' read list from a given json file
        ''' </summary>
        ''' <param name="file">A json file path</param>
        ''' <param name="mode">The value mode of the loaded list object in ``R#``</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("read.list")>
        Public Function readList(file$, Optional mode$ = "character", Optional envir As Environment = Nothing) As Object
            Select Case LCase(mode)
                Case "character"
                    Return file.LoadJsonFile(Of Dictionary(Of String, String))
                Case "numeric"
                    Return file.LoadJsonFile(Of Dictionary(Of String, Double))
                Case "integer"
                    Return file.LoadJsonFile(Of Dictionary(Of String, Long))
                Case "logical"
                    Return file.LoadJsonFile(Of Dictionary(Of String, Boolean))
                Case "any"
                    Return file.LoadJsonFile(Of Dictionary(Of String, Object))(knownTypes:={GetType(String), GetType(Boolean), GetType(Double), GetType(Long), GetType(Integer)})
                Case Else
                    Return Internal.stop($"Invalid data mode: '{mode}'!", envir)
            End Select
        End Function
    End Module
End Namespace
