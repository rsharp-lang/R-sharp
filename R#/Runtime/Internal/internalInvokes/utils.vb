#Region "Microsoft.VisualBasic::6e2700760c0621cb30f41dd6a9959bfe, R#\Runtime\Internal\internalInvokes\utils.vb"

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
    '         Function: debugTool, GetInstalledPackages, head, installPackages, keyGroups
    '                   md5, memorySize, system, wget
    ' 
    '         Sub: cls, sleep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Package
Imports RPkg = SMRUCC.Rsharp.System.Package.Package
Imports REnv = SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Components
Imports System.IO
Imports Microsoft.VisualBasic.SecurityString
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

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
        ''' http/ftp file download
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="save"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("wget")>
        Public Function wget(url As String, Optional save As String = Nothing, Optional env As Environment = Nothing) As Object
            If url.StringEmpty Then
                Return Internal.debug.stop({"Missing url data source for file get!"}, env)
            ElseIf save.StringEmpty Then
                save = App.CurrentDirectory & "/" & url.Split("?"c).First.BaseName.NormalizePathString(False)
            End If

            Return Http.wget.Download(url, save)
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
                    Dim data As Array = Array.CreateInstance(v.elementType.raw, n)

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
                Dim df As dataframe = DirectCast(x, dataframe)

                If df.nrows <= n Then
                    Return df
                Else
                    Dim data As New Dictionary(Of String, Array)
                    Dim colVal As Array
                    Dim colSubset As Array

                    For Each col In df.columns
                        If col.Value.Length = 1 Then
                            data.Add(col.Key, col.Value)
                        Else
                            colVal = col.Value
                            colSubset = Array.CreateInstance(colVal.GetType.GetElementType, n)

                            For i As Integer = 0 To n - 1
                                colSubset.SetValue(colVal.GetValue(i), i)
                            Next

                            data.Add(col.Key, colSubset)
                        End If
                    Next

                    Return New dataframe With {
                        .columns = data,
                        .rownames = df.rownames _
                            .SafeQuery _
                            .Take(n) _
                            .ToArray
                    }
                End If
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
                               Optional input As Object = Nothing,
                               Optional show_output_on_console As Boolean = True,
                               Optional minimized As Boolean = False,
                               Optional invisible As Boolean = True,
                               Optional timeout As Double = 0) As Integer
            Throw New NotImplementedException
        End Function

        <ExportAPI("md5")>
        Public Function md5(<RRawVectorArgument> data As Object, Optional env As Environment = Nothing) As Object
            If data Is Nothing Then
                Return Nothing
            End If

            If TypeOf data Is String Then
                Return DirectCast(data, String).MD5
            ElseIf TypeOf data Is String() Then
                Return DirectCast(data, String()) _
                    .Select(Function(str) str.MD5) _
                    .ToArray
            ElseIf TypeOf data Is vector Then
                Return REnv.asVector(Of String)(DirectCast(data, vector).data) _
                    .AsObjectEnumerator(Of String) _
                    .Select(Function(str) str.MD5) _
                    .ToArray
            Else
                data = RConversion.asRaw(data, ,, env)

                If Not TypeOf data Is Message Then
                    Return data
                Else
                    Using buffer As MemoryStream = DirectCast(data, MemoryStream), md5hash As New Md5HashProvider
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
    End Module
End Namespace
