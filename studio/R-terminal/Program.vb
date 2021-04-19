#Region "Microsoft.VisualBasic::9404d79448ca02793a35939b44070431, studio\R-terminal\Program.vb"

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

' Module Program
' 
'     Function: Main, RunExpression, RunScript
' 
'     Sub: attachPackageFile
' 
' /********************************************************************************/

#End Region

#Const DEBUG = 0

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

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

    Private Function QueryCommandLineArgvs(args As CommandLine) As Integer
        Dim script As String = args.SingleValue
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)

        If Not script.FileExists Then
            Call Internal.debug.PrintMessageInternal(
                 Internal.debug.stop({
                     $"the given R script file is not found on your filesystem!",
                     $"Rscript: {script}"
                 }, R.globalEnvir),
                    R.globalEnvir
            )

            Return 404
        End If

        Dim Rscript As ShellScript = REnv.Components.Rscript.AutoHandleScript(handle:=script)

        If Not Rscript.message.StringEmpty Then
            Call Internal.debug.PrintMessageInternal(
                 Internal.debug.stop(Rscript.message, R.globalEnvir), R.globalEnvir
            )

            Return 500
        End If

        Call Rscript.AnalysisAllCommands()
        Call Rscript.PrintUsage()

        Return 0
    End Function

    Private Function RunExpression(args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim [error] As String = Nothing
        Dim program As RProgram = RProgram.BuildProgram(args.cli, [error]:=[error])
        Dim result As Object

        If Not [error] Is Nothing Then
            result = REnv.Internal.debug.stop([error], R.globalEnvir)
        Else
#If DEBUG Then
            result = R.Run(program)
#Else
            result = REnv.TryCatch(Function() R.Run(program), debug:=R.debug)
#End If
        End If

        Return Rscript.handleResult(result, R.globalEnvir, program)
    End Function

    ''' <summary>
    ''' R# script.R ...
    ''' </summary>
    ''' <param name="filepath$"></param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Private Function RunScript(filepath$, args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim silent As Boolean = args("--silent")
        Dim ignoreMissingStartupPackages As Boolean = args("--ignore-missing-startup-packages")
        Dim verbose As Boolean = args("--verbose")
        Dim attach As String = args("--attach")

        If args("--debug") Then
            R.debug = True
        End If

        If verbose Then
            R.options(verbose:=True)
        End If

        If Not silent Then
            Call Console.WriteLine(args.ToString)
            Call Console.WriteLine()
        End If

        ' Call R.LoadLibrary("base")
        ' Call R.LoadLibrary("utils")
        ' Call R.LoadLibrary("grDevices")
        ' Call R.LoadLibrary("stats")
        For Each pkgName As String In R.configFile.GetStartupLoadingPackages
            Call R.LoadLibrary(
                packageName:=pkgName,
                silent:=silent,
                ignoreMissingStartupPackages:=ignoreMissingStartupPackages
            )
        Next

        If Not silent Then
            Call Console.WriteLine()
        End If

        If Not attach.StringEmpty Then
            If attach.FileExists Then
                Call R.attachPackageFile(zip:=attach)
            Else
                Call Console.WriteLine($"[warning] the specific attach package file '{attach.GetFullPath}' is not found!")
            End If
        End If

        Dim result As Object = R.Source(filepath)

        Return Rscript.handleResult(result, R.globalEnvir, Nothing)
    End Function

    <Extension>
    Private Sub attachPackageFile(R As RInterpreter, zip As String)
        Dim tmpDir As String = TempFileSystem.GetAppSysTempFile("_package", App.PID.ToHexString, zip.BaseName)

        Call UnZip.ImprovedExtractToDirectory(zip, tmpDir, Overwrite.Always)
        Call PackageLoader2.LoadPackage(tmpDir, R.globalEnvir)
    End Sub
End Module
