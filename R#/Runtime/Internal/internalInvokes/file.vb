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
        Private Function normalizePath(envir As Environment, params As Object()) As Object
            If params.IsNullOrEmpty Then
                Return Internal.stop("no file names provided!", envir)
            Else
                Return Runtime.asVector(Of String)(params(Scan0)) _
                    .AsObjectEnumerator(Of String) _
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
        Private Function Rhome(envir As Environment, params As Object()) As Object
            Return GetType(file).Assembly.Location.ParentPath
        End Function

        <ExportAPI("dirname")>
        Private Function dirname(envir As Environment, params As Object()) As Object
            If params.IsNullOrEmpty Then
                Return Internal.stop("no file names provided!", envir)
            End If

            Dim fileNames As String() = params _
                .Select(Function(str)
                            Return Runtime.asVector(Of String)(str).AsObjectEnumerator(Of String)
                        End Function) _
                .IteratesALL _
                .ToArray

            Return fileNames.Select(AddressOf ParentPath).ToArray
        End Function

        <ExportAPI("list.files")>
        Private Function listFiles(envir As Environment, params As Object()) As Object
            Dim dir = Scripting.ToString(Runtime.getFirst(params(Scan0)), Nothing)
            Dim pattern = Runtime.asVector(Of String)(params(1)).ToArray(Of String)

            If pattern.Length = 0 Then
                pattern = {"*.*"}
            End If

            Return (ls - l - r - pattern <= dir).ToArray
        End Function

        <ExportAPI("list.dirs")>
        Private Function listDirs(envir As Environment, params As Object()) As Object
            Dim dir$ = Runtime.asVector(Of String)(params(Scan0)) _
                .AsObjectEnumerator _
                .DefaultFirst("./")
            Dim dirs$() = dir _
                .ListDirectory(SearchOption.SearchAllSubDirectories) _
                .ToArray

            Return dirs
        End Function

        <ExportAPI("basename")>
        Private Function basename(envir As Environment, params As Object()) As Object
            If params.IsNullOrEmpty Then
                Return Internal.stop("no file names provided!", envir)
            End If

            Dim fileNames As String() = Runtime.asVector(Of String)(params(Scan0)) _
                .AsObjectEnumerator _
                .ToArray(Of String)
            Dim withExtensionName As Boolean = Runtime.asLogical(params.ElementAtOrDefault(1))(Scan0)

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
        Friend Function normalizeFileName(envir As Environment, params As Object()) As String()
            Return params.SafeQuery _
                .Select(Function(val)
                            Return Runtime.asVector(Of Double)(val) _
                                .AsObjectEnumerator _
                                .Select(Function(file)
                                            Return Scripting.ToString(file).NormalizePathString(False)
                                        End Function)
                        End Function) _
                .IteratesALL _
                .ToArray
        End Function

        <ExportAPI("file.exists")>
        Friend Function exists(envir As Environment, params As Object()) As Boolean()
            Return params.SafeQuery _
                .Select(Function(val)
                            If val Is Nothing Then
                                Return False
                            Else
                                Return Scripting _
                                    .ToString(Runtime.getFirst(val)) _
                                    .DoCall(AddressOf FileExists)
                            End If
                        End Function) _
                .ToArray
        End Function

        <ExportAPI("readLines")>
        Friend Function readLines(envir As Environment, params As Object()) As String()
            Return Scripting.ToString(Runtime.getFirst(params(Scan0))).ReadAllLines
        End Function

        ' writeLines(text, con = stdout(), sep = "\n", useBytes = FALSE)
        <ExportAPI("writeLines")>
        Friend Function writeLines(envir As Environment, params As Object()) As Object
            Dim text = Runtime.asVector(Of String)(params(Scan0))
            Dim con$ = Scripting.ToString(Runtime.getFirst(params(1)))

            If con.StringEmpty Then
                Call text.AsObjectEnumerator _
                    .JoinBy(vbCrLf) _
                    .DoCall(AddressOf Console.WriteLine)
            Else
                Call text.AsObjectEnumerator _
                    .JoinBy(vbCrLf) _
                    .SaveTo(con)
            End If

            Return text
        End Function

        <ExportAPI("setwd")>
        Friend Function setwd(envir As Environment, paramVals As Object()) As Object
            Dim dir As String() = Runtime.asVector(Of String)(paramVals(Scan0))

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
