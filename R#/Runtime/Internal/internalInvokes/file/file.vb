#Region "Microsoft.VisualBasic::aaf5dc6746eb92965ad03473aff1fe3f, R#\Runtime\Internal\internalInvokes\file.vb"

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

    '   Total Lines: 2068
    '    Code Lines: 1185 (57.30%)
    ' Comment Lines: 672 (32.50%)
    '    - Xml Docs: 86.31%
    ' 
    '   Blank Lines: 211 (10.20%)
    '     File Size: 95.04 KB


    '     Enum endianness
    ' 
    '         big, little
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module file
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [erase], basename, buffer, bytes, close
    '                   dataUri, dir_exists, dirCopy, dirCreate, dirname
    '                   exists, file, file_allocate, file_ext, filecopy
    '                   fileExt, fileinfo, fileInfoByFile, filepath, fileRemove
    '                   filesize, getRelativePath, GetSha1Hash, getwd, gzcheck
    '                   gzfile, handleWriteLargeTextStream, isSystemDir, listDirs, listFiles
    '                   loadListInternal, NextTempToken, normalizeFileName, normalizePath, openDir
    '                   openGzip, openTargzip, openZip, readBin, readBinDataframe
    '                   readBinOverloads, readFromFile, readFromStream, readLines, readList
    '                   readText, Rhome, saveList, scanZipFiles, setwd
    '                   stdin_dev, tempdir, tempfile, unlinks, writeBin
    '                   writeBinDataframe, writeLines
    ' 
    '         Sub: fileRename
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.UNIX
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports BASICString = Microsoft.VisualBasic.Strings
Imports Clang = Microsoft.VisualBasic.Language.C
Imports fsOptions = Microsoft.VisualBasic.FileIO.SearchOption
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Runtime.Internal.Invokes

    Public Enum endianness
        big
        little
    End Enum

    ''' <summary>
    ''' #### File Manipulation
    ''' 
    ''' These functions provide a low-level interface to the computer's file system.
    ''' </summary>
    ''' 
    <Package("file")>
    Public Module file

        Sub New()
            Call generic.add("writeBin", GetType(dataframe), AddressOf writeBinDataframe)
            Call generic.add("readBin.dataframe", GetType(Stream), AddressOf readBinDataframe)
        End Sub

        ''' <summary>
        ''' # Generate SHA1 checksum of a file
        ''' </summary>
        ''' <param name="filePath"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("fileSha1")>
        Public Function GetSha1Hash(filePath As String) As String
            Return SecurityString.GetSha1Hash(filePath)
        End Function

        ''' <summary>
        ''' Gets the relative pathname relative to a directory.
        ''' </summary>
        ''' <param name="pathname">A character String Of the pathname To be converted into an relative pathname.</param>
        ''' <param name="relativeTo">A character string of the reference pathname.</param>
        ''' <returns>Returns a character string of the relative pathname.</returns>
        ''' <remarks>
        ''' In case the two paths are on different file systems, 
        ''' for instance, C:/foo/bar/ and D:/foo/, the method 
        ''' returns pathname as is.
        ''' </remarks>
        <ExportAPI("relative_path")>
        Public Function getRelativePath(pathname As String(), <RDefaultExpression> Optional relativeTo As Object = "~getwd()") As String()
            Return pathname _
                .SafeQuery _
                .Select(Function(path)
                            Return RelativePath(relativeTo, path,
                                                appendParent:=False,
                                                fixZipPath:=True
                                ) _
                                .StringReplace("(\\+)|([/]{2,})", "/")
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' ## Extract File Information
        ''' 
        ''' Utility function to extract information about files on the user's file systems.
        ''' </summary>
        ''' <param name="x">
        ''' character vectors containing file paths. Tilde-expansion is done: see path.expand.
        ''' </param>
        ''' <returns>Double: File size In bytes. For missing file, this function will 
        ''' returns a negative number -1; and the file is exists on the filesystem, this 
        ''' function returns ZERO(empty file) or a positive number.</returns>
        ''' <remarks>
        ''' What constitutes a ‘file’ is OS-dependent but includes directories. (However, 
        ''' directory names must not include a trailing backslash or slash on Windows.) 
        ''' See also the section in the help for file.exists on case-insensitive file 
        ''' systems.
        ''' 
        ''' The file 'mode’ follows POSIX conventions, giving three octal digits summarizing 
        ''' the permissions for the file owner, the owner's group and for anyone respectively. 
        ''' Each digit is the logical or of read (4), write (2) and execute/search (1) 
        ''' permissions.
        ''' 
        ''' See files For how file paths With marked encodings are interpreted.
        ''' 
        ''' File modes are probably only useful On NTFS file systems, And it seems all three 
        ''' digits refer To the file's owner. The execute/search bits are set for directories, 
        ''' and for files based on their extensions (e.g., ‘.exe’, ‘.com’, ‘.cmd’ and ‘.bat’ 
        ''' files). file.access will give a more reliable view of read/write access 
        ''' availability to the R process.
        ''' 
        ''' UTF-8-encoded file names Not valid in the current locale can be used.
        ''' 
        ''' Junction points And symbolic links are followed, so information Is given about 
        ''' the file/directory To which the link points rather than about the link.
        ''' </remarks>
        <ExportAPI("file.size")>
        <RApiReturn(TypeCodes.integer)>
        Public Function filesize(x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return -1
            ElseIf TypeOf x Is String Then
                Return CStr(x).FileLength
            ElseIf TypeOf x Is FileReference Then
                Dim p As FileReference = x
                Dim size As Long = p.fs.FileSize(p.filepath)

                Return size
            Else
                Return Message.InCompatibleType(GetType(FileReference), x.GetType, env)
            End If
        End Function

        ''' <summary>
        ''' Construct Path to File
        ''' 
        ''' Construct the path to a file from components in a
        ''' platform-independent way.
        ''' </summary>
        ''' <param name="x">character vectors.  Long vectors are not supported.</param>
        ''' <param name="fsep">the path separator to use (assumed to be ASCII).
        ''' The components are by default separated by ‘/’ (not ‘\’) on
        ''' Windows.
        ''' </param>
        ''' <returns>
        ''' A character vector of the arguments concatenated term-by-term and
        ''' separated by 'fsep’ if all arguments have positive length;
        ''' otherwise, an empty character vector (unlike 'paste’).
        '''
        ''' An element Of the result will be marked (see 'Encoding’ as UTF-8
        ''' If run In a UTF-8 locale (When marked inputs are converted To
        ''' UTF-8) Or if an component of the result Is marked as UTF-8, Or as
        ''' Latin-1 in a non-Latin-1 locale.
        ''' </returns>
        ''' <remarks>
        ''' The implementation is designed to be fast (faster than ‘paste’) as
        ''' this Function is() used extensively In R itself.
        ''' It can also be used for environment paths such as 'PATH’ and
        ''' 'R_LIBS’ with ‘fsep = .Platform$path.sep’.
        ''' Trailing Path separators are invalid For Windows file paths apart
        ''' from '/’ and ‘d:/’ (although some functions/utilities do accept
        ''' them), so a trailing '/’ or ‘\’ is removed there.
        ''' </remarks>
        <ExportAPI("file.path")>
        <RApiReturn(GetType(String))>
        Public Function filepath(<RListObjectArgument>
                                 x As Object,
                                 Optional fsep As String = "/",
                                 Optional env As Environment = Nothing) As Object

            ' convert the invoke parameter to runtime values
            Dim array As Object = base.c(x, env)

            If TypeOf array Is Message Then
                Return array
            Else
                Dim path As String = DirectCast(array, Array) _
                    .AsObjectEnumerator _
                    .Select(Function(c)
                                If c Is Nothing Then
                                    Return ""
                                ElseIf c.GetType.ImplementInterface(Of IWorkspace) Then
                                    Return DirectCast(c, IWorkspace).Workspace
                                Else
                                    Return any.ToString(c)
                                End If
                            End Function) _
                    .JoinBy(fsep)

                If path.CheckUNCNetworkPath Then
                    Return path
                Else
                    Return path.StringReplace("[/]{2,}", "/")
                End If
            End If
        End Function

        ''' <summary>
        ''' Extract File Information
        ''' 
        ''' Utility function to extract information about files on the user's file systems.
        ''' </summary>
        ''' <param name="files">
        ''' The fully qualified name of the new file, or the relative file name. Do not end
        ''' the path with the directory separator character.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' a object list with slots:
        ''' 
        ''' + DirectoryName: Gets a string representing the directory's full path.
        ''' + Length: Gets the size, in bytes, of the current file.
        ''' + Name: Gets the name of the file.
        ''' + IsReadOnly: Gets or sets a value that determines if the current file is read only.
        ''' + Exists: Gets a value indicating whether a file exists.
        ''' 
        ''' </returns>
        <ExportAPI("file.info")>
        Public Function fileinfo(<RRawVectorArgument> files As Object,
                                 Optional clr_obj As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

            Dim fileList As String() = CLRVector.asCharacter(files)

            If fileList.IsNullOrEmpty Then
                Return Nothing
            ElseIf fileList.Length = 1 Then
                Return fileInfoByFile(fileList(Scan0), clr_obj)
            Else
                Return fileList _
                    .Select(Function(path) path.GetFullPath) _
                    .Distinct _
                    .ToDictionary(Function(filepath) filepath,
                                  Function(filepath)
                                      Return fileInfoByFile(filepath, clr_obj)
                                  End Function) _
                    .DoCall(Function(slots)
                                Return New list With {
                                    .slots = slots
                                }
                            End Function)
            End If
        End Function

        Private Function fileInfoByFile(filepath As String, clr_obj As Boolean) As Object
            Dim data As New Dictionary(Of String, Object)

            If filepath.FileExists Then
                Dim fileInfo As New FileInfo(filepath)

                If clr_obj Then
                    Return fileInfo
                End If

                Call data.Add("name", fileInfo.Name)
                Call data.Add("ctime", fileInfo.CreationTime)
                Call data.Add("mtime", fileInfo.LastWriteTime)
                Call data.Add("atime", fileInfo.LastAccessTime)
                Call data.Add("isdir", False)
                Call data.Add("size", fileInfo.Length)
                Call data.Add("mime", If(FileMimeType(filepath)?.MIMEType, MIME.Unknown))
            ElseIf filepath.DirectoryExists Then
                Dim dirinfo As New DirectoryInfo(filepath)

                If clr_obj Then
                    Return dirinfo
                End If

                Call data.Add("name", dirinfo.Name)
                Call data.Add("ctime", dirinfo.CreationTime)
                Call data.Add("mtime", dirinfo.LastWriteTime)
                Call data.Add("atime", dirinfo.LastAccessTime)
                Call data.Add("isdir", True)
            ElseIf clr_obj Then
                Return Nothing
            Else
                Call data.Add("name", filepath.BaseName)
            End If

            Return New list With {.slots = data}
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
        ''' <param name="To"></param>
        ''' <returns>
        ''' These functions return a logical vector indicating which operation succeeded 
        ''' for each of the files attempted. Using a missing value for a file or path 
        ''' name will always be regarded as a failure.
        ''' </returns>
        ''' 
        <ExportAPI("file.copy")>
        <RApiReturn(GetType(Boolean()))>
        Public Function filecopy(from$(), to$(),
                                 Optional overwrite As Boolean = False,
                                 Optional verbose As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

            Dim result As New List(Of Object)
            Dim toDir As Boolean = [to].Length = 1 AndAlso [to](Scan0).EndsWith("/"c)
            Dim isDir As Boolean = (from.Length > 1 AndAlso [to].Length = 1) OrElse (from.Length = 1 AndAlso from(Scan0).DirectoryExists AndAlso toDir)
            Dim println = env.WriteLineHandler

            If from.Length = 0 Then
                Return {}
            End If

            If isDir Then
                Dim dirName$ = [to](Scan0) & "/"

                If from.Length = 1 Then
                    Call New FileIO.Directory(from(Scan0)).CopyTo(dirName).ToArray
                Else
                    For Each file As String In from
                        If file.FileCopy(dirName) Then
                            result.Add(True)
                        Else
                            result.Add(file)
                        End If
                    Next
                End If
            ElseIf from.Length <> [to].Length Then
                Return Internal.debug.stop("number Of from files Is Not equals To the number Of target file locations!", env)
            ElseIf toDir AndAlso from.Length = 1 AndAlso from(Scan0).Contains("*"c) Then
                ' dir/wildcard copy to dir?
                Dim toDirName As String = [to](Scan0).GetDirectoryFullPath & "/"
                Dim dirSrc = from(Scan0).ParentPath
                Dim filePattern = from(Scan0).FileName
                Dim lookFiles = dirSrc.EnumerateFiles(filePattern).ToArray

                For i As Integer = 0 To lookFiles.Length - 1
                    If verbose Then
                        Call println($"[copy] {lookFiles(i)} ({StringFormats.Lanudry(lookFiles(i).FileLength)}) => {toDirName}")
                    End If

                    If lookFiles(i).FileCopy(toDirName) Then
                        Call result.Add(True)
                    Else
                        Call result.Add(lookFiles(i))
                    End If
                Next
            Else
                For i As Integer = 0 To from.Length - 1
                    If from(i).FileCopy([to](i)) Then
                        Call result.Add(True)
                    Else
                        If verbose Then
                            Call $"[file copy error] {App.GetLastError.Message}".warning
                        End If
                        Call result.Add(from(i))
                    End If
                Next
            End If

            Return result.ToArray
        End Function

        ''' <summary>
        ''' Get file extension name
        ''' </summary>
        ''' <param name="path">the file path string</param>
        ''' <returns>
        ''' returns a file extension suffix name in lower case, if there is 
        ''' no extension name or path string is empty, then empty string 
        ''' value will be returned.
        ''' </returns>
        <ExportAPI("file.ext")>
        <RApiReturn(GetType(String))>
        Public Function fileExt(<RRawVectorArgument> path As Object, Optional env As Environment = Nothing) As Object
            Return env.EvaluateFramework(Of String, String)(path, AddressOf ExtensionSuffix)
        End Function

        ''' <summary>
        ''' copy file contents in one dir to another dir
        ''' </summary>
        ''' <param name="from"></param>
        ''' <param name="To"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("dir.copy")>
        <RApiReturn(GetType(String))>
        Public Function dirCopy(from$, to$, Optional env As Environment = Nothing) As Object
            If Not from.DirectoryExists Then
                Return Internal.debug.stop($"the content source directory '{from}' is not exists on your file system!", env)
            Else
                Return New FileIO.Directory(from).CopyTo([to]).ToArray
            End If
        End Function

        ''' <summary>
        ''' Open an interface to a specific local filesystem location
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <param name="fs">
        ''' the logical filesystem view
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>a directory model</returns>
        <ExportAPI("dir.open")>
        Public Function openDir(dir As String,
                                Optional fs As IFileSystemEnvironment = Nothing,
                                Optional env As Environment = Nothing) As IFileSystemEnvironment

            If fs Is Nothing Then
                If Not dir.DirectoryExists Then
                    Call env.AddMessage($"target directory: {dir.GetDirectoryFullPath} is not found on the file system...")
                    Call dir.MakeDir
                End If

                Return FileIO.Directory.FromLocalFileSystem(dir)
            Else
                Return New FileSystemView(fs, dir)
            End If
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
        <RApiReturn(GetType(String))>
        Public Function normalizePath(fileNames$(), envir As Environment) As Object
            If fileNames.IsNullOrEmpty Then
                Return Internal.debug.stop("no file names provided!", envir)
            Else
                Return fileNames _
                    .Select(Function(path)
                                If path.DirectoryExists OrElse path.EndsWith("/"c) OrElse path.EndsWith("\"c) Then
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
        <RApiReturn(GetType(String))>
        Public Function dirname(<RRawVectorArgument> fileNames As Object,
                                Optional fullpath As Boolean = True,
                                Optional env As Environment = Nothing) As Object

            Return env.EvaluateFramework(Of String, String)(fileNames, Function(path) path.ParentPath(full:=fullpath))
        End Function

        ''' <summary>
        ''' List the Files in a Directory/Folder
        ''' </summary>
        ''' <param name="dir">
        ''' a character vector of full path names; the default corresponds to the 
        ''' working directory, ``getwd()``. 
        ''' Tilde expansion (see path.expand) is performed. Missing values will be 
        ''' ignored.
        ''' 
        ''' or zip folder object if this parameter is a file stream r zip file path.
        ''' </param>
        ''' <param name="pattern">
        ''' an optional regular expression/wildcard expression. Only file names which 
        ''' match the regular expression will be returned.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("list.files")>
        <RApiReturn(GetType(String()))>
        Public Function listFiles(Optional dir As Object = "./",
                                  Optional pattern$() = Nothing,
                                  Optional recursive As Boolean = False,
                                  Optional wildcard As Boolean = True,
                                  Optional env As Environment = Nothing) As Object

            Dim listfile As String()

            If dir Is Nothing Then
                Return Internal.debug.stop({
                    $"target file system resource can not be nothing!",
                    $"parameter_null: {NameOf(dir)}"
                }, env)
            End If

            If pattern.IsNullOrEmpty Then
                pattern = {"*.*"}
            End If

            ' 20220825 regexp *.* will be filter by the pattern
            ' regular expression at last

            If TypeOf dir Is String AndAlso
                DirectCast(dir, String).ExtensionSuffix("zip") AndAlso
                DirectCast(dir, String).FileLength > 0 Then

                Using zip As New ZipFolder(DirectCast(dir, String))
                    If wildcard Then
                        listfile = zip.scanZipFiles(pattern)
                    Else
                        listfile = zip.scanZipFiles("*.*")
                    End If
                End Using
            ElseIf TypeOf dir Is Stream Then
                Dim zip As New ZipFolder(DirectCast(dir, Stream))

                If wildcard Then
                    listfile = zip.scanZipFiles(pattern)
                Else
                    listfile = zip.scanZipFiles("*.*")
                End If
            ElseIf TypeOf dir Is ZipFolder Then
                If wildcard Then
                    listfile = DirectCast(dir, ZipFolder).scanZipFiles(pattern)
                Else
                    listfile = DirectCast(dir, ZipFolder).scanZipFiles("*.*")
                End If
            ElseIf dir.GetType.ImplementInterface(GetType(IFileSystemEnvironment)) Then
                Dim dir_fs As IFileSystemEnvironment = DirectCast(dir, IFileSystemEnvironment)

                If wildcard Then
                    listfile = If(recursive, dir_fs.GetFiles("/", pattern), dir_fs.EnumerateFiles("/", pattern)).ToArray
                Else
                    listfile = If(recursive, dir_fs.GetFiles("/"), dir_fs.EnumerateFiles("/")).ToArray
                End If
            Else
                Dim dirStr As String = any.ToString(dir)
                Dim match As String() = If(wildcard, pattern, {"*.*"})

                If recursive Then
                    listfile = (ls - l - r - match <= dirStr).ToArray
                Else
                    listfile = (ls - l - match <= dirStr).ToArray
                End If
            End If

            If wildcard Then
                Return listfile
            Else
                Return listfile _
                    .Where(Function(path)
                               Dim name As String = path.FileName
                               Dim test As Boolean = pattern.Any(Function(t) name.IsPattern(t))

                               Return test
                           End Function) _
                    .ToArray
            End If
        End Function

        <Extension>
        Private Function scanZipFiles(zip As ZipFolder, ParamArray pattern As String()) As String()
            If pattern(Scan0) = "*.*" Then
                Return zip.ls
            Else
                Return UnixBash.Search _
                    .DoFileNameGreps(ls - l - r - pattern, zip.ls) _
                    .ToArray
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
                                 Optional recursive As Boolean = True,
                                 Optional fs As IFileSystemEnvironment = Nothing) As Object

            If fs Is Nothing Then
                If Not dir.DirectoryExists Then
                    Return {}
                Else
                    Dim level As fsOptions = If(recursive, fsOptions.SearchAllSubDirectories, fsOptions.SearchTopLevelOnly)
                    Dim dirs$() = dir _
                        .ListDirectory(level, fullNames) _
                        .ToArray

                    Return dirs
                End If
            End If

            If recursive Then
                Return fs.GetFiles(dir & "/").Select(Function(file) file.ParentPath).Distinct.ToArray
            Else
                Return fs.GetFiles(dir & "/").Select(Function(file) file.ParentPath).Distinct.ToArray
            End If
        End Function

        ''' <summary>
        ''' ## File Utilities
        ''' 
        ''' </summary>
        ''' <param name="filenames">character vector giving file paths.</param>
        ''' <returns>
        ''' ``file_ext`` returns the file (name) extensions (excluding the leading dot). 
        ''' (Only purely alphanumeric extensions are recognized.)
        ''' </returns>
        <ExportAPI("file_ext")>
        Public Function file_ext(filenames As String()) As String()
            Return filenames _
                .SafeQuery _
                .Select(AddressOf ExtensionSuffix) _
                .ToArray
        End Function

        ''' <summary>
        ''' removes all of the path up to and including the last path separator (if any).
        ''' </summary>
        ''' <param name="fileNames">character vector, containing path names.</param>
        ''' <param name="withExtensionName">
        ''' option for config keeps the extension suffix in the name or not, 
        ''' removes the file suffix name by default.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("basename")>
        <RApiReturn(GetType(String))>
        Public Function basename(fileNames$(),
                                 Optional withExtensionName As Boolean = False,
                                 <RDefaultExpression>
                                 Optional strict As Object = "~as.logical(getOption('strict'))",
                                 Optional env As Environment = Nothing) As Object

            Dim strictFlag As Boolean = CLRVector.asLogical(strict).FirstOrDefault([default]:=True)

            If fileNames Is Nothing Then
                Return Internal.debug.stop("the given file name can not be nothing!", env)
            End If

            If withExtensionName Then
                ' get fileName
                Return fileNames _
                    .Select(Function(path) path.FileName) _
                    .ToArray
            Else
                Return fileNames _
                    .Select(Function(file)
                                If file.DirectoryExists Then
                                    Return file.DirectoryName
                                Else
                                    Return file.BaseName(allowEmpty:=Not strictFlag)
                                End If
                            End Function) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' removes all of the invalid character for the windows file name
        ''' </summary>
        ''' <param name="strings"></param>
        ''' <param name="alphabetOnly"></param>
        ''' <param name="replacement">
        ''' all of the invalid character for the windows file name 
        ''' will be replaced as this placeholder character
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("normalizeFileName")>
        <RApiReturn(GetType(String))>
        Public Function normalizeFileName(<RRawVectorArgument>
                                          strings As Object,
                                          Optional alphabetOnly As Boolean = True,
                                          Optional replacement As String = "_",
                                          Optional shrink As Boolean = True,
                                          Optional maxchars As Integer = 32,
                                          Optional env As Environment = Nothing) As Object

            If shrink AndAlso (replacement = "[" OrElse replacement = "]") Then
                Return Internal.debug.stop({
                    $"regular expression pattern error: '[{replacement}\s]{{2,}}'!",
                    $"please change the replacement character: '{replacement}'"
                }, env)
            End If

            Return env.EvaluateFramework(Of String, String)(
                x:=strings,
                eval:=Function(file)
                          If file Is Nothing Then
                              Return ""
                          Else
                              file = file.NormalizePathString(alphabetOnly, replacement)

                              If shrink Then
                                  file = file.StringReplace(
                                     pattern:=$"[{replacement}\s]{{2,}}",
                                     replaceAs:=replacement
                                  )
                              End If
                              If file.Length > maxchars Then
                                  file = $"{file.Substring(0, maxchars - 3)}..."
                              End If

                              Return file
                          End If
                      End Function)
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
        ''' <param name="fs">
        ''' a virtual filesystem object
        ''' </param>
        ''' <returns>
        ''' this function returns FALSE if the given files value is NULL
        ''' </returns>
        <ExportAPI("file.exists")>
        <RApiReturn(GetType(Boolean))>
        Public Function exists(<RRawVectorArgument> files As Object,
                               Optional ZERO_Nonexists As Boolean = False,
                               Optional fs As IFileSystemEnvironment = Nothing,
                               Optional env As Environment = Nothing) As Object

            If files Is Nothing Then
                Return False
            ElseIf TypeOf files Is FileReference Then
                Dim p As FileReference = files
                Dim vfs As IFileSystemEnvironment = p.fs
                Dim check As Boolean = vfs.FileExists(p.filepath, ZERO_Nonexists)

                Return check
            ElseIf Not fs Is Nothing Then
                Return env.EvaluateFramework(Of String, Boolean)(files, Function(path) fs.FileExists(path, ZERO_Nonexists))
            End If

            Return env.EvaluateFramework(Of String, Boolean)(files, Function(path) path.FileExists(ZERO_Nonexists))
        End Function

        <ExportAPI("file.allocate")>
        Public Function file_allocate(filepath As String, fs As IFileSystemEnvironment) As FileReference
            Return New FileReference With {.fs = fs, .filepath = filepath}
        End Function

        ''' <summary>
        ''' dir.create creates the last element of the path, unless recursive = TRUE. 
        ''' Trailing path separators are discarded. On Windows drives are allowed in 
        ''' the path specification and unless the path is rooted, it will be interpreted 
        ''' relative to the current directory on that drive. mode is ignored on Windows.
        ''' 
        ''' One of the idiosyncrasies of Windows Is that directory creation may report 
        ''' success but create a directory with a different name, for example dir.create("G.S.") 
        ''' creates '"G.S"’. This is undocumented, and what are the precise circumstances 
        ''' is unknown (and might depend on the version of Windows). Also avoid directory 
        ''' names with a trailing space.
        ''' </summary>
        ''' <param name="path">a character vector containing a single path name.</param>
        ''' <param name="showWarnings">logical; should the warnings on failure be shown?</param>
        ''' <param name="recursive">logical. Should elements of the path other than the last be created? 
        ''' If true, Like the Unix command mkdir -p.</param>
        ''' <param name="mode">the mode To be used On Unix-alikes: it will be coerced by as.octmode. 
        ''' For Sys.chmod it Is recycled along paths.</param>
        ''' <returns>
        ''' dir.create and Sys.chmod return invisibly a logical vector indicating if 
        ''' the operation succeeded for each of the files attempted. Using a missing 
        ''' value for a path name will always be regarded as a failure. dir.create 
        ''' indicates failure if the directory already exists. If showWarnings = TRUE, 
        ''' dir.create will give a warning for an unexpected failure (e.g., not for a 
        ''' missing value nor for an already existing component for recursive = TRUE).
        ''' </returns>
        ''' <remarks>
        ''' There is no guarantee that these functions will handle Windows relative paths 
        ''' of the form ‘d:path’: try ‘d:./path’ instead. In particular, ‘d:’ is 
        ''' not recognized as a directory. Nor are \\?\ prefixes (and similar) supported.
        ''' 
        ''' UTF-8-encoded dirnames Not valid in the current locale can be used.
        ''' </remarks>
        <ExportAPI("dir.create")>
        Public Function dirCreate(path$,
                                  Optional showWarnings As Boolean = True,
                                  Optional recursive As Boolean = False,
                                  Optional mode$ = "0777") As Boolean

            If showWarnings AndAlso path.DirectoryExists Then
                Call $"in dir.create(""{path}"") : '{path}' already exists".warning
            End If

            Call path.MakeDir

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
        ''' ### Read Text Lines from a Connection
        ''' 
        ''' Read some or all text lines from a connection.
        ''' </summary>
        ''' <param name="con">a connection object or a character string.</param>
        ''' <param name="stream">
        ''' if this options is config as TRUE, means this function will returns 
        ''' a lazy load data pipeline. default value of this option is FALSE, which
        ''' means this function will returns a character vector which contains all 
        ''' data content lines directly.
        ''' </param>
        ''' <param name="strict">
        ''' this function will returns an empty string vector if not in strict mode
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("readLines")>
        <RApiReturn(GetType(String))>
        Public Function readLines(<RDefaultExpression> Optional con As Object = "~stdin()",
                                  Optional n As Integer = -1,
                                  Optional encoding As Encodings = Encodings.UTF8,
                                  Optional stream As Boolean = False,
                                  Optional strict As Boolean? = Nothing,
                                  Optional env As Environment = Nothing) As Object

            If TypeOf con Is Stream Then
                Return readFromStream(DirectCast(con, Stream), n, stream)
            ElseIf TypeOf con Is WebResponseResult Then
                ' read web result html text into multuple text lines
                Return DirectCast(con, WebResponseResult).html.LineTokens
            ElseIf TypeOf con Is FileReference Then
                Dim p As FileReference = con

                If env.globalEnvironment.strictOption(strict) Then
                    If Not p.fs.FileExists(p.filepath) Then
                        Call $"the specific file '{p.filepath}' for read text lines is missing from filesystem: {p.fs.ToString}".warning
                        Return New String() {}
                    End If
                End If

                Dim text As String = p.fs.ReadAllText(p.filepath)
                Return text.LineTokens
            Else
                Dim str = CLRVector.asCharacter(con)
                Dim filepath As String = str.ElementAtOrDefault(Scan0)

                If str.IsNullOrEmpty Then
                    Return Internal.debug.stop("no file is specified for read data!", env)
                End If

                Return readFromFile(filepath, encoding, strict, env)
            End If
        End Function

        Private Function readFromFile(filepath As String, encoding As Encodings, strict As Boolean?, env As Environment) As Object
            If Not filepath.FileExists Then
                If env.globalEnvironment.strictOption(opt:=strict) Then
                    Return Internal.debug.stop($"the given file '{filepath}' is missing!", env)
                Else
                    Call env.AddMessage($"the given file '{filepath}' is missing!")
                    Return Nothing
                End If
            End If

            Return filepath.ReadAllLines(encoding.CodePage)
        End Function

        Private Function readFromStream(con As Stream, n As Integer, stream As Boolean) As Object
            ' 20250922 there is a bug about the streamreader object:
            ' it is buffered reader: for example if the given stream length is 15, but contains 5 lines of text data,
            ' then this buffered stream reader will read 1024 bytes at once, the stream position will be move to 15.
            ' in this function call we just needs read (n=1) one line of text data, but the stream position is in 15 now,
            ' so next time we try to read the stream, which its position is in 15 bytes, which is the end of file,
            ' this function it will return nothing.
            ' we use the unbuffered stream reader at here for avoid such problem.
            Dim text As New UnbufferedStreamReader(con)
            Dim line As Value(Of String) = ""
            Dim assert As Func(Of Boolean)

            If n < 0 Then
                assert = Function() True
            Else
                assert = Function()
                             If n > 0 Then
                                 n -= 1
                                 Return True
                             Else
                                 Return False
                             End If
                         End Function
            End If

            If stream Then
                Return Iterator Function() As IEnumerable(Of String)
                           Do While assert()
                               If Not (line = text.ReadLine) Is Nothing Then
                                   Yield CType(line, String)
                               Else
                                   Exit Do
                               End If
                           Loop
                       End Function() _
                                      _
                    .DoCall(AddressOf pipeline.CreateFromPopulator)
            Else
                Dim lines As New List(Of String)

                Do While assert()
                    If Not (line = text.ReadLine) Is Nothing Then
                        Call lines.Add(line)
                    Else
                        Exit Do
                    End If
                Loop

                Return lines.ToArray
            End If
        End Function

        ''' <summary>
        ''' Reads all characters from the current position to the end of the given stream.
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <ExportAPI("readText")>
        <RApiReturn(GetType(String))>
        Public Function readText(con As Object,
                                 Optional encoding As Encodings = Encodings.UTF8,
                                 Optional env As Environment = Nothing) As Object

            Dim linesVal As Object = readLines(con, encoding, stream:=False, env:=env)

            If TypeOf linesVal Is Message Then
                Return linesVal
            End If

            Dim text As String = DirectCast(linesVal, String()).JoinBy(vbLf)

            Return text
        End Function

        ' writeLines(text, con = stdout(), sep = "\n", useBytes = FALSE)

        ''' <summary>
        ''' ### Write Lines to a Connection
        ''' 
        ''' Write text lines to a connection.
        ''' </summary>
        ''' <param name="text">A character vector, or a serials of compatible interface for get text contents.</param>
        ''' <param name="con">A connection Object Or a character String.</param>
        ''' <param name="sep">
        ''' character string. A string to be written to the connection after each line of text.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' If the con is a character string, the function calls file to obtain a file connection
        ''' which is opened for the duration of the function call.
        '''
        ''' If the connection Is open it Is written from its current position. If it Is Not open, 
        ''' it Is opened For the duration Of the Call In "wt" mode And Then closed again.
        '''
        ''' Normally writeLines Is used With a text-mode connection, And the Default separator Is 
        ''' converted To the normal separator For that platform (LF On Unix/Linux, CRLF On Windows). 
        ''' For more control, open a binary connection And specify the precise value you want 
        ''' written To the file In sep. For even more control, use writeChar On a binary connection.
        '''
        ''' useBytes Is for expert use. Normally (when false) character strings with marked 
        ''' encodings are converted to the current encoding before being passed to the connection 
        ''' (which might do further re-encoding). useBytes = TRUE suppresses the re-encoding of 
        ''' marked strings so they are passed byte-by-byte to the connection: this can be useful 
        ''' When strings have already been re-encoded by e.g. iconv. (It Is invoked automatically 
        ''' For strings With marked encoding "bytes".)
        ''' </remarks>
        <ExportAPI("writeLines")>
        Public Function writeLines(<RRawVectorArgument>
                                   text As Object,
                                   Optional con As Object = Nothing,
                                   Optional sep$ = vbLf,
                                   Optional fs As IFileSystemEnvironment = Nothing,
                                   Optional encoding As Encodings = Encodings.UTF8WithoutBOM,
                                   Optional env As Environment = Nothing) As Object

            If text Is Nothing Then
                text = ""
            ElseIf TypeOf text Is vector Then
                text = DirectCast(text, vector).data
            ElseIf TypeOf text Is WebResponseResult Then
                text = DirectCast(text, WebResponseResult).html
            End If

            If sep Is Nothing Then
                sep = ""
            Else
                sep = Clang.sprintf(sep)
            End If

            If TypeOf con Is FileReference Then
                Dim p As FileReference = con

                fs = p.fs
                con = p.filepath
            End If

            If text.GetType().ImplementInterface(Of ISaveHandle) Then
                Dim saveFile As ISaveHandle = text

                If fs Is Nothing Then
                    If TypeOf con Is String Then
                        Return saveFile.Save(CStr(con), encoding)
                    Else
                        Return saveFile.Save(DirectCast(con, Stream), encoding.CodePage)
                    End If
                Else
                    Dim s As Stream = fs.OpenFile(CStr(con), FileMode.OpenOrCreate, FileAccess.Write)
                    Dim flag As Boolean = saveFile.Save(s, encoding.CodePage)

                    Call s.Flush()
                    Call fs.Flush()

                    Return flag
                End If
            End If

            If TypeOf text Is pipeline Then
                Return DirectCast(text, pipeline) _
                    .populates(Of String)(env) _
                    .handleWriteLargeTextStream(con, sep, encoding.CodePage, fs, env)
            ElseIf TypeOf text Is String Then
                Return {DirectCast(text, String)}.handleWriteLargeTextStream(con, sep, encoding.CodePage, fs, env)
            Else
                ' 20240610 output memory error maybe happends when do string join
                '
                '  Error in <globalEnvironment> -> InitializeEnvironment -> "writeLines"("con" <- "./cfmid4.sh"...) -> writeLines
                '   1. OutOfMemoryException: Exception of type 'System.OutOfMemoryException' was thrown.
                '   2. stackFrames:
                '    at System.String.JoinCore(ReadOnlySpan`1 separator, ReadOnlySpan`1 values)
                '    at System.String.Join(String separator, String[] value)
                '    at Microsoft.VisualBasic.Extensions.JoinBy(IEnumerable`1 tokens, String delimiter) in E:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\Extensions\Extensions.vb:line 478
                '    at SMRUCC.Rsharp.Runtime.Internal.Invokes.file.writeLines(Object text, Object con, String sep, IFileSystemEnvironment fs, Encodings encoding, Environment env) in E:\GCModeller\src\R-sharp\R#\Runtime\Internal\internalInvokes\file.vb:line 1044

                '    Call "writeLines"(&cmdl, "con" <- "./cfmid4.sh")
                '    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                ' file.R#_clr_interop::.writeLines at [REnv, Version=2.33.856.6961, Culture=neutral, PublicKeyToken=null]:line &Hx0c4972403
                ' SMRUCC/R#.call_function."writeLines"("con" <- "./cfmid4.sh"...) at export_task.R:line 46
                ' SMRUCC/R#.n/a.InitializeEnvironment at export_task.R:line 0
                ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a

                Return CLRVector.asCharacter(text).handleWriteLargeTextStream(con, sep, encoding.CodePage, fs, env)
            End If

            Return Nothing
        End Function

        <Extension>
        Private Function handleWriteLargeTextStream(text As IEnumerable(Of String),
                                                    con As Object,
                                                    sep As String,
                                                    encoding As Encoding,
                                                    fs As IFileSystemEnvironment,
                                                    env As Environment) As Object

            If con Is Nothing OrElse (TypeOf con Is String AndAlso DirectCast(con, String).StringEmpty) Then
                Dim stdOut As Action(Of String)

                If env.globalEnvironment.stdout Is Nothing Then
                    stdOut = AddressOf Console.WriteLine
                Else
                    stdOut = AddressOf env.globalEnvironment.stdout.WriteLine
                End If

                For Each line As String In text
                    Call stdOut(line)
                Next
            ElseIf TypeOf con Is String Then
                If fs Is Nothing Then
                    ' save to local filesystem
                    Call text.SaveTo(CStr(con), encoding, newLine:=sep)
                Else
                    ' save to a virtual filesystem
                    Dim s As Stream = fs.OpenFile(CStr(con), access:=FileAccess.Write)
                    Dim wr As New StreamWriter(s, encoding) With {.NewLine = sep}

                    For Each line As String In text
                        Call wr.WriteLine(line)
                    Next

                    Call wr.Flush()
                    Call wr.Close()
                    Call wr.Dispose()
                End If
            ElseIf TypeOf con Is textBuffer Then
                ' save to a http stream
                DirectCast(con, textBuffer).text = text.JoinBy(sep)
                Return con
            ElseIf TypeOf con Is ITextWriter OrElse con.GetType.IsInheritsFrom(GetType(ITextWriter)) Then
                Dim dev As ITextWriter = DirectCast(con, ITextWriter)

                For Each line As String In text
                    Call dev.WriteLine(line)
                Next

                Return con
            ElseIf TypeOf con Is Stream Then
                Dim dev As New StreamWriter(DirectCast(con, Stream))

                For Each line As String In text
                    Call dev.WriteLine(line)
                Next

                Call dev.Flush()
            Else
                Return Internal.debug.stop(New NotSupportedException($"invalid buffer type: {con.GetType.FullName}!"), env)
            End If

            Return Nothing
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
        Public Function saveList(list As Object, file$,
                                 Optional encodings As Encodings = Encodings.UTF8,
                                 Optional envir As Environment = Nothing) As Object

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

            Return json.SaveTo(file, encodings.CodePage)
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
                                 Optional encoding As Encodings = Encodings.UTF8,
                                 Optional envir As Environment = Nothing) As Object

            Select Case BASICString.LCase(mode)
                Case "character" : Return loadListInternal(Of String)(file, ofVector, encoding.CodePage)
                Case "numeric" : Return loadListInternal(Of Double)(file, ofVector, encoding.CodePage)
                Case "integer" : Return loadListInternal(Of Long)(file, ofVector, encoding.CodePage)
                Case "logical" : Return loadListInternal(Of Boolean)(file, ofVector, encoding.CodePage)
                Case "any"
                    Return file.LoadJsonFile(Of Dictionary(Of String, Object))(knownTypes:=listKnownTypes)
                Case Else
                    Return Internal.debug.stop($"Invalid data mode: '{mode}'!", envir)
            End Select
        End Function

        Private Function loadListInternal(Of T)(file As String, ofVector As Boolean, encoding As Encoding) As Object
            If ofVector Then
                Return file.LoadJsonFile(Of Dictionary(Of String, T()))(encoding)
            Else
                Return file.LoadJsonFile(Of Dictionary(Of String, T))(encoding)
            End If
        End Function

        ''' <summary>
        ''' open a *.gz file for make file data read/write
        ''' </summary>
        ''' <param name="description"></param>
        ''' <returns>stream object for read/write data</returns>
        <ExportAPI("gzfile")>
        <RApiReturn(GetType(Stream))>
        Public Function gzfile(description$, Optional open As FileMode = FileMode.OpenOrCreate, Optional env As Environment = Nothing) As Object
            If open = FileMode.Open Then
                If description.FileExists(True) Then
                    Using file As Stream = description.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                        Dim gz As New GZipStream(file, mode:=CompressionMode.Decompress)
                        Dim data As New MemoryStream

                        Call gz.CopyTo(data)
                        Call data.Flush()
                        Call data.Seek(Scan0, SeekOrigin.Begin)

                        Return data
                    End Using
                Else
                    Return Internal.debug.stop($"the given gzip file('{description}') is not existsed or zero byte size on your filesystem!", env)
                End If
            Else
                Return Internal.debug.stop("write gzip file stream has not been implemented yet!", env)
            End If
        End Function

        ''' <summary>
        ''' check of the target gz file data is corrupted or not
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' true for no error in gzfile, false means the given gz file is corrupted
        ''' </returns>
        <ExportAPI("gz_check")>
        Public Function gzcheck(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
            Dim auto_close As Boolean = False
            Dim s = GetFileStream(file, FileAccess.Read, env, is_filepath:=auto_close)

            If s Like GetType(Message) Then
                Return s.TryCast(Of Message)
            End If

            Try
                Using fileStream As Stream = s.TryCast(Of Stream)
                    Using gzipStream As New GZipStream(fileStream, CompressionMode.Decompress)
                        ' 尝试完整读取解压后的数据
                        Dim buffer(4096) As Byte
                        While gzipStream.Read(buffer, 0, buffer.Length) > 0
                        End While
                    End Using
                End Using
                Return True
            Catch ex As InvalidDataException
                Return False
            Catch ex As Exception
                Return False
            Finally
                If auto_close Then
                    Call s.TryCast(Of Stream).Dispose()
                End If
            End Try
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' stdin(), stdout() and stderr() return connection objects.
        ''' </returns>
        ''' <remarks>
        ''' stdin(), stdout() and stderr() are standard connections corresponding to input, output and error on the console 
        ''' respectively (and not necessarily to file streams). They are text-mode connections of class "terminal" which 
        ''' cannot be opened or closed, and are read-only, write-only and write-only respectively. The stdout() and stderr() 
        ''' connections can be re-directed by sink (and in some circumstances the output from stdout() can be split: see the 
        ''' help page).
        ''' 
        ''' The encoding For stdin() When redirected can be Set by the command-line flag --encoding.
        ''' 
        ''' nullfile() returns filename of the null device ("/dev/null" on Unix, "nul:" on Windows).
        ''' 
        ''' showConnections returns a matrix Of information. If a connection Object has been lost Or forgotten, getConnection 
        ''' will take a row number from the table And Return a connection Object For that connection, which can be used To 
        ''' close the connection, For example. However, If there Is no R level Object referring To the connection it will be 
        ''' closed automatically at the Next garbage collection (except For gzcon connections).
        ''' 
        ''' closeAllConnections closes(And destroys) all user connections, restoring all sink diversions As it does so.
        ''' 
        ''' isatty returns True If the connection Is one Of the Class "terminal" connections And it Is apparently connected To 
        ''' a terminal, otherwise False. This may Not be reliable In embedded applications, including GUI consoles.
        ''' 
        ''' getAllConnections returns a sequence Of Integer connection descriptors For use With getConnection, corresponding 
        ''' To the row names Of the table returned by showConnections(all = True).
        ''' 
        ''' stdin() refers to the ‘console’ and not to the C-level ‘stdin’ of the process. The distinction matters in GUI 
        ''' consoles (which may not have an active ‘stdin’, and if they do it may not be connected to console input), and also 
        ''' in embedded applications. If you want access to the C-level file stream ‘stdin’, use file("stdin").
        ''' 
        ''' When R Is reading a script from a file, the file Is the 'console’: this is traditional usage to allow in-line 
        ''' data (see ‘An Introduction to R’ for an example).
        ''' </remarks>
        <ExportAPI("stdin")>
        Public Function stdin_dev() As Stream
            Return Console.OpenStandardInput
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
        ''' <param name="repo">
        ''' this function will open an internal block stream if this repository reference has been specificed.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' + ``stdin``  for stdinput stream, and
        ''' + ``stdout`` for stdoutput stream.
        ''' </remarks>
        <ExportAPI("file")>
        Public Function file(description$,
                             Optional open As FileModeDescriptor = FileModeDescriptor.Open,
                             Optional truncate As Boolean = False,
                             Optional repo As IFileSystemEnvironment = Nothing) As Stream

            If description.TextEquals("stdin") Then
                ' read from console stdinput
                ' can not truncated
                If truncate Then
                    Call $"you can'nt truncate the standard input stream.".warning
                End If

                Return Console.OpenStandardInput
            ElseIf description.TextEquals("stdout") Then
                ' write to console stdoutput
                ' can not truncated
                If truncate Then
                    Call $"you can'nt truncate the standard output stream.".warning
                End If

                Return Console.OpenStandardOutput
            ElseIf Not repo Is Nothing Then
                Return repo.OpenFile(description, open, access:=If(truncate, FileAccess.Write, FileAccess.Read))
            Else
                If open = FileMode.Truncate OrElse open = FileMode.CreateNew Then
                    Return description.Open(open, doClear:=truncate)
                ElseIf open = FileMode.Append Then
                    If description.FileExists Then
                        Dim exists_file As New FileStream(description, CType(CInt(open), FileMode))
                        ' seek to end of file for append mode
                        exists_file.Seek(exists_file.Length, SeekOrigin.Begin)
                        Return exists_file
                    Else
                        ' create new file
                        Return description.Open(FileMode.OpenOrCreate, doClear:=truncate)
                    End If
                Else
                    Return description.Open(open, doClear:=truncate)
                End If
            End If
        End Function

        ''' <summary>
        ''' ### ransfer Binary Data To and From Connections
        ''' 
        ''' Read binary data from or write binary data to a connection
        ''' or raw vector.
        ''' </summary>
        ''' <param name="con">A connection Object Or a character 
        ''' String naming a file Or a raw vector.</param>
        ''' <param name="n">
        ''' numeric. The (maximal) number of records to be read. 
        ''' You can use an over-estimate here, but not too large 
        ''' as storage is reserved for n items.
        ''' </param>
        ''' <param name="size">
        ''' Integer.The number Of bytes per element In the Byte 
        ''' stream. The Default, NA_integer_, uses the natural 
        ''' size. Size changing Is Not supported For raw And 
        ''' complex vectors.
        ''' </param>
        ''' <param name="signed">
        ''' logical. Only used for integers of sizes 1 and 2, when 
        ''' it determines if the quantity on file should be regarded
        ''' as a signed or unsigned integer.
        ''' </param>
        ''' <param name="endian">The endian-ness ("big" Or "little") 
        ''' Of the target system For the file. Using "swap" will force 
        ''' swapping endian-ness.
        ''' </param>
        ''' <param name="what">
        ''' Either an object whose mode will give the mode of the 
        ''' vector to be read, or a character vector of length one 
        ''' describing the mode: one of "numeric", "double", 
        ''' "integer", "int", "logical", "complex", "character", 
        ''' "raw".
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("readBin")>
        <CodeAnalysis.SuppressMessage("Reliability", "CA2022:Avoid inexact read with 'Stream.Read'", Justification:="<Pending>")>
        Public Function readBin(<RRawVectorArgument> con As Object, what As Object,
                                Optional n As Integer = 1,
                                Optional size As Integer = NA_integer_,
                                Optional signed As Boolean = True,
                                Optional endian As endianness = endianness.big,
                                <RListObjectArgument>
                                Optional args As list = Nothing,
                                Optional env As Environment = Nothing) As Object

            Dim is_path As Boolean = False
            Dim buf = GetFileStream(con, FileAccess.Read, env, is_filepath:=is_path)

            If buf Like GetType(Message) Then
                Return buf.TryCast(Of Message)
            End If

            Dim br As New BinaryReader(buf.TryCast(Of Stream))

            If what Is Nothing Then
                what = Components.What.raw
            End If

            If TypeOf what Is What Then
                Dim cwhat = WhatReader.LoadWhat(DirectCast(what, What))
                Dim rwhat = WhatReader.ReadWhat(DirectCast(what, What))

                Throw New NotImplementedException
            ElseIf TypeOf what Is String Then
                Select Case CStr(what).ToLower
                    Case "raw"
                        Dim bytes As Byte() = New Byte(n - 1) {}
                        buf.TryCast(Of Stream).Read(bytes, Scan0, n)
                        Return bytes
                    Case Else
                        ' invoke generic function overloads
                        Return readBinOverloads(buf.TryCast(Of Stream), CStr(what), con, is_path, args, env)
                End Select
            Else
                Return Message.InCompatibleType(GetType(What), what.GetType, env)
            End If
        End Function

        Private Function readBinOverloads(s As Stream, what As String, con As Object, is_path As Boolean, args As list, env As Environment) As Object
            ' invoke generic function for parse
            ' binary file as R#/clr object
            Dim fname As String = $"readBin.{what}"
            Dim f As GenericFunction = generic.get(fname, GetType(Stream))

            If f Is Nothing Then
                Return generic.missingGenericSymbol(fname, Nothing, env)
            End If

            Dim out As Object

            Try
                out = f(s, args, env)
            Catch ex As Exception
                If is_path Then
                    Throw New Exception($"error_file: {CLRVector.asCharacter(con).First}", ex)
                Else
                    Throw
                End If
            End Try

            If is_path Then
                Call s.Dispose()
            End If

            Return out
        End Function

        Const NA_integer_ = Integer.MinValue

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[object]"></param>
        ''' <param name="con"></param>
        ''' <param name="size"></param>
        ''' <param name="endian"></param>
        ''' <param name="useBytes"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("writeBin")>
        Public Function writeBin(<RRawVectorArgument> [object] As Object, con As Object,
                                 Optional size As Integer = NA_integer_,
                                 Optional endian As endianness = endianness.big,
                                 Optional useBytes As Boolean = False,
                                 <RListObjectArgument>
                                 Optional args As list = Nothing,
                                 Optional env As Environment = Nothing) As Object

            Dim buf = GetFileStream(con, FileAccess.Write, env)

            If buf Like GetType(Message) Then
                Return buf.TryCast(Of Message)
            End If

            If [object] Is Nothing Then
                Return False
            End If

            If Not generic.exists("writeBin") Then
                Return generic.missingGenericSymbol("writeBin", Nothing, env)
            End If

            Dim f As GenericFunction = generic.get("writeBin", [object].GetType())

            If f Is Nothing Then
                Return generic.missingGenericSymbol("writeBin", [object].GetType, env)
            ElseIf args Is Nothing Then
                args = New list(slot("con") = buf.TryCast(Of Stream))
            Else
                args.slots("con") = buf.TryCast(Of Stream)
            End If

            Dim out = f([object], args, env)

            If TypeOf con Is String Then
                Call buf.TryCast(Of Stream).Dispose()
            End If

            Return out
        End Function

        <RGenericOverloads("writeBin")>
        Private Function writeBinDataframe(dataframe As dataframe, args As list, env As Environment) As Object
            Dim con As Stream = args!con
            Dim buffer As New dataframeBuffer(dataframe, env)

            Call buffer.Serialize(con)
            Call con.Flush()

            Return True
        End Function

        <RGenericOverloads("readBin.dataframe")>
        Private Function readBinDataframe(s As Stream, args As list, env As Environment) As Object
            Dim buffer As New dataframeBuffer(s)
            Dim df As dataframe = buffer.getFrame
            Return df
        End Function

        ''' <summary>
        ''' close connections, i.e., “generalized files”, such as possibly compressed files, URLs, pipes, etc.
        ''' </summary>
        ''' <param name="con">a connection.</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("close")>
        <RApiReturn(GetType(Boolean))>
        Public Function close(con As Object, Optional env As Environment = Nothing) As Object
            If con Is Nothing Then
                Return Internal.debug.stop("the required connection can not be nothing!", env)
            ElseIf TypeOf con Is Stream Then
                Try
                    With DirectCast(con, Stream)
                        Call .Flush()
                        Call .Close()
                        Call .Dispose()
                    End With

                    Return True
                Catch ex As Exception
                    Call env.AddMessage(ex.Message)
                    Return False
                End Try
            ElseIf TypeOf con Is StreamWriter Then
                Try
                    With DirectCast(con, StreamWriter)
                        Call .Flush()
                        Call .Close()
                        Call .Dispose()
                    End With

                    Return True
                Catch ex As Exception
                    Call env.AddMessage(ex.Message)
                    Return False
                End Try
            ElseIf con.GetType.ImplementInterface(GetType(IDisposable)) Then
                Try
                    Call DirectCast(con, IDisposable).Dispose()
                    Return True
                Catch ex As Exception
                    Call env.AddMessage(ex.Message)
                    Return False
                End Try
            Else
                Return Internal.debug.stop(Message.InCompatibleType(GetType(Stream), con.GetType, env), env)
            End If
        End Function

        ''' <summary>
        ''' open a zip file
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' a folder liked list object
        ''' </returns>
        <ExportAPI("open.zip")>
        <RApiReturn(GetType(ZipFolder))>
        Public Function openZip(file As String, Optional env As Environment = Nothing) As Object
            If Not file.FileExists Then
                Return debug.stop({"target file is not exists on your file system!", "file: " & file}, env)
            Else
                Return New ZipFolder(file)
            End If
        End Function

#If NET8_0_OR_GREATER Then
        <ExportAPI("open.tar.gz")>
        <RApiReturn(GetType(TarGzFileSystem))>
        Public Function openTargzip(file As String, Optional env As Environment = Nothing) As Object
            If Not file.FileExists(ZERO_Nonexists:=True) Then
                Return Internal.debug.stop("file is not existsed!", env)
            Else
                Return New TarGzFileSystem(file)
            End If
        End Function
#End If

        ''' <summary>
        ''' decompression of a gzip file and get the deflate file data stream.
        ''' </summary>
        ''' <param name="file">
        ''' the file path or file stream data.
        ''' </param>
        ''' <param name="tmpfileWorker">
        ''' using tempfile for process the large data file which its file length 
        ''' is greater then the memorystream its upbound capacity.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("open.gzip")>
        <RApiReturn(GetType(Stream))>
        Public Function openGzip(file As Object,
                                 Optional tmpfileWorker$ = Nothing,
                                 Optional env As Environment = Nothing) As Object

            If file Is Nothing Then
                Return Nothing
            End If

            Dim originalFileStream As Stream

            If TypeOf file Is String Then
                originalFileStream = DirectCast(file, String).Open(FileMode.Open, doClear:=False)
            ElseIf TypeOf file Is Stream Then
                originalFileStream = DirectCast(file, Stream)
            Else
                Return Internal.debug.stop(Message.InCompatibleType(GetType(Stream), file.GetType, env), env)
            End If

            Using originalFileStream
                Dim deflate As Stream

                If Not tmpfileWorker.StringEmpty Then
                    deflate = tmpfileWorker.Open(FileMode.OpenOrCreate, doClear:=True)
                Else
                    deflate = New MemoryStream
                End If

                Using decompressionStream As New GZipStream(originalFileStream, CompressionMode.Decompress)
                    decompressionStream.CopyTo(deflate)
                    deflate.Position = Scan0
                End Using

                Return deflate
            End Using
        End Function

        ''' <summary>
        ''' create a new buffer object
        ''' </summary>
        ''' <param name="type">the r-sharp internal buffer data type</param>
        ''' <param name="mime">the data mime-type for http response, some buffer 
        ''' object type may required of this parameter for specific the correct 
        ''' mine content type.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function usually works for the http web services.
        ''' </remarks>
        <ExportAPI("buffer")>
        Public Function buffer(Optional type As BufferObjects = BufferObjects.raw,
                               Optional mime As String = "~ifelse(type == 'text' || type == 100, 'text/html','" & MIME.Unknown & "')",
                               Optional env As Environment = Nothing) As Object
            Select Case type
                Case BufferObjects.raw : Return New rawBuffer
                Case BufferObjects.text : Return New textBuffer With {.mime = mime}
                Case BufferObjects.bitmap : Return New bitmapBuffer
                Case BufferObjects.vector : Return New vectorBuffer(env)
                Case BufferObjects.dataframe : Return New dataframeBuffer(env)
                Case BufferObjects.rscript : Return New rscriptBuffer
                Case BufferObjects.list : Return New listBuffer(env)
                Case Else
                    Return Internal.debug.stop(New NotImplementedException(type.Description), env)
            End Select
        End Function

        ''' <summary>
        ''' ### Create Names for Temporary Files
        ''' 
        ''' ``tempfile`` returns a vector of character strings 
        ''' which can be used as names for temporary files.
        ''' </summary>
        ''' <param name="pattern">a non-empty character vector giving the initial part of the name.</param>
        ''' <param name="tmpdir">a non-empty character vector giving the directory name</param>
        ''' <param name="fileext">a non-empty character vector giving the file extension</param>
        ''' <returns>
        ''' a character vector giving the names of possible (temporary) files. 
        ''' Note that no files are generated by tempfile.
        ''' </returns>
        ''' <remarks>
        ''' The length of the result is the maximum of the lengths of the three arguments; 
        ''' values of shorter arguments are recycled.
        '''
        ''' The names are very likely To be unique among calls To tempfile In an R session 
        ''' And across simultaneous R sessions (unless tmpdir Is specified). The filenames 
        ''' are guaranteed Not To be currently In use.
        '''
        ''' The file name Is made by concatenating the path given by tmpdir, the pattern 
        ''' String, a random String In hex And a suffix Of fileext.
        '''
        ''' By Default, tmpdir will be the directory given by tempdir(). This will be a 
        ''' subdirectory of the per-session temporary directory found by the following 
        ''' rule when the R session Is started. The environment variables TMPDIR, TMP And TEMP 
        ''' are checked in turn And the first found which points to a writable directory Is 
        ''' used: If none succeeds the value Of R_USER (see Rconsole) Is used. If the path 
        ''' To the directory contains a space In any Of the components, the path returned will 
        ''' use the shortnames version Of the path. Note that setting any Of these environment 
        ''' variables In the R session has no effect On tempdir(): the per-session temporary 
        ''' directory Is created before the interpreter Is started.
        ''' </remarks>
        <ExportAPI("tempfile")>
        <RApiReturn(GetType(String))>
        Public Function tempfile(<RRawVectorArgument>
                                 Optional pattern As Object = "file",
                                 <RDefaultExpression>
                                 Optional tmpdir$ = "~tempdir()",
                                 <RRawVectorArgument>
                                 Optional fileext As Object = "",
                                 Optional env As Environment = Nothing) As Object

            Dim patterns As String() = CLRVector.asCharacter(pattern)
            Dim exts As String() = CLRVector.asCharacter(fileext)
            Dim files As New List(Of String)

            If patterns.Length = 1 Then
                For Each ext As String In exts
                    files += $"{tmpdir}/{patterns(Scan0)}{NextTempToken()}{ext}".GetFullPath
                Next
            ElseIf exts.Length = 1 Then
                For Each patternStr As String In patterns
                    files += $"{tmpdir}/{patternStr}{NextTempToken()}{exts(Scan0)}".GetFullPath
                Next
            ElseIf patterns.Length <> exts.Length Then
                Return Internal.debug.stop({
                    $"the size of filename patterns should be equals to the file extension names!",
                    $"sizeof pattern: {patterns.Length}",
                    $"sizeof fileext: {exts.Length}"
                }, env)
            Else
                For i As Integer = 0 To exts.Length - 1
                    files += $"{tmpdir}/{patterns(i)}{NextTempToken()}{exts(i)}".GetFullPath
                Next
            End If

            Return files.ToArray
        End Function

        Private Function NextTempToken() As String
            Return (randf.NextInteger(10000).ToString & now.ToString).MD5.Substring(3, 9)
        End Function

        ''' <summary>
        ''' ### Create Names For Temporary Files
        ''' </summary>
        ''' <param name="check">
        ''' logical indicating if ``tmpdir()`` should be checked and recreated if no longer valid.
        ''' </param>
        ''' <returns>the path of the per-session temporary directory.</returns>
        ''' <remarks>
        ''' + On Windows, both will use a backslash as the path separator.
        ''' + On a Unix-alike, the value will be an absolute path (unless tmpdir Is set to a relative path), 
        '''   but it need Not be canonical (see normalizePath) And on macOS it often Is Not.
        ''' </remarks>
        <ExportAPI("tempdir")>
        Public Function tempdir(Optional check As Boolean = False) As String
            Static dir As String = (App.SysTemp & $"/Rtmp{App.PID.ToString.MD5.Substring(3, 6).ToUpper}").GetDirectoryFullPath

            If check Then
                Call dir.MakeDir
            End If

            Return dir
        End Function

        ''' <summary>
        ''' File renames
        ''' </summary>
        ''' <param name="from">character vectors, containing file names Or paths.</param>
        ''' <param name="to">character vectors, containing file names Or paths.</param>
        ''' <param name="env"></param>
        <ExportAPI("file.rename")>
        Public Sub fileRename(from$, to$, Optional env As Environment = Nothing)
            If Not from.FileExists Then
                Call env.AddMessage({$"the given file is not exists...", $"source file: {from}"}, MSG_TYPES.WRN)
            Else
                Call [to].ParentPath.MakeDir
                Call from.FileMove(to$)
            End If
        End Sub

        ''' <summary>
        ''' ## Delete files or directories
        ''' 
        ''' ``file.remove`` attempts to remove the files named 
        ''' in its argument. On most Unix platforms ‘file’ 
        ''' includes empty directories, symbolic links, fifos 
        ''' and sockets. On Windows, ‘file’ means a regular file 
        ''' and not, say, an empty directory.
        ''' </summary>
        ''' <param name="x">
        ''' character vectors, containing file names or paths.
        ''' </param>
        <ExportAPI("file.remove")>
        Public Function fileRemove(x As String(),
                                   Optional verbose As Boolean? = Nothing,
                                   Optional env As Environment = Nothing) As Object

            Dim println As Action(Of Object)

            verbose = env.verboseOption(opt:=verbose)

            If verbose Then
                println = env.WriteLineHandler
            Else
                println = AddressOf App.DoNothing
            End If

            For Each file As String In x.SafeQuery
                If file.DirectoryExists Then
                    If isSystemDir(file) Then
                        Return Internal.debug.stop({$"system directory: '{file}' is not allowed to erase!", "dir: " & file}, env)
                    End If
                End If

                Call file.DeleteFile(verbose:=println, strictFile:=False)
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Delete files or directories
        ''' </summary>
        ''' <remarks>
        ''' this function is the alias name of the function
        ''' ``file.remove``.
        ''' </remarks>
        ''' <param name="x"></param>
        <ExportAPI("unlink")>
        Public Function unlinks(x As String(), Optional env As Environment = Nothing) As Object
            Return fileRemove(x, env:=env)
        End Function

        ''' <summary>
        ''' delete all contents in target directory
        ''' </summary>
        ''' <param name="dir"></param>
        <ExportAPI("erase")>
        Public Function [erase](dir As String, Optional env As Environment = Nothing) As Object
            If isSystemDir(dir) Then
                Return Internal.debug.stop({$"system directory: '{dir}' is not allowed to erase!", "dir: " & dir}, env)
            Else
                Call env.AddMessage({$"all of the content files in target directory '{dir}' will be deleted.", $"dir: {dir}"}, MSG_TYPES.WRN)
            End If

            For Each file As String In dir.ListFiles
                Call file.DeleteFile
            Next
            For Each folder As String In dir.ListDirectory(fsOptions.SearchAllSubDirectories)
                Call Global.System.IO.Directory.Delete(folder, recursive:=True)
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Check of the given directory path is a system internal directory
        ''' or not.
        ''' 
        ''' The system directories are:
        ''' + Windows: c:\, c:/, c:, c:\program files, c:\program files (x86), c:\windows, c:\windows\system32, c:\windows\syswow64
        ''' + Linux: /bin, /boot, /dev, /etc, /home, /lib, /lib64, /media, /mnt, /opt, /root, /run, /sbin, /srv, /sys, /usr, /var
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <returns></returns>
        <ExportAPI("is.sysdir")>
        Public Function isSystemDir(dir As String) As Boolean
            ' is the root directory '/'
            If dir.IsPattern("/+") Then
                Return True
            End If

            Select Case dir.ToLower
                ' windows
                Case "c:\",
                     "c:/",
                     "c:",
                     "c:\program files",
                     "c:\program files (x86)",
                     "c:\windows",
                     "c:\windows\system32",
                     "c:\windows\syswow64"
                    Return True
                    ' linux
                Case "/bin",
                     "/boot",
                     "/dev",
                     "/etc",
                     "/home",
                     "/lib",
                     "/lib64",
                     "/media",
                     "/mnt",
                     "/opt",
                     "/root",
                     "/run",
                     "/sbin",
                     "/srv",
                     "/sys",
                     "/usr",
                     "/var"
                    Return True
                Case Else
                    ' is not a system directory
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' read file as data URI string
        ''' </summary>
        ''' <param name="file">the file path</param>
        ''' <returns></returns>
        <ExportAPI("dataUri")>
        Public Function dataUri(file As Object, Optional env As Environment = Nothing) As Object
            If file Is Nothing Then
                Return Internal.debug.stop("the required of the file object can not be nothing!", env)
            ElseIf TypeOf file Is Bitmap Then
                Return New DataURI(DirectCast(file, Bitmap)).ToString
            ElseIf TypeOf file Is Image Then
                Return New DataURI(DirectCast(file, Image)).ToString
            ElseIf TypeOf file Is String Then
                Return New DataURI(DirectCast(file, String)).ToString
            Else
                Return Message.InCompatibleType(GetType(String), file.GetType, env)
            End If
        End Function

        ''' <summary>
        ''' create a in-memory byte stream object
        ''' </summary>
        ''' <param name="byts"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("bytes")>
        Public Function bytes(<RRawVectorArgument> byts As Object, Optional env As Environment = Nothing) As MemoryStream
            Dim stream As Byte() = CLRVector.asRawByte(byts)
            Dim ms As New MemoryStream(stream)

            Return ms
        End Function
    End Module
End Namespace
