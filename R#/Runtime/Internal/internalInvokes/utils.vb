#Region "Microsoft.VisualBasic::4f51c89973f41d2cc2f8079f4bf7ae9d, R#\Runtime\Internal\internalInvokes\utils.vb"

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

'     Module utils
' 
'         Function: createAlternativeName, createCommandLine, data, dataSearchByPackageDir, debugTool
'                   description, FindSystemFile, GetInstalledPackages, head, installPackages
'                   keyGroups, md5, memorySize, now, readFile
'                   system, systemFile, wget, workdir
' 
'         Sub: cls, pause, sleep
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Parsers
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.SecurityString
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Linq
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RPkg = SMRUCC.Rsharp.Development.Package.Package

Namespace Runtime.Internal.Invokes

    Module utils

        ''' <summary>
        ''' # Install Packages from Repositories or Local Files
        ''' 
        ''' Download and install packages from CRAN-like repositories or from local files.
        ''' 
        ''' This is the main function to install packages. It takes a vector of names and 
        ''' a destination library, downloads the packages from the repositories and installs 
        ''' them. (If the library is omitted it defaults to the first directory in 
        ''' .libPaths(), with a message if there is more than one.) If lib is omitted or 
        ''' is of length one and is not a (group) writable directory, in interactive use 
        ''' the code offers to create a personal library tree (the first element of 
        ''' Sys.getenv("R_LIBS_USER")) and install there. Detection of a writable 
        ''' directory is problematic on Windows: see the ‘Note’ section.
        '''
        ''' For installs from a repository an attempt Is made To install the packages In 
        ''' an order that respects their dependencies. This does assume that all the 
        ''' entries In Lib are On the Default library path For installs (Set by 
        ''' environment variable R_LIBS).
        '''
        ''' You are advised To run update.packages before install.packages To ensure that 
        ''' any already installed dependencies have their latest versions.
        ''' </summary>
        ''' <param name="packages">The dll file name, character vector of the names of 
        ''' packages whose current versions should be downloaded from the repositories.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' install.packages tries to detect if you have write permission on the library 
        ''' directories specified, but Windows reports unreliably. If there is only one 
        ''' library directory (the default), R tries to find out by creating a test directory, 
        ''' but even this need not be the whole story: you may have permission to write 
        ''' in a library directory but lack permission to write binary files (such as ‘.dll’ 
        ''' files) there. See the ‘R for Windows FAQ’ for workarounds.
        ''' </remarks>
        <ExportAPI("install.packages")>
        Public Function installPackages(packages$(), Optional envir As Environment = Nothing) As Object
            Dim pkgMgr As PackageManager = envir.globalEnvironment.packages
            Dim namespaces As New List(Of String)

            For Each pkgName As String In packages.SafeQuery
                If pkgName.FileExists Then
                    namespaces += pkgMgr.InstallLocals(pkgName)
                Else
                    Return Internal.debug.stop($"Library module '{pkgName.GetFullPath}' is not exists on your file system!", envir)
                End If
            Next

            Call pkgMgr.Flush()

            Return namespaces.ToArray
        End Function

        ''' <summary>
        ''' ## Find Installed Packages
        ''' 
        ''' Find (or retrieve) details of all packages installed in the specified libraries.
        ''' 
        ''' ``installed.packages`` scans the ‘DESCRIPTION’ files of each package found along 
        ''' ``lib.loc`` and returns a matrix of package names, library paths and version numbers.
        '''
        ''' The information found Is cached (by library) For the R session And specified fields argument, 
        ''' And updated only If the top-level library directory has been altered, 
        ''' For example by installing Or removing a package. If the cached information becomes confused, 
        ''' it can be refreshed by running ``installed.packages(noCache = True)``.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("installed.packages")>
        Public Function GetInstalledPackages(Optional groupBy$ = "none|LibPath|Author|Category", Optional envir As Environment = Nothing) As Object
            Dim pkgMgr As PackageManager = envir.globalEnvironment.packages
            Dim packages As RPkg() = pkgMgr _
                .AsEnumerable _
.OrderBy(Function(pkg) pkg.namespace) _
.ToArray
            Dim Package As Array = packages.Select(Function(pkg) pkg.namespace).ToArray
            Dim LibPath As Array = packages.Select(Function(pkg) If(pkg.isMissing, "<missing>", pkg.libPath.FileName)).ToArray
            Dim Version As Array = packages.Select(Function(pkg) pkg.info.Revision).ToArray
            Dim Author As Array = packages.Select(Function(pkg) pkg.info.Publisher.LineTokens.DefaultFirst("n/a")).ToArray
            Dim Category As Array = packages.Select(Function(pkg) pkg.info.Category.ToString).ToArray
            Dim Built As Array = packages _
                .Select(Function(pkg)
                            Return If(pkg.isMissing, "<Unknown>", pkg.GetPackageModuleInfo.BuiltTime.ToString)
                        End Function) _
                .ToArray
            Dim Description As Array = packages _
                .Select(Function(pkg)
                            Return pkg.GetPackageDescription(envir) _
.LineTokens _
.DefaultFirst("n/a") _
.TrimStart("#"c, " "c)
                        End Function) _
                .ToArray
            Dim summary As New dataframe With {
                .rownames = packages.Select(Function(pkg) pkg.namespace).ToArray,
                .columns = New Dictionary(Of String, Array) From {
                    {NameOf(Package), Package},
                    {NameOf(LibPath), LibPath},
                    {NameOf(Version), Version},
                    {NameOf(Built), Built},
                    {NameOf(Author), Author},
                    {NameOf(Description), Description},
                    {NameOf(Category), Category}
                }
            }

            If Not groupBy.StringEmpty Then
                Dim groupIndex As Dictionary(Of String, Integer())

                Select Case groupBy.Split("|"c).First.ToLower
                    Case "libpath"
                        groupIndex = DirectCast(LibPath, String()).keyGroups
                    Case "author"
                        groupIndex = DirectCast(Author, String()).keyGroups
                    Case "category"
                        groupIndex = DirectCast(Category, String()).keyGroups
                    Case Else
                        Return summary
                End Select

                Return New list With {
                    .slots = groupIndex _
                        .ToDictionary(Function(group) group.Key,
                                      Function(partition)
                                          Return CObj(summary.GetByRowIndex(partition.Value))
                                      End Function)
                }
            Else
                Return summary
            End If
        End Function

        <Extension>
        Private Function keyGroups(keys As String()) As Dictionary(Of String, Integer())
            Dim uniques As String() = keys.Distinct.ToArray

            Return uniques _
                .ToDictionary(Function(key) key,
                              Function(key)
                                  Return keys.SeqIterator _
                                      .Where(Function(name) name.value = key) _
                                      .Select(Function(index) index.i) _
.ToArray
                              End Function)
        End Function

        ''' <summary>
        ''' retrieving files using HTTP, HTTPS, FTP and FTPS, the most widely used Internet protocols.
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="save">the function will returns a stream object if this parameter is nothing</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("wget")>
        Public Function wget(url As String, Optional save As String = Nothing, Optional env As Environment = Nothing) As Object
            If url.StringEmpty Then
                Return Internal.debug.stop({"Missing url data source for file get!"}, env)
            ElseIf save.StringEmpty Then
                Dim buffer As New MemoryStream

                Http.wget.Download(url, buffer)
                buffer.Flush()

                Return buffer
            Else
                Return Http.wget.Download(url, save)
            End If
        End Function

        ''' <summary>
        ''' Clears the console buffer and corresponding console window of display information.
        ''' </summary>
        <ExportAPI("clear")>
        Public Sub cls()
            Call Console.Clear()
        End Sub

        ''' <summary>
        ''' Suspends the current thread for the specified number of seconds.
        ''' </summary>
        ''' <param name="sec"></param>
        <ExportAPI("sleep")>
        Public Sub sleep(sec As Integer)
            Call Thread.Sleep(sec * 1000)
        End Sub

        ''' <summary>
        ''' Return the First or Last Part of an Object
        ''' 
        ''' Returns the first or last parts of a vector, matrix, table, data frame or function. 
        ''' Since head() and tail() are generic functions, they may also have been extended 
        ''' to other classes.
        ''' </summary>
        ''' <param name="x">an object</param>
        ''' <param name="n">
        ''' a single integer. If positive or zero, size for the resulting object: number of 
        ''' elements for a vector (including lists), rows for a matrix or data frame or lines 
        ''' for a function. If negative, all but the n last/first number of elements of x.
        ''' </param>
        ''' <param name="env"></param>
        ''' <remarks>
        ''' For matrices, 2-dim tables and data frames, head() (tail()) returns the first (last) 
        ''' n rows when n >= 0 or all but the last (first) n rows when n &lt; 0. head.matrix() 
        ''' and tail.matrix() are exported. For functions, the lines of the deparsed function 
        ''' are returned as character strings.
        '''
        ''' If a matrix has no row names, Then tail() will add row names Of the form "[n,]" 
        ''' To the result, so that it looks similar To the last lines Of x When printed. 
        ''' Setting addrownums = False suppresses this behaviour.
        ''' </remarks>
        ''' <returns>An object (usually) like x but generally smaller. For ftable objects x, 
        ''' a transformed format(x).</returns>
        <ExportAPI("head")>
        Public Function head(<RRawVectorArgument> x As Object, Optional n% = 6, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return x
            ElseIf x.GetType.IsArray Then
                x = New vector With {.data = DirectCast(x, Array)}
            ElseIf x.GetType.ImplementInterface(GetType(IDictionary(Of String, Object))) Then
                x = New list With {.slots = DirectCast(x, Dictionary(Of String, Object))}
            End If

            Dim type As Type = x.GetType

            If type Is GetType(vector) Then
                Dim v As vector = DirectCast(x, vector)

                If v.length <= n Then
                    Return x
                Else
                    Dim data As Array

                    If v.elementType Is Nothing Then
                        data = New Object(n - 1) {}
                    Else
                        data = Array.CreateInstance(v.elementType.raw, n)
                    End If

                    For i As Integer = 0 To data.Length - 1
                        data.SetValue(v.data.GetValue(i), i)
                    Next

                    Return New vector With {.data = data}
                End If
            ElseIf type Is GetType(list) Then
                Dim l As list = DirectCast(x, list)

                If l.length <= n Then
                    Return l
                Else
                    Return New list With {
                        .slots = l.slots.Keys _
                            .Take(n) _
                            .ToDictionary(Function(key) key,
                                          Function(key)
                                              Return l.slots(key)
                                          End Function)
                    }
                End If
            ElseIf type Is GetType(dataframe) Then
                Return DirectCast(x, dataframe).head(n)
            Else
                Return x
            End If
        End Function

        ''' <summary>
        ''' ### Report on Memory Allocation
        ''' 
        ''' ``memory.size`` reports the current or maximum memory allocation 
        ''' of the malloc function used in this version of R.
        ''' </summary>
        ''' <param name="max">	
        ''' logical. If TRUE the maximum amount of memory obtained from the OS 
        ''' Is reported, if FALSE the amount currently in use, if NA the memory 
        ''' limit.
        ''' </param>
        ''' <returns>
        ''' Size in Mb (1048576 bytes), rounded to 0.01 Mb for memory.size
        ''' </returns>
        <ExportAPI("memory.size")>
        <RApiReturn(GetType(Double))>
        Public Function memorySize(Optional max As Boolean = False) As Object
            Dim Rsharp As Process = Process.GetCurrentProcess()
            Dim memSize As Double = Rsharp.WorkingSet64 / 1024 / 1024

            Return vector.asVector({memSize}, New unit With {.name = "MB"})
        End Function

        ''' <summary>
        ''' create commandline string
        ''' </summary>
        ''' <param name="argv"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("commandline")>
        Public Function createCommandLine(<RListObjectArgument> argv As list, Optional env As Environment = Nothing) As String
            If argv.length = 1 Then
                Return REnv _
                    .asVector(Of Object)(argv.slots.First.Value) _
                    .AsObjectEnumerator _
                    .Select(AddressOf any.ToString) _
                    .Select(Function(str) str.CLIToken) _
                    .JoinBy(" ")
            Else
                Return argv.slots _
                    .Values _
                    .Select(Function(a)
                                Return any.ToString(REnv.single(a)).CLIToken
                            End Function) _
                    .JoinBy(" ")
            End If
        End Function

        ''' <summary>
        ''' ### Invoke a System Command
        ''' 
        ''' ``system`` invokes the OS command specified by ``command``.
        ''' </summary>
        ''' <param name="command">the system command to be invoked, as a character string.</param>
        ''' <param name="intern">a logical (not NA) which indicates whether to capture the 
        ''' output of the command as an R character vector.</param>
        ''' <param name="ignore_stdout"></param>
        ''' <param name="ignore_stderr"></param>
        ''' <param name="wait"></param>
        ''' <param name="input"></param>
        ''' <param name="show_output_on_console">
        ''' logical (Not NA), indicates whether to capture the output of the command And show 
        ''' it on the R console (Not used by Rterm, which shows the output in the terminal 
        ''' unless wait Is false).
        ''' </param>
        ''' <param name="minimized">
        ''' logical (Not NA), indicates whether a command window should be displayed initially 
        ''' as a minimized window.
        ''' </param>
        ''' <param name="invisible"></param>
        ''' <param name="timeout"></param>
        ''' <remarks>
        ''' This interface has become rather complicated over the years: see system2 for a more 
        ''' portable and flexible interface which is recommended for new code.
        '''
        ''' command Is parsed as a command plus arguments separated by spaces. So if the path to 
        ''' the command (Or a single argument such as a file path) contains spaces, it must be 
        ''' quoted e.g. by shQuote. Only double quotes are allowed on Windows: see the examples. 
        ''' (Note: a Windows path name cannot contain a Double quote, so we Do Not need To worry 
        ''' about escaping embedded quotes.)
        '''
        ''' command must be an executable (extensions '.exe’, ‘.com’) or a batch file (extensions 
        ''' ‘.cmd’ and ‘.bat’): these extensions are tried in turn if none is supplied. This 
        ''' means that redirection, pipes, DOS internal commands, ... cannot be used: see shell 
        ''' if you want to pass a shell command-line.
        '''
        ''' The search path For command may be system-dependent: it will include the R 'bin’ 
        ''' directory, the working directory and the Windows system directories before PATH.
        '''
        ''' When timeout Is non-zero, the command Is terminated after the given number of seconds. 
        ''' The termination works for typical commands, but Is Not guaranteed it Is possible to 
        ''' write a program that would keep running after the time Is out. Timeouts can only be 
        ''' set with wait = TRUE.
        '''
        ''' The ordering Of arguments after the first two has changed from time To time: it Is 
        ''' recommended to name all arguments after the first.
        '''
        ''' There are many pitfalls In Using system To ascertain If a command can be run — 
        ''' Sys.which Is more suitable.
        ''' </remarks>
        ''' <returns>
        ''' If intern = TRUE, a character vector giving the output of the command, one line per 
        ''' character string. (Output lines of more than 8095 bytes will be split.) If the command 
        ''' could not be run an R error is generated. Under the Rgui console intern = TRUE also 
        ''' captures stderr unless ignore.stderr = TRUE. If command runs but gives a non-zero exit 
        ''' status this will be reported with a warning and in the attribute "status" of the result: 
        ''' an attribute "errmsg" may also be available.
        '''
        ''' If intern = False, the Return value Is an Error code (0 For success), given the invisible 
        ''' attribute (so needs To be printed explicitly). If the command could Not be run For any 
        ''' reason, the value Is 127 And a warning Is issued (As from R 3.5.0). Otherwise If 
        ''' wait = True the value Is the Exit status returned by the command, And If wait = False it 
        ''' Is 0 (the conventional success value).
        '''
        ''' If the command times out, a warning Is reported And the Exit status Is 124. Some Windows 
        ''' commands Return out-Of-range status values (e.g., -1) And so only the bottom 16 bits Of 
        ''' the value are used.
        '''
        ''' If intern = False, wait = True, show.output.On.console = True the 'stdout’ and ‘stderr’ 
        ''' (unless ignore.stdout = TRUE or ignore.stderr = TRUE) output from a command that is a 
        ''' ‘console application’ should appear in the R console (Rgui) or the window running R 
        ''' (Rterm).
        '''
        ''' Not all Windows executables properly respect redirection of output, Or may only do so 
        ''' from a console application such as Rterm And Not from Rgui For example, 'fc.exe’ was 
        ''' among these in the past, but we have had more success recently.
        ''' </returns>
        <ExportAPI("system")>
        Public Function system(command$,
                               Optional intern As Boolean = False,
                               Optional ignore_stdout As Boolean = False,
                               Optional ignore_stderr As Boolean = False,
                               Optional wait As Boolean = True,
                               <RRawVectorArgument>
                               Optional input As Object = Nothing,
                               Optional show_output_on_console As Boolean = True,
                               Optional minimized As Boolean = False,
                               Optional invisible As Boolean = True,
                               Optional timeout As Double = 0,
                               Optional clr As Boolean = False,
                               Optional env As Environment = Nothing) As String

            Dim tokens As String() = command _
                .Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF) _
                .LineTokens _
                .JoinBy(" ") _
                .DoCall(AddressOf CLIParser.GetTokens)
            Dim executative As String = tokens(Scan0)
            Dim arguments As String = tokens _
                .Skip(1) _
                .Select(Function(str) str.CLIToken) _
                .JoinBy(" ")
            Dim inputStr As String() = REnv.asVector(Of Object)(input) _
                .AsObjectEnumerator _
                .Select(AddressOf any.ToString) _
                .ToArray
            Dim std_out As String

            If env.globalEnvironment.debugMode Then
                Call base.print("get app executative:", , env)
                Call base.print(executative,, env)
                Call base.print("commandline argument is:",, env)
                Call base.print(arguments,, env)
            End If

            If Global.System.Environment.OSVersion.Platform = Global.System.PlatformID.Win32NT Then
                If env.globalEnvironment.debugMode Then
                    Call base.print($"run on windows {If(clr, ".NET/CLR", "naive")} environment!", , env)
                End If

                If clr Then
                    Dim ps = App.Shell(executative, arguments, CLR:=clr, debug:=True, stdin:=inputStr.JoinBy(vbLf))

                    ps.Run()
                    std_out = ps.StandardOutput

                    If show_output_on_console Then
                        Call Console.WriteLine(ps.StandardOutput)
                    End If
                Else
                    std_out = PipelineProcess.Call(executative, arguments, inputStr.JoinBy(vbLf))

                    If show_output_on_console Then
                        Call Console.WriteLine(StdOut)
                    End If
                End If
            ElseIf clr Then
                If env.globalEnvironment.debugMode Then
                    Call base.print("run on UNIX mono!", , env)
                End If

                std_out = UNIX.Shell("mono", $"{executative.CLIPath} {arguments}", verbose:=show_output_on_console, stdin:=inputStr.JoinBy(vbLf))
            Else
                If env.globalEnvironment.debugMode Then
                    Call base.print("run a UNIX program.",, env)
                End If

                std_out = PipelineProcess.Call(executative, arguments, [in]:=inputStr.JoinBy(vbLf))
            End If

            Return std_out
        End Function

        <ExportAPI("workdir")>
        Public Function workdir(dir As String) As TemporaryEnvironment
            Return New TemporaryEnvironment(newLocation:=dir)
        End Function

        ''' <summary>
        ''' ## Vectorized hash/hmac functions
        ''' </summary>
        ''' <param name="x">
        ''' character vector, raw vector or connection object.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Digest types: https://www.openssl.org/docs/man1.1.1/man1/openssl-dgst.html
        ''' </remarks>
        <ExportAPI("md5")>
        Public Function md5(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return Nothing
            ElseIf TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If

            If TypeOf x Is String Then
                Return DirectCast(x, String).MD5
            ElseIf TypeOf x Is String() Then
                Return DirectCast(x, String()) _
                    .Select(Function(str) str.MD5) _
                    .ToArray
            ElseIf TypeOf x Is Double() Then
                If DirectCast(x, Array).Length = 1 Then
                    Using buffer As New MemoryStream(BitConverter.GetBytes(DirectCast(x, Double())(Scan0))), md5hash As New Md5HashProvider
                        Return md5hash.GetMd5Hash(buffer.ToArray)
                    End Using
                Else
                    Return Message.InCompatibleType(GetType(Byte), x.GetType, env)
                End If
            ElseIf x.GetType.IsArray Then
                Return REnv.asVector(Of String)(DirectCast(x, vector).data) _
                    .AsObjectEnumerator(Of String) _
                    .Select(Function(str) str.MD5) _
                    .ToArray
            Else
                x = RConversion.asRaw(x, ,, env)

                If TypeOf x Is Message Then
                    Return x
                Else
                    Using buffer As MemoryStream = DirectCast(x, MemoryStream), md5hash As New Md5HashProvider
                        Return md5hash.GetMd5Hash(buffer.ToArray)
                    End Using
                End If
            End If
        End Function

        ''' <summary>
        ''' ### Debug an Expression
        ''' 
        ''' apply for debug in Visual Studio, like a script breakpoint
        ''' </summary>
        ''' <param name="expr">any interpreted R expression.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("debug")>
        Public Function debugTool(expr As Expression, env As Environment) As Object
            Dim caller As Environment = env.parent.parent
            Dim result As Object = expr.Evaluate(caller)
            Return result
        End Function

        ''' <summary>
        ''' Pause the console program.
        ''' </summary>
        <ExportAPI("pause")>
        Public Sub pause()
            Call App.Pause()
        End Sub

        ''' <summary>
        ''' ### Data Sets
        ''' 
        ''' Loads specified data sets, or list the available data sets.
        ''' </summary>
        ''' <param name="name">literal character strings Or names.</param>
        ''' <param name="package">
        ''' a character vector giving the package(s) to look in for data sets, or NULL.
        ''' By Default, all packages in the search path are used, then the 'data’ 
        ''' subdirectory (if present) of the current working directory.
        ''' </param>
        ''' <param name="lib_loc">
        ''' a character vector of directory names of R libraries, or NULL. 
        ''' The default value of NULL corresponds to all libraries currently 
        ''' known.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Currently, four formats of data files are supported:
        ''' 
        '''    1. files ending ‘.R’ or ‘.r’ are source()d in, with the R working directory changed 
        '''       temporarily to the directory containing the respective file. (data ensures that the 
        '''       utils package is attached, in case it had been run via utils::data.)
        '''    2. files ending ‘.RData’ or ‘.rda’ are load()ed.
        '''    3. files ending ‘.tab’, ‘.txt’ or ‘.TXT’ are read using read.table(..., header = TRUE, as.is=FALSE), 
        '''       and hence result in a data frame.
        '''    4. files ending ‘.csv’ or ‘.CSV’ are read using read.table(..., header = TRUE, sep = ";", as.is=FALSE), 
        '''       and also result in a data frame.
        '''       
        ''' If more than one matching file name is found, the first on this list is used. (Files with 
        ''' extensions ‘.txt’, ‘.tab’ or ‘.csv’ can be compressed, with or without further 
        ''' extension ‘.gz’, ‘.bz2’ or ‘.xz’.)
        ''' 
        ''' The data sets to be loaded can be specified as a set of character strings or names, or as 
        ''' the character vector list, or as both.
        ''' 
        ''' For each given data set, the first two types (‘.R’ or ‘.r’, and ‘.RData’ or ‘.rda’ 
        ''' files) can create several variables in the load environment, which might all be named differently 
        ''' from the data set. The third and fourth types will always result in the creation of a single 
        ''' variable with the same name (without extension) as the data set.
        ''' 
        ''' If no data sets are specified, data lists the available data sets. It looks for a new-style 
        ''' data index in the ‘Meta’ or, if this is not found, an old-style ‘00Index’ file in the ‘data’ 
        ''' directory of each specified package, and uses these files to prepare a listing. If there is a 
        ''' ‘data’ area but no index, available data files for loading are computed and included in the 
        ''' listing, and a warning is given: such packages are incomplete. The information about available 
        ''' data sets is returned in an object of class "packageIQR". The structure of this class is experimental. 
        ''' Where the datasets have a different name from the argument that should be used to retrieve them 
        ''' the index will have an entry like beaver1 (beavers) which tells us that dataset beaver1 can be 
        ''' retrieved by the call data(beaver).
        ''' 
        ''' If lib.loc and package are both NULL (the default), the data sets are searched for in all the 
        ''' currently loaded packages then in the ‘data’ directory (if any) of the current working 
        ''' directory.
        ''' 
        ''' If lib.loc = NULL but package is specified as a character vector, the specified package(s) are 
        ''' searched for first amongst loaded packages and then in the default library/ies (see .libPaths).
        ''' 
        ''' If lib.loc is specified (and not NULL), packages are searched for in the specified library/ies, 
        ''' even if they are already loaded from another library.
        ''' 
        ''' To just look in the ‘data’ directory of the current working directory, set package = character(0) 
        ''' (and lib.loc = NULL, the default).
        ''' </remarks>
        <ExportAPI("data")>
        Public Function data(name As String,
                             Optional package As String() = Nothing,
                             Optional lib_loc$ = Nothing,
                             Optional env As Environment = Nothing) As Object

            Dim hit As Boolean = False
            Dim err As New Value(Of Message)

            If lib_loc.StringEmpty Then
                lib_loc = env.globalEnvironment.options.lib_loc
            End If

            If package.IsNullOrEmpty Then
                package = env.globalEnvironment _
                    .attachedNamespace _
                    .packageNames
            End If

            For Each pkgFile As String In package.Select(Function(pkgName) $"{lib_loc}/{pkgName}")
                If Not (err = env.dataSearchByPackageDir(name, pkgFile, hit)) Is Nothing Then
                    Return err.Value
                ElseIf hit Then
                    Return Nothing
                End If
            Next

            ' 如果是attatch的程序包，则可能会在程序包库文件夹中搜索不到
            ' 直接使用程序包之中的路径文件夹
            Dim attached = env.globalEnvironment.attachedNamespace

            For Each pkgNs As NamespaceEnvironment In package _
                .Where(Function(ns) attached.hasNamespace(ns)) _
                .Select(Function(ns)
                            Return attached(ns)
                        End Function)

                If Not (err = env.dataSearchByPackageDir(name, pkgNs.libpath, hit)) Is Nothing Then
                    Return err.Value
                ElseIf hit Then
                    Return Nothing
                End If
            Next

            Return Internal.debug.stop({
                 $"no dataset exists which is named '{name}'.",
                 $"dataset: {name}"
            }, env)
        End Function

        <Extension>
        Private Function dataSearchByPackageDir(env As Environment,
                                                name As String,
                                                pkgFile As String,
                                                ByRef hit As Boolean) As Message

            Dim dataSymbols = $"{pkgFile}/manifest/data.json".LoadJsonFile(Of Dictionary(Of String, NamedValue))
            Dim reader As String
            Dim load As Object

            hit = False

            If dataSymbols.IsNullOrEmpty OrElse Not dataSymbols.ContainsKey(name) Then
                Return Nothing
            Else
                reader = dataSymbols(name).text
                pkgFile = $"{pkgFile}/data/{dataSymbols(name).name}"
                load = env.readFile(reader, pkgFile)
            End If

            If Program.isException(load) Then
                Return load
            Else
                hit = True

                Return TryCast(env _
                    .globalEnvironment _
                    .Push(
                        name:=name,
                        value:=load,
                        [readonly]:=False
                    ), Message)
            End If
        End Function

        <Extension>
        Private Function readFile(env As Environment, reader As String, file$) As Object
            Dim tokens As String() = reader.Split(","c)
            Dim args As New List(Of Object)

            reader = tokens(Scan0)

            For Each item In tokens.Skip(1)
                If item = "%s" Then
                    args.Add(file)
                ElseIf item = "$" Then
                    args.Add(env.globalEnvironment)
                ElseIf item = "NULL" Then
                    args.Add(Nothing)
                ElseIf item = "TRUE" OrElse item = "FALSE" Then
                    args.Add(item.ParseBoolean)
                ElseIf item.Last = "%" Then
                    args.Add(item.Replace("%", "").DoCall(AddressOf Integer.Parse))
                Else
                    args.Add(item)
                End If
            Next

            Return env.globalEnvironment.Rscript.Invoke(reader, args.ToArray)
        End Function

        <Extension>
        Private Function createAlternativeName(fileName As String) As String
            Dim list = fileName.Split("\"c, "/"c)
            Dim basename = fileName.BaseName
            Dim alter = $"{list.Take(list.Length - 1).JoinBy("/")}/{basename}"

            Return alter
        End Function

        ''' <summary>
        ''' ### Find Names of R System Files
        ''' 
        ''' Finds the full file names of files in packages etc.
        ''' </summary>
        ''' <param name="fileName"></param>
        ''' <param name="package">a character String With the name Of a Single package.
        ''' An Error occurs If more than one package name Is given.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' do not add extension suffix name for csv data set due to 
        ''' the reason of csv file extension suffix will be removed
        ''' automatically in the progress of R compile the data file 
        ''' into package file.
        ''' </remarks>
        <ExportAPI("system.file")>
        Public Function systemFile(fileName As String,
                                   Optional package$ = Nothing,
                                   Optional mustWork As Boolean = False,
                                   Optional env As Environment = Nothing) As Object

            If Not package.StringEmpty Then
                Dim pkgDir As String
                Dim alternativeName As String = fileName.createAlternativeName

                ' 优先从已经加载的程序包位置进行加载操作
                If env.globalEnvironment.attachedNamespace.hasNamespace(package) Then
                    pkgDir = env.globalEnvironment.attachedNamespace(package).libpath
                ElseIf Not RFileSystem.PackageInstalled(package, env) Then
                    Return Internal.debug.stop({$"we could not found any installed package which is named '{package}'!", $"package: {package}"}, env)
                Else
                    pkgDir = $"{RFileSystem.GetPackageDir(env)}/{package}"
                End If

                fileName = $"{pkgDir}/{fileName}".GetFullPath

                If fileName.FileExists Then
                    Return fileName.GetFullPath
                ElseIf $"{pkgDir}/{alternativeName}".FileExists Then
                    Call env.AddMessage($"Target file '{fileName}' is missing in R file system. Use alternative file name: '{pkgDir}/{alternativeName}'...")
                    Return $"{pkgDir}/{alternativeName}"
                ElseIf mustWork Then
                    Return Internal.debug.stop("file is not found!", env)
                Else
                    Call env.AddMessage($"target file '{fileName}' is missing in R file system.")
                    Return Nothing
                End If
            Else
                Return Internal.debug.stop("not implemented!", env)
            End If
        End Function

        ''' <summary>
        ''' loading a ``DESCRIPTION`` file
        ''' </summary>
        ''' <param name="package">
        ''' loading current package if the parameter is nothing
        ''' </param>
        ''' <returns>
        ''' a list object that contains the meta data of the 
        ''' package descirption information.
        ''' </returns>
        <ExportAPI("description")>
        Public Function description(Optional package As String = Nothing,
                                    Optional ignoreMissingPkg As Boolean = True,
                                    Optional env As Environment = Nothing) As Object

            Dim globalEnv As GlobalEnvironment = env.globalEnvironment

            If package.StringEmpty Then
                package = env.parent.stackFrame.Method.Namespace
            End If

            If Not globalEnv.packages.hasLibPackage(package) Then
                If ignoreMissingPkg Then
                    env.AddMessage($"the required package '{package}' is not yet installed!")
                    Return Nothing
                Else
                    Return Internal.debug.stop({$"the required package '{package}' is not yet installed!", $"package: {package}"}, env)
                End If
            Else
                Return ($"{globalEnv.packages.getPackageDir(package)}/index.json") _
                    .LoadJsonFile(Of DESCRIPTION) _
                    .toList
            End If
        End Function

        <Extension>
        Private Function FindSystemFile(env As GlobalEnvironment, fileName As String) As String
            Dim file As Value(Of String) = ""
            Dim findFileByName As Func(Of String, String) =
                Function(dir As String) As String
                    Dim ls As String() = dir.ListFiles("*").ToArray

                    For Each filepath As String In ls
                        If filepath.FileName = fileName Then
                            Return filepath
                        End If
                    Next

                    Return Nothing
                End Function
            Dim packageDir As String = RFileSystem.GetPackageDir(env)

            ' 当搜索失败的时候才会在已经安装的程序列表之中进行搜索
            For Each dir As String In packageDir.ListDirectory
                If Not (file = findFileByName(dir)).StringEmpty Then
                    Return file
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' get current system time
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("now")>
        Public Function now() As Date
            Return Date.Now
        End Function
    End Module
End Namespace
