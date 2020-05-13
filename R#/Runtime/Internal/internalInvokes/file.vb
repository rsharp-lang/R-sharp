#Region "Microsoft.VisualBasic::94bd77907e2977e619013c5ad238152b, R#\Runtime\Internal\internalInvokes\file.vb"

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
    '         Function: basename, dir_exists, dirCreate, dirname, exists
    '                   file, filecopy, fileinfo, getwd, listDirs
    '                   listFiles, loadListInternal, normalizeFileName, normalizePath, openGzip
    '                   openZip, readLines, readList, Rhome, saveList
    '                   setwd, writeLines
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Components
Imports BASICString = Microsoft.VisualBasic.Strings
Imports fsOptions = Microsoft.VisualBasic.FileIO.SearchOption

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' #### File Manipulation
    ''' 
    ''' These functions provide a low-level interface to the computer's file system.
    ''' </summary>
    Public Module file

        ''' <summary>
        ''' Extract File Information
        ''' 
        ''' Utility function to extract information about files on the user's file systems.
        ''' </summary>
        ''' <param name="files"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("file.info")>
        Public Function fileinfo(files As String(), Optional env As Environment = Nothing) As Object
            If files.IsNullOrEmpty Then
                Return Nothing
            ElseIf files.Length = 1 Then
                Dim file As String = files(Scan0)
                Dim fileInfoObj As New FileInfo(file)
                Dim data As New Dictionary(Of String, Object)

                For Each [property] As PropertyInfo In fileInfoObj _
                    .GetType _
                    .GetProperties(PublicProperty) _
                    .Where(Function(p)
                               Return p.GetIndexParameters.IsNullOrEmpty
                           End Function)

                    Call data.Add([property].Name, [property].GetValue(fileInfoObj))
                Next

                Return New list With {.slots = data}
            Else
                Return Internal.debug.stop(New NotImplementedException, env)
            End If
        End Function

        ''' <summary>
        ''' ``file.copy`` works in a similar way to ``file.append`` but with the arguments 
        ''' in the natural order for copying. Copying to existing destination files is 
        ''' skipped unless overwrite = TRUE. The to argument can specify a single existing 
        ''' directory. If copy.mode = TRUE file read/write/execute permissions are copied 
        ''' where possible, restricted by ‘umask’. (On Windows this applies only to files.
        ''' ) Other security attributes such as ACLs are not copied. On a POSIX filesystem 
        ''' the targets of symbolic links will be copied rather than the links themselves, 
        ''' and hard links are copied separately. Using copy.date = TRUE may or may not 
        ''' copy the timestamp exactly (for example, fractional seconds may be omitted), 
        ''' but is more likely to do so as from R 3.4.0.
        ''' </summary>
        ''' <param name="from"></param>
        ''' <param name="to"></param>
        ''' <returns>
        ''' These functions return a logical vector indicating which operation succeeded 
        ''' for each of the files attempted. Using a missing value for a file or path 
        ''' name will always be regarded as a failure.
        ''' </returns>
        ''' 
        <ExportAPI("file.copy")>
        <RApiReturn(GetType(Boolean()))>
        Public Function filecopy(from$(), to$(), Optional env As Environment = Nothing) As Object
            Dim result As New List(Of Object)
            Dim isDir As Boolean = from.Length > 1 AndAlso [to].Length = 1

            If from.Length = 0 Then
                Return {}
            End If

            If isDir Then
                Dim dirName$ = [to](Scan0) & "/"

                For Each file As String In from
                    If file.FileCopy(dirName) Then
                        result.Add(True)
                    Else
                        result.Add(file)
                    End If
                Next
            ElseIf from.Length <> [to].Length Then
                Return Internal.debug.stop("number of from files is not equals to the number of target file locations!", env)
            Else
                For i As Integer = 0 To from.Length - 1
                    If from(i).FileCopy([to](i)) Then
                        result.Add(True)
                    Else
                        result.Add(from(i))
                    End If
                Next
            End If

            Return result.ToArray
        End Function

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
        <RApiReturn(GetType(String()))>
        Public Function normalizePath(fileNames$(), envir As Environment) As Object
            If fileNames.IsNullOrEmpty Then
                Return Internal.debug.stop("no file names provided!", envir)
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

        ''' <summary>
        ''' Return the R Home Directory
        ''' 
        ''' Return the R home directory, or the full path to a 
        ''' component of the R installation.
        ''' 
        ''' The R home directory is the top-level directory of the R installation being run.
        '''
        ''' The R home directory Is often referred To As R_HOME, And Is the value Of an 
        ''' environment variable Of that name In an R session. It can be found outside 
        ''' an R session by R RHOME.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("R.home")>
        Public Function Rhome() As String
            Return GetType(file).Assembly.Location.ParentPath
        End Function

        ''' <summary>
        ''' ``dirname`` returns the part of the ``path`` up to but excluding the last path separator, 
        ''' or "." if there is no path separator.
        ''' </summary>
        ''' <param name="fileNames">character vector, containing path names.</param>
        ''' <returns></returns>
        <ExportAPI("dirname")>
        Public Function dirname(fileNames As String()) As String()
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
        <RApiReturn(GetType(String()))>
        Public Function listFiles(Optional dir$ = "./",
                                  Optional pattern$() = Nothing,
                                  Optional recursive As Boolean = False) As Object

            If pattern.IsNullOrEmpty Then
                pattern = {"*.*"}
            End If

            If dir.ExtensionSuffix("zip") AndAlso dir.FileLength > 0 Then
                Using zip As New ZipFolder(dir)
                    Return Search.DoFileNameGreps(ls - l - r - pattern, zip.ls).ToArray
                End Using
            Else
                If recursive Then
                    Return (ls - l - r - pattern <= dir).ToArray
                Else
                    Return (ls - l - pattern <= dir).ToArray
                End If
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
        <RApiReturn(GetType(String()))>
        Public Function listDirs(Optional dir$ = "./",
                                 Optional fullNames As Boolean = True,
                                 Optional recursive As Boolean = True) As Object

            If Not dir.DirectoryExists Then
                Return {}
            Else
                Dim level As fsOptions = If(recursive, fsOptions.SearchAllSubDirectories, fsOptions.SearchTopLevelOnly)
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
        Public Function basename(fileNames$(), Optional withExtensionName As Boolean = False) As String()
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

        <ExportAPI("dir.create")>
        Public Function dirCreate(path$, Optional showWarnings As Boolean = True, Optional recursive As Boolean = False, Optional mode$ = "0777") As Boolean
            Call path.MkDIR
            Return True
        End Function

        ''' <summary>
        ''' dir.exists returns a logical vector of TRUE or FALSE values (without names).
        ''' </summary>
        ''' <param name="paths">
        ''' character vectors containing file or directory paths. 
        ''' Tilde expansion (see path.expand) is done.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("dir.exists")>
        Public Function dir_exists(paths As String()) As Boolean()
            Return paths.Select(AddressOf DirectoryExists).ToArray
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
        Public Function writeLines(text$(), Optional con$ = Nothing, Optional sep$ = vbCrLf, Optional env As Environment = Nothing) As Object
            If con.StringEmpty Then
                Dim stdOut As Action(Of String)

                If env.globalEnvironment.stdout Is Nothing Then
                    stdOut = AddressOf Console.WriteLine
                Else
                    stdOut = AddressOf env.globalEnvironment.stdout.WriteLine
                End If

                Call text.AsObjectEnumerator _
                    .JoinBy(sep) _
                    .DoCall(stdOut)
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
        <RApiReturn(GetType(String))>
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
            Dim listType As Type = list.GetType

            If listType Is GetType(list) Then
                json = DirectCast(list, list).slots.GetJson(knownTypes:=listKnownTypes)
            ElseIf listType.ImplementInterface(GetType(IDictionary)) AndAlso
                listType.GenericTypeArguments.Length > 0 AndAlso
                listType.GenericTypeArguments(Scan0) Is GetType(String) Then

                json = JsonContract.GetObjectJson(listType, list, True, True, listKnownTypes)
            Else
                Return Internal.debug.stop(New NotSupportedException(listType.FullName), envir)
            End If

            Return json.SaveTo(file)
        End Function

        ReadOnly listKnownTypes As Type() = {
            GetType(String), GetType(Boolean), GetType(Double), GetType(Long), GetType(Integer),
            GetType(String()), GetType(Boolean()), GetType(Double()), GetType(Long()), GetType(Integer())
        }

        ''' <summary>
        ''' read list from a given json file
        ''' </summary>
        ''' <param name="file">A json file path</param>
        ''' <param name="mode">The value mode of the loaded list object in ``R#``</param>
        ''' <param name="ofVector">
        ''' Is a list of vector?
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("read.list")>
        Public Function readList(file$,
                                 Optional mode$ = "character",
                                 Optional ofVector As Boolean = False,
                                 Optional envir As Environment = Nothing) As Object

            Select Case BASICString.LCase(mode)
                Case "character" : Return loadListInternal(Of String)(file, ofVector)
                Case "numeric" : Return loadListInternal(Of Double)(file, ofVector)
                Case "integer" : Return loadListInternal(Of Long)(file, ofVector)
                Case "logical" : Return loadListInternal(Of Boolean)(file, ofVector)
                Case "any"
                    Return file.LoadJsonFile(Of Dictionary(Of String, Object))(knownTypes:=listKnownTypes)
                Case Else
                    Return Internal.debug.stop($"Invalid data mode: '{mode}'!", envir)
            End Select
        End Function

        Private Function loadListInternal(Of T)(file As String, ofVector As Boolean) As Object
            If ofVector Then
                Return file.LoadJsonFile(Of Dictionary(Of String, T()))
            Else
                Return file.LoadJsonFile(Of Dictionary(Of String, T))
            End If
        End Function

        ''' <summary>
        ''' Functions to create, open and close connections, i.e., 
        ''' "generalized files", such as possibly compressed files, 
        ''' URLs, pipes, etc.
        ''' </summary>
        ''' <param name="description">character string. A description of the connection: see ‘Details’.</param>
        ''' <param name="open">
        ''' character string. A description of how to open the connection (if it should be opened initially). 
        ''' See section ‘Modes’ for possible values.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("file")>
        Public Function file(description$,
                             Optional open As FileMode = FileMode.OpenOrCreate,
                             Optional truncate As Boolean = False) As FileStream

            If open = FileMode.Truncate OrElse open = FileMode.CreateNew Then
                Return description.Open(open, doClear:=truncate)
            Else
                Return description.Open(open, doClear:=False)
            End If
        End Function

        <ExportAPI("open.zip")>
        <RApiReturn(GetType(ZipFolder))>
        Public Function openZip(file As String, Optional env As Environment = Nothing) As Object
            If Not file.FileExists Then
                Return debug.stop({"target file is not exists on your file system!", "file: " & file}, env)
            Else
                Return New ZipFolder(file)
            End If
        End Function

        <ExportAPI("open.gzip")>
        Public Function openGzip(file As String) As Stream
            Using originalFileStream As FileStream = file.Open(FileMode.Open, doClear:=False)
                Dim deflate As New MemoryStream

                Using decompressionStream As New GZipStream(originalFileStream, CompressionMode.Decompress)
                    decompressionStream.CopyTo(deflate)
                End Using

                Return deflate
            End Using
        End Function
    End Module
End Namespace
