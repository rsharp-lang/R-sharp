#Region "Microsoft.VisualBasic::36a01e8560441d5442190fc0e435af47, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/file.vb"

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

'   Total Lines: 1541
'    Code Lines: 875
' Comment Lines: 519
'   Blank Lines: 147
'     File Size: 69.41 KB


'     Module file
' 
'         Function: [erase], basename, buffer, bytes, close
'                   dataUri, dir_exists, dirCopy, dirCreate, dirname
'                   exists, file, file_ext, filecopy, fileExt
'                   fileinfo, fileInfoByFile, filepath, filesize, getRelativePath
'                   GetSha1Hash, getwd, handleWriteLargeTextStream, handleWriteTextArray, isSystemDir
'                   listDirs, listFiles, loadListInternal, NextTempToken, normalizeFileName
'                   normalizePath, openDir, openGzip, openZip, readBin
'                   readFromFile, readFromStream, readLines, readList, readText
'                   Rhome, saveList, scanZipFiles, setwd, tempdir
'                   tempfile, writeLines
' 
'         Sub: fileRemove, fileRename, unlinks
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.IO.Compression
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.UNIX
Imports Microsoft.VisualBasic.Net.Http
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
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' #### File Manipulation
    ''' 
    ''' These functions provide a low-level interface to the computer's file system.
    ''' </summary>
    ''' 
    <Package("file")>
    Public Module file

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
        <ExportAPI("getRelativePath")>
        Public Function getRelativePath(pathname As String(), <RDefaultExpression> Optional relativeTo As Object = "~getwd()") As String()
            Return pathname _
                .SafeQuery _
                .Select(Function(path) RelativePath(relativeTo, path, appendParent:=False)) _
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
        ''' <returns>Double: File size In bytes.</returns>
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
        Public Function filesize(x As String) As Long
            Return x.FileLength
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

            Dim array As Object = base.c(x, env)

            If TypeOf array Is Message Then
                Return array
            Else
                Return DirectCast(array, Array) _
                    .AsObjectEnumerator _
                    .Select(AddressOf any.ToString) _
                    .JoinBy(fsep)
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
        Public Function fileinfo(<RRawVectorArgument> files As Object, Optional env As Environment = Nothing) As Object
            Dim fileList As String() = CLRVector.asCharacter(files)

            If fileList.IsNullOrEmpty Then
                Return Nothing
            ElseIf fileList.Length = 1 Then
                Return fileInfoByFile(fileList(Scan0))
            Else
                Return fileList _
                    .Select(Function(path) path.GetFullPath) _
                    .Distinct _
                    .ToDictionary(Function(filepath) filepath,
                                  Function(filepath)
                                      Return fileInfoByFile(filepath)
                                  End Function) _
                    .DoCall(Function(slots)
                                Return New list With {
                                    .slots = slots
                                }
                            End Function)
            End If
        End Function

        Private Function fileInfoByFile(filepath As String) As Object
            Dim fileInfoObj As New FileInfo(filepath)
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
                Return Internal.debug.stop("number of from files is not equals to the number of target file locations!", env)
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
        ''' <param name="to"></param>
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
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("dir.open")>
        Public Function openDir(dir As String, Optional env As Environment = Nothing) As FileIO.Directory
            If Not dir.DirectoryExists Then
                Call env.AddMessage($"target directory: {dir.GetDirectoryFullPath} is not found on the file system...")
                Call dir.MakeDir
            End If

            Return FileIO.Directory.FromLocalFileSystem(dir)
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
        Public Function dirname(<RRawVectorArgument> fileNames As Object, Optional env As Environment = Nothing) As Object
            Return env.EvaluateFramework(Of String, String)(fileNames, Function(path) path.ParentPath)
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
        ''' <returns>
        ''' this function returns FALSE if the given files value is NULL
        ''' </returns>
        <ExportAPI("file.exists")>
        <RApiReturn(GetType(Boolean))>
        Public Function exists(<RRawVectorArgument> files As Object,
                               Optional ZERO_Nonexists As Boolean = False,
                               Optional env As Environment = Nothing) As Object

            If files Is Nothing Then
                Return False
            End If

            Return env.EvaluateFramework(Of String, Boolean)(files, Function(path) path.FileExists(ZERO_Nonexists))
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
                Call $"in dir.create(""{path}"") : '{path}' already exists".Warning
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
        ''' Read Text Lines from a Connection
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
        ''' <returns></returns>
        <ExportAPI("readLines")>
        <RApiReturn(GetType(String))>
        Public Function readLines(con As Object,
                                  Optional encoding As Encodings = Encodings.UTF8,
                                  Optional stream As Boolean = False,
                                  Optional env As Environment = Nothing) As Object

            If TypeOf con Is Stream Then
                Return readFromStream(DirectCast(con, Stream), stream)
            ElseIf TypeOf con Is WebResponseResult Then
                ' read web result html text into multuple text lines
                Return DirectCast(con, WebResponseResult).html.LineTokens
            Else
                Dim str = CLRVector.asCharacter(con)
                Dim filepath As String = str.ElementAtOrDefault(Scan0)

                If str.IsNullOrEmpty Then
                    Return Internal.debug.stop("no file is specified for read data!", env)
                End If

                Return readFromFile(filepath, encoding, env)
            End If
        End Function

        Private Function readFromFile(filepath As String, encoding As Encodings, env As Environment) As Object
            If Not filepath.FileExists Then
                If env.globalEnvironment.options.strict Then
                    Return Internal.debug.stop($"the given file '{filepath}' is missing!", env)
                End If
            End If

            Return filepath.ReadAllLines(encoding.CodePage)
        End Function

        Private Function readFromStream(con As Stream, stream As Boolean) As Object
            Dim text As New StreamReader(con)
            Dim line As Value(Of String) = ""

            If stream Then
                Return Iterator Function() As IEnumerable(Of String)
                           Do While True
                               If Not (line = text.ReadLine) Is Nothing Then
                                   Yield CType(line, String)
                               End If
                           Loop
                       End Function() _
                                      _
                    .DoCall(AddressOf pipeline.CreateFromPopulator)
            Else
                Dim lines As New List(Of String)

                Do While True
                    If Not (line = text.ReadLine) Is Nothing Then
                        Call lines.Add(line)
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
                                   Optional sep$ = vbCrLf,
                                   Optional fs As IFileSystemEnvironment = Nothing,
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

            If TypeOf text Is pipeline Then
                Return DirectCast(text, pipeline) _
                    .populates(Of String)(env) _
                    .handleWriteLargeTextStream(con, sep, fs, env)
            ElseIf TypeOf text Is String Then
                Return DirectCast(text, String).handleWriteTextArray(con, fs, env)
            Else
                Return REnv.asVector(Of Object)(text) _
                    .AsObjectEnumerator _
                    .JoinBy(sep) _
                    .handleWriteTextArray(con, fs, env)
            End If

            Return Nothing
        End Function

        <Extension>
        Private Function handleWriteLargeTextStream(text As IEnumerable(Of String),
                                                    con As Object,
                                                    sep As String,
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
                    Call text.SaveTo(con, Encodings.UTF8WithoutBOM.CodePage)
                Else
                    Call fs.WriteText(text.JoinBy(vbCr), con)
                End If
            ElseIf TypeOf con Is textBuffer Then
                DirectCast(con, textBuffer).text = text.JoinBy(sep)
                Return con
            ElseIf TypeOf con Is ITextWriter OrElse con.GetType.IsInheritsFrom(GetType(ITextWriter)) Then
                Dim dev As ITextWriter = DirectCast(con, ITextWriter)

                For Each line As String In text
                    Call dev.WriteLine(line)
                Next

                Return con
            Else
                Return Internal.debug.stop(New NotSupportedException($"invalid buffer type: {con.GetType.FullName}!"), env)
            End If

            Return Nothing
        End Function

        <Extension>
        Private Function handleWriteTextArray(text As String,
                                              con As Object,
                                              fs As IFileSystemEnvironment,
                                              env As Environment) As Object

            If con Is Nothing OrElse (TypeOf con Is String AndAlso DirectCast(con, String).StringEmpty) Then
                Dim stdOut As Action(Of String)

                If env.globalEnvironment.stdout Is Nothing Then
                    stdOut = AddressOf Console.WriteLine
                Else
                    stdOut = AddressOf env.globalEnvironment.stdout.WriteLine
                End If

                Call stdOut(text)
            ElseIf TypeOf con Is String Then
                If fs Is Nothing Then
                    Call text.SaveTo(con, Encodings.UTF8WithoutBOM.CodePage)
                Else
                    Call fs.WriteText(text, con)
                End If
            ElseIf TypeOf con Is Stream Then
                Dim writer As New StreamWriter(DirectCast(con, Stream))
                Call writer.WriteLine(text)
                Call writer.Flush()
            ElseIf TypeOf con Is textBuffer Then
                DirectCast(con, textBuffer).text = text
                Return con
            ElseIf TypeOf con Is ITextWriter OrElse con.GetType.IsInheritsFrom(GetType(ITextWriter)) Then
                DirectCast(con, ITextWriter).WriteLine(text)
                Return con
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
        ''' <remarks>
        ''' + ``stdin``  for stdinput stream, and
        ''' + ``stdout`` for stdoutput stream.
        ''' </remarks>
        <ExportAPI("file")>
        Public Function file(description$,
                             Optional open As FileMode = FileMode.OpenOrCreate,
                             Optional truncate As Boolean = False) As Stream

            If description.TextEquals("stdin") Then
                ' read from console stdinput
                ' can not truncated
                If truncate Then
                    Call $"you can'nt truncate the standard input stream.".Warning
                End If

                Return Console.OpenStandardInput
            ElseIf description.TextEquals("stdout") Then
                ' write to console stdoutput
                ' can not truncated
                If truncate Then
                    Call $"you can'nt truncate the standard output stream.".Warning
                End If

                Return Console.OpenStandardOutput
            Else
                If open = FileMode.Truncate OrElse open = FileMode.CreateNew Then
                    Return description.Open(open, doClear:=truncate)
                ElseIf open = FileMode.Append Then
                    If description.FileExists Then
                        Dim exists_file As New FileStream(description, open)
                        ' seek to end of file for append mode
                        exists_file.Seek(exists_file.Length, SeekOrigin.Begin)
                        Return exists_file
                    Else
                        ' create new file
                        Return description.Open(FileMode.OpenOrCreate, doClear:=truncate)
                    End If
                Else
                    Return description.Open(open, doClear:=False)
                End If
            End If
        End Function

        ''' <summary>
        ''' ### ransfer Binary Data To and From Connections
        ''' 
        ''' Read binary data from or write binary data to a connection or raw vector.
        ''' </summary>
        ''' <param name="file">A connection Object Or a character String naming a file Or a raw vector.</param>
        ''' <returns></returns>
        <ExportAPI("readBin")>
        Public Function readBin(file As String) As Object
            Return file.ReadBinary
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
                Catch ex As Exception
                    Call env.AddMessage(ex.Message)
                End Try

                Return True
            ElseIf TypeOf con Is StreamWriter Then
                Try
                    With DirectCast(con, StreamWriter)
                        Call .Flush()
                        Call .Close()
                        Call .Dispose()
                    End With
                Catch ex As Exception
                    Call env.AddMessage(ex.Message)
                End Try

                Return True
            ElseIf con.GetType.ImplementInterface(GetType(IDisposable)) Then
                Try
                    Call DirectCast(con, IDisposable).Dispose()
                Catch ex As Exception
                    Call env.AddMessage(ex.Message)
                End Try

                Return True
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
        ''' <param name="type"></param>
        ''' <returns></returns>
        <ExportAPI("buffer")>
        Public Function buffer(Optional type As BufferObjects = BufferObjects.raw, Optional env As Environment = Nothing) As Object
            Select Case type
                Case BufferObjects.raw : Return New rawBuffer
                Case BufferObjects.text : Return New textBuffer
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
                                 Optional fileext As Object = ".tmp",
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
        Public Sub fileRemove(x As String(),
                              Optional verbose As Boolean? = Nothing,
                              Optional env As Environment = Nothing)

            Dim println As Action(Of Object)

            If verbose Is Nothing Then
                verbose = env.globalEnvironment.options.verbose
            End If

            If verbose Then
                println = env.WriteLineHandler
            Else
                println = Sub(any)
                              ' do nothing
                          End Sub
            End If

            For Each file As String In x.SafeQuery
                Call file.DeleteFile(verbose:=println)
            Next
        End Sub

        ''' <summary>
        ''' Delete files or directories
        ''' </summary>
        ''' <remarks>
        ''' this function is the alias name of the function
        ''' ``file.remove``.
        ''' </remarks>
        ''' <param name="x"></param>
        <ExportAPI("unlink")>
        Public Sub unlinks(x As String())
            Call fileRemove(x)
        End Sub

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
                Call Directory.Delete(folder)
            Next

            Return Nothing
        End Function

        <ExportAPI("is.sysdir")>
        Public Function isSystemDir(dir As String) As Boolean
            If dir.IsPattern("/+") Then
                Return True
            Else
                Select Case dir.ToLower
                    Case "c:\",
                         "c:/",
                         "c:",
                         "c:\program files",
                         "c:\program files (x86)",
                         "c:\windows",
                         "c:\windows\system32",
                         "c:\windows\syswow64"
                        Return True
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
                        Return False
                End Select
            End If
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
