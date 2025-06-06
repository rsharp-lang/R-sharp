﻿#Region "Microsoft.VisualBasic::1a50abe73f9df2ba0de45ffcb43d9f1c, studio\R-terminal\Program.vb"

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

    '   Total Lines: 294
    '    Code Lines: 207 (70.41%)
    ' Comment Lines: 47 (15.99%)
    '    - Xml Docs: 40.43%
    ' 
    '   Blank Lines: 40 (13.61%)
    '     File Size: 12.16 KB


    ' Module Program
    ' 
    '     Function: InspectFile, Main, (+2 Overloads) QueryCommandLineArgvs, RunExpression, RunRScriptFile
    '               RunScript
    ' 
    ' /********************************************************************************/

#End Region

#Const DEBUG = 0

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.CommandLine
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

Module Program

    <DebuggerStepThrough>
    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(
            args:=App.CommandLine,
            executeFile:=AddressOf RunScript,
            executeEmpty:=AddressOf Terminal.RunTerminal,
            executeNotFound:=AddressOf RunExpression,
            executeQuery:=AddressOf QueryCommandLineArgvs
        )
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function QueryCommandLineArgvs(args As CommandLine) As Integer
        Return QueryCommandLineArgvs(script:=args.SingleValue, dev:=App.StdOut)
    End Function

    Friend Function QueryCommandLineArgvs(script As String, dev As TextWriter) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)

        If script.IsURLPattern Then
            ' do nothing
        ElseIf Not script.FileExists Then
            Call RInternal.debug.PrintMessageInternal(
                 RInternal.debug.stop({
                     $"the given R script file is not found on your filesystem!",
                     $"Rscript: {script}"
                 }, R.globalEnvir),
                    R.globalEnvir
            )

            Return 404
        End If

        Dim Rscript As ShellScript = REnv.Components.Rscript.AutoHandleScript(handle:=script)

        If Not Rscript.message.StringEmpty Then
            Call RInternal.debug.PrintMessageInternal(
                 RInternal.debug.stop(Rscript.message, R.globalEnvir), R.globalEnvir
            )

            Return 500
        End If

        Call Rscript.AnalysisAllCommands()
        Call Rscript.PrintUsage(dev)

        Return 0
    End Function

    ''' <summary>
    ''' all of the external package module will not loaded, only use the 
    ''' library package functions from the R# core runtime module.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Private Function RunExpression(args As CommandLine) As Integer
        If args.Name.StartsWith("--") OrElse args.Name.StartsWith("/") Then
            ' run shell with commandline options
            ' example as:
            '
            ' R# --unix --verbose --debug
            '
            Return Terminal.RunTerminal
        Else
            Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
            Dim [error] As String = Nothing
            Dim test_str = args.Name
            Dim check_filepath = test_str.isFilePath(includeWindowsFs:=True)
            Dim check_splash = test_str.Contains("/"c) OrElse test_str.Contains("\"c)
            Dim check_bracket = test_str.GetStackValue("(", ")").StringEmpty

            If check_filepath AndAlso check_splash AndAlso check_bracket Then
                ' is file path but the file is missing
                Call Message.Error($"The given script file '{args.Name.GetFullPath}'({args.Name}) is missing on your file system!", R.globalEnvir.stackFrame) _
                    .DoCall(Sub(ex)
                                Rscript.handleResult(ex, R.globalEnvir)
                            End Sub)

                Return 404
            Else
                Return CLI.runExpression(args.cli)
            End If
        End If
    End Function

    ''' <summary>
    ''' run R script file
    ''' </summary>
    ''' <param name="filepath">the file path of the Rscript file.</param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function RunScript(filepath$, args As CommandLine) As Integer
        Static helpName As Index(Of String) = {"?", "??", "--help", "--usage", "--info"}

        ' unix liked
        If args.ParameterList.Any(Function(a) Microsoft.VisualBasic.Strings.LCase(a.Name) Like helpName) OrElse
             helpName.Objects.Any(AddressOf args.HavebFlag) Then

            ' query commandline arguments
            ' show commandline help
            Return Program.QueryCommandLineArgvs(script:=filepath, dev:=App.StdOut)
        ElseIf filepath.ExtensionSuffix("csv", "json") Then
            ' andalso this app could be used as a utils for print table file
            Return InspectFile(filepath)
        Else
            ' run Rscript file
            Return Program.RunRScriptFile(filepath, args)
        End If
    End Function

    Private Function InspectFile(filepath As String) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(
            configs:=ConfigFile.localConfigs
        )
        Dim result As Object
        Dim expr As String

        Select Case filepath.ExtensionSuffix.ToLower
            Case "csv"
                Call R.LoadLibrary(
                    packageName:="utils",
                    silent:=True,
                    ignoreMissingStartupPackages:=True
                )
                expr = $"print(read.csv('{filepath}', row.names = 1, check.names = FALSE));"
            Case "json"
                Call R.Imports({"JSON"}, baseDll:="base")
                expr = $"str(JSON::json_decode(readText('{filepath}')));"
            Case Else
                Call VBDebugger.EchoLine($"The given file type(*.{filepath.ExtensionSuffix.ToLower}) '{filepath}' is not yet implemented!")
                Return 405
        End Select

        result = R.Evaluate(expr)

        Return Rscript.handleResult(result, R.globalEnvir, Nothing)
    End Function

    ''' <summary>
    ''' R# script.R ... 
    ''' 
    ''' + ``--attach`` parameter for hot load a package zip file or package dir without installed.
    ''' 
    ''' </summary>
    ''' <param name="filepath$"></param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Private Function RunRScriptFile(filepath$, args As CommandLine) As Integer
        Dim engineConfig As String = (args("--R_LIBS_USER") Or System.Environment.GetEnvironmentVariable("R_LIBS_USER"))
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(
            configs:=If(engineConfig.StringEmpty, ConfigFile.localConfigs, engineConfig)
        )
        Dim silent As Boolean = args("--silent")
        Dim verbose As Boolean = args("--verbose")
        Dim workdir As String = args("--WORKDIR") Or args.EnvironmentVariables.TryGetValue("workdir")
        Dim strict As Boolean = args.ContainsParameter("--strict") And args("--strict")
        Dim ignoreMissingStartupPackages As Boolean = args("--ignore-missing-startup-packages")
        Dim SetDllDirectory As String = args("--SetDllDirectory")

        If Not SetDllDirectory.StringEmpty Then
            Call R.globalEnvir.options.setOption("SetDllDirectory", SetDllDirectory)
        End If

        ' 显示的指定在VisualStudio中进行调试的程序包zip文件路径或者文件夹路径
        Dim attach As String = args("--attach")
        ' 设定一个库文件夹路径，里面包含有很多个程序包zip文件
        Dim pkg_attach As String = args("--pkg_attach") Or System.Environment.GetEnvironmentVariable("pkg_attach")
        Dim defaultStartups As String = args("--pkg_startup") Or System.Environment.GetEnvironmentVariable("pkg_startup")
        Dim startupsLoading As String() = If(defaultStartups.StringEmpty, R.configFile.GetStartupLoadingPackages, defaultStartups.StringSplit("\s*[,;]\s*"))

        If args.HavebFlag("--debug") OrElse args.ContainsParameter("--debug") Then
            R.debug = True
            R.globalEnvir.SetDebug(SMRUCC.Rsharp.ParseDebugLevel(args("--debug")))
        End If

        If verbose Then
            R.options(verbose:=True)
        End If
        If Not strict Then
            R.options(strict:=False)
        End If

        If Not silent AndAlso R.debug Then
            Call Console.WriteLine(args.ToString)
            Call Console.WriteLine()
        End If

        ' Call R.LoadLibrary("base")
        ' Call R.LoadLibrary("utils")
        ' Call R.LoadLibrary("grDevices")
        ' Call R.LoadLibrary("stats")
        For Each pkgName As String In startupsLoading
            Call R.LoadLibrary(
                packageName:=pkgName,
                silent:=True,
                ignoreMissingStartupPackages:=ignoreMissingStartupPackages
            )
        Next

        If Not silent Then
            Call Console.WriteLine()
        End If

        If Not attach.StringEmpty Then
            ' loading package list
            ' --attach GCModeller,mzkit,REnv
            ' --attach GCModeller.zip;mzkit.zip;/root/REnv/
            Dim packageList As String() = attach.Split(";"c, ","c)
            Dim is_pkg_dir As Boolean = packageList.TryCount = 1 AndAlso packageList(0).DirectoryExists

            If is_pkg_dir Then
                Call R.globalEnvir.options.setOption(
                    opt:=[Imports].attach_lib_dir,
                    value:=packageList(0) & "/assembly/",
                    env:=R.globalEnvir
                )
            End If

            For Each packageRef As String In packageList
                If packageRef.FileExists Then
                    Call R.attachPackageFile(zip:=packageRef, quietly:=False)
                ElseIf packageRef.DirectoryExists Then
                    Dim err As Message = PackageLoader2.Hotload(packageRef.GetDirectoryFullPath, R.globalEnvir)

                    If Not err Is Nothing Then
                        Return Rscript.handleResult(err, R.globalEnvir, Nothing)
                    End If
                Else
                    Call R.LoadLibrary(
                        packageName:=packageRef,
                        silent:=silent,
                        ignoreMissingStartupPackages:=False
                    )
                End If
            Next
        End If

        ' 20230410
        ' pkg_attach provides a repository directory which contains
        ' multiple package zip files for attach into the runtime
        ' environment
        If Not pkg_attach.StringEmpty AndAlso pkg_attach.DirectoryExists Then
            Call Console.WriteLine($"load required packages from alternative repository: '{pkg_attach.GetDirectoryFullPath}'...")

            For Each packageFile As String In pkg_attach.ListFiles("*.zip")
                Call Console.WriteLine($" ...{packageFile.FileName}")
                Call R.attachPackageFile(zip:=packageFile, quietly:=False)
            Next
        End If

        If Not workdir.StringEmpty Then
            App.CurrentDirectory = workdir.TranslateWorkdir(filepath.GetFullPath)
        End If

        Dim result As Object

        If Not filepath.ExtensionSuffix("R") Then
            If R.globalEnvir.polyglot.CanHandle(filepath) Then
                result = R _
                    .globalEnvir _
                    .polyglot _
                    .LoadScript(filepath, R.globalEnvir)
            Else
                result = RInternal.debug.stop({$"unsupported script file type(*.{filepath.ExtensionSuffix})!"}, R.globalEnvir)
            End If
        Else
            result = R.Source(filepath)
        End If

        Return Rscript.handleResult(result, R.globalEnvir, Nothing)
    End Function
End Module
