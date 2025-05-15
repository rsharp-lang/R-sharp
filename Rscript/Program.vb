﻿#Region "Microsoft.VisualBasic::0902f21000be350c4d7ef243b3002d2a, Rscript\Program.vb"

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

    '   Total Lines: 221
    '    Code Lines: 155 (70.14%)
    ' Comment Lines: 33 (14.93%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 33 (14.93%)
    '     File Size: 8.85 KB


    ' Module Program
    ' 
    '     Function: Main, Run, RunRscriptFile, RunScript
    ' 
    '     Sub: LoadLibrary
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports Libdir = Microsoft.VisualBasic.FileIO.Directory
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program
Imports RscriptText = SMRUCC.Rsharp.Runtime.Components.Rscript

''' <summary>
''' 
''' </summary>
Module Program

    ''' <summary>
    ''' 1. accept a R script file path
    ''' 2. accept R script text from standard input.
    ''' </summary>
    ''' <returns></returns>
    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine, executeFile:=AddressOf RunRscriptFile, executeEmpty:=AddressOf Run)
    End Function

    ''' <summary>
    ''' Run R script from std_input 
    ''' </summary>
    ''' <returns></returns>
    Private Function Run() As Integer
        Using stdin As StreamReader = App.StdInput
            Dim script As New StringBuilder
            Dim line As Value(Of String) = ""
            Dim check As Boolean = False
            Dim determineEndOfStream As New Task(Sub() check = stdin.EndOfStream)

            Try
                determineEndOfStream.TimeoutAfter(100).Wait()
                check = True
            Catch ex As Exception
                ' no stdinput
                ' just display help
                Return GetType(CLI).RunCLI(App.CommandLine)
            End Try

            Do While Not stdin.EndOfStream
                If (line = stdin.ReadLine) Is Nothing Then
                    Exit Do
                Else
                    Call script.AppendLine(line)
                End If
            Loop

            Return RunScript(Rscript:=RscriptText.AutoHandleScript(script.ToString))
        End Using
    End Function

    Private Function RunScript(Rscript As RscriptText)
        Dim args As CommandLine = App.CommandLine
        Dim engineConfig As String = (args("--R_LIBS_USER") Or System.Environment.GetEnvironmentVariable("R_LIBS_USER"))
        Dim vanillaMode As Boolean = args("--vanilla")
        Dim attach As String = args("--attach")
        Dim [error] As String = Nothing
        Dim program As RProgram = RProgram.CreateProgram(Rscript, debug:=False, [error]:=[error])
        Dim ignoreMissingStartupPackages As Boolean = False
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(
            configs:=If(engineConfig.StringEmpty, ConfigFile.localConfigs, engineConfig)
        )

        If Not attach.StringEmpty Then
            ' loading package list
            ' --attach GCModeller,mzkit,REnv
            ' --attach GCModeller.zip;mzkit.zip;/root/REnv/
            Dim packageList As String() = attach.Split(";"c, ","c)

            For Each packageRef As String In packageList
                If packageRef.DirectoryExists Then
                    Dim libdir2 As Libdir = Libdir.FromLocalFileSystem(packageRef)
                    Dim err As Message = PackageLoader2.LoadPackage(libdir2, packageRef.BaseName, quietly:=False, env:=R.globalEnvir)

                    If Not err Is Nothing Then
                        Return handleResult(err, R.globalEnvir, Nothing)
                    End If
                End If
            Next
        End If

        If Not [error].StringEmpty Then
            Call App.LogException([error])
            Call handleResult(RInternal.debug.stop([error], R.globalEnvir), R.globalEnvir, Nothing)

            Return 500
        Else
            Call LoadLibrary(R, ignoreMissingStartupPackages, "base", "utils", "grDevices", "math")
        End If

        Dim result As Object = R.Run(program)

        If (Not result Is Nothing) AndAlso result.GetType Is GetType(Message) Then
            Return DirectCast(result, Message).level
        Else
            Return 0
        End If
    End Function

    Friend Sub LoadLibrary(REnv As RInterpreter, ignoreMissingStartupPackages As Boolean, ParamArray names As String())
        For Each pkgName As String In names
            Call REnv.LoadLibrary(
                packageName:=pkgName,
                ignoreMissingStartupPackages:=ignoreMissingStartupPackages,
                silent:=Not REnv.debug
            )
        Next

        ' Call Console.WriteLine()
    End Sub

    Private Function RunRscriptFile(filepath As String, args As CommandLine) As Integer
        Dim ignoreMissingStartupPackages As Boolean = args("--ignore-missing-startup-packages")
        Dim engineConfig As String = (args("--R_LIBS_USER") Or System.Environment.GetEnvironmentVariable("R_LIBS_USER"))
        Dim SetDllDirectory As String = args("--SetDllDirectory")
        ' redirect the standard output to a log file
        ' this option will mute all console output
        Dim redirectConsoleLog As String = args("--redirect_stdout")
        Dim redirectErrorLog As String = args("--redirect_stderr")
        ' a shortcut of the sink() log function
        ' this option could also redirect the standard output to a log file
        ' but not mute all console outputs
        Dim logfile As String = args("--sink")
        Dim attach As String = args("--attach")
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(
            configs:=If(engineConfig.StringEmpty, ConfigFile.localConfigs, engineConfig)
        )

        For Each var As KeyValuePair(Of String, String) In args.EnvironmentVariables
            Call R.globalEnvir.options.setOption(var.Key, var.Value)
        Next

        If Not logfile.StringEmpty(, True) Then
            Call base.sink(logfile, env:=R.globalEnvir)
        End If

        If Not redirectConsoleLog.StringEmpty Then
            Dim text = App.RedirectLogging(redirectConsoleLog)

            Call App.AddExitCleanHook(
                Sub()
                    Call text.Flush()
                    Call text.Close()
                End Sub)
        End If
        If Not redirectErrorLog.StringEmpty Then
            Dim text = App.RedirectErrLogging(redirectErrorLog)

            Call App.AddExitCleanHook(
                Sub()
                    Call text.Flush()
                    Call text.Close()
                End Sub)
        End If

        If args("--debug") Then
            R.debug = True
        End If

        ' 20241127
        ' the base package should be loaded at first, before we attach other packages
        ' due to the reason of some function from the base packages may be called from
        ' the .onLoad function in the zzz.R when on the package startup
        Call LoadLibrary(R, ignoreMissingStartupPackages, "base", "utils", "grDevices", "math")
        Call VBDebugger.EchoLine("")

        If Not attach.StringEmpty Then
            ' loading package list
            ' --attach GCModeller,mzkit,REnv
            ' --attach GCModeller.zip;mzkit.zip;/root/REnv/
            Dim packageList As String() = attach.Split(";"c, ","c)

            For Each packageRef As String In packageList
                If packageRef.DirectoryExists Then
                    Dim libdir2 As Libdir = Libdir.FromLocalFileSystem(packageRef)
                    Dim err As Message = PackageLoader2.LoadPackage(libdir2, packageRef.BaseName,
                                                                    quietly:=False,
                                                                    env:=R.globalEnvir)
                    If Not err Is Nothing Then
                        Return handleResult(err, R.globalEnvir, Nothing)
                    End If
                End If
            Next
        End If

        If Not SetDllDirectory.StringEmpty Then
            Call R.globalEnvir.options.setOption("SetDllDirectory", SetDllDirectory)
        End If

        If R.debug Then
            Call VBDebugger.EchoLine(args.ToString)
            Call VBDebugger.EchoLine("")
        End If

        'For Each arg As NamedValue(Of String) In args.ToArgumentVector
        '    Call R.Add(CommandLine.TrimNamePrefix(arg.Name), arg.Value, TypeCodes.generic)
        'Next

        Dim result As Object = R.Source(filepath)
        Dim code As Integer = 0

        If RProgram.isException(result) Then
            code = Rscript.handleResult(result, R.globalEnvir, Nothing)
        End If

        If Not logfile.StringEmpty(, True) Then
            Call base.sink(env:=R.globalEnvir)
        End If

        Return code
    End Function
End Module
