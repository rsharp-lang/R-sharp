#Region "Microsoft.VisualBasic::a1564c49d4425c3c516d879551f2510e, studio\R-terminal\CLI\CLI.vb"

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

' Module CLI
' 
'     Function: Info, InitializeEnvironment, Install, SyntaxText, Version
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System
Imports SMRUCC.Rsharp.System.Configuration
Imports SMRUCC.Rsharp.System.Package
Imports RlangScript = SMRUCC.Rsharp.Runtime.Components.Rscript
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

<CLI> Module CLI

    <ExportAPI("--install.packages")>
    <Description("Install new packages.")>
    <Usage("--install.packages /module <*.dll> [--verbose]")>
    <Argument("/module", False, CLITypes.File,
              Extensions:="*.dll",
              Description:=".NET Framework 4.8 assembly module file.")>
    Public Function Install(args As CommandLine) As Integer
        Dim module$ = args <= "/module"
        Dim config As New Options(ConfigFile.localConfigs)

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {ConfigFile.localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        If [module].StringEmpty Then
            Return "Missing '/module' argument!".PrintException
        End If

        Using pkgMgr As New PackageManager(config)
            If Not [module].ToLower.StartsWith("scan=") Then
                Call pkgMgr.InstallLocals(dllFile:=[module])
            Else
                For Each file As String In ls - l - "*.dll" <= [module].GetTagValue("=", trim:=True).Value
                    Try
                        Dim assm As Assembly = Assembly.LoadFrom(file.GetFullPath)

                        If Not assm.GetCustomAttribute(Of RPackageModuleAttribute) Is Nothing Then
                            Call pkgMgr.InstallLocals(dllFile:=file)
                        End If
                    Catch ex As Exception

                    End Try
                Next
            End If
        End Using

        Return 0
    End Function

    <ExportAPI("--version")>
    <Description("Print R# interpreter version")>
    Public Function Version(args As CommandLine) As Integer
        Console.Write(GetType(RInterpreter).Assembly.FromAssembly.AssemblyVersion)
        Return 0
    End Function

    <ExportAPI("--setup")>
    <Description("Initialize the R# runtime environment.")>
    Public Function InitializeEnvironment(args As CommandLine) As Integer
        Dim config As New Options(ConfigFile.localConfigs)

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {ConfigFile.localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        App.CurrentDirectory = App.HOME

        Using pkgMgr As New PackageManager(config)
            For Each file As String In {"R.base.dll", "R.graph.dll", "R.graphics.dll", "R.math.dll", "R.plot.dll"}
                If Not file.FileExists Then
                    file = "Library/" & file
                End If

                Call pkgMgr.InstallLocals(dllFile:=file)
                Call pkgMgr.Flush()
            Next
        End Using

        Return 0
    End Function

    <ExportAPI("--man.1")>
    <Description("Exports unix man page data for current installed packages.")>
    <Usage("--man.1 [--module <module.dll> --out <directory, default=./>]")>
    Public Function unixman(args As CommandLine) As Integer
        Dim out$ = args("--out") Or "./"
        Dim module$ = args("--module")
        Dim env As New RInterpreter
        Dim xmldocs = env.globalEnvir.packages.packageDocs
        Dim utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage

        If [module].FileExists Then
            For Each pkg As Package In PackageLoader.ParsePackages(dll:=[module])
                For Each ref As String In pkg.ls
                    Dim symbol As RMethodInfo = pkg.GetFunction(apiName:=ref)
                    Dim docs = xmldocs.GetAnnotations(symbol.GetRawDeclares)
                    Dim help As UnixManPage = UnixManPagePrinter.CreateManPage(symbol, docs)

                    Call UnixManPage _
                        .ToString(help, "man page create by R# package system.") _
                        .SaveTo($"{out}/{pkg.namespace}/{ref}.1", utf8)
                Next

                Call $"load: {pkg.info.Namespace}".__INFO_ECHO
            Next
        Else
            ' run build for all installed package modules
            For Each pkg As Package In env.globalEnvir.packages.AsEnumerable
                For Each ref As String In pkg.ls
                    Dim symbol As RMethodInfo = pkg.GetFunction(apiName:=ref)
                    Dim docs = xmldocs.GetAnnotations(symbol.GetRawDeclares)
                    Dim help As UnixManPage = UnixManPagePrinter.CreateManPage(symbol, docs)

                    Call UnixManPage _
                        .ToString(help, "man page create by R# package system.") _
                        .SaveTo($"{out}/{pkg.namespace}/{ref}.1", utf8)
                Next
            Next
        End If

        Return 0
    End Function

    <ExportAPI("--info")>
    <Description("Print R# interpreter version information and R# terminal version information.")>
    Public Function Info(args As CommandLine) As Integer
        Dim Rterminal As AssemblyInfo = GetType(Program).Assembly.FromAssembly
        Dim RsharpCore As AssemblyInfo = GetType(RInterpreter).Assembly.FromAssembly

        Call Rterminal.AppSummary(Nothing, Nothing, App.StdOut)
        Call RsharpCore.AppSummary(Nothing, Nothing, App.StdOut)

        Call App.StdOut.value.Flush()
        Call New RInterpreter().Print("sessionInfo();")

        Return 0
    End Function

    <ExportAPI("--syntax")>
    <Description("Show syntax parser result of the input script.")>
    <Usage("--syntax /script <script.R>")>
    Public Function SyntaxText(args As CommandLine) As Integer
        Dim script$ = args <= "/script"
        Dim Rscript As RlangScript = RlangScript.FromFile(script)
        Dim program As RProgram = RProgram.CreateProgram(Rscript, debug:=False)

        Call Console.WriteLine(program.ToString)

        Return 0
    End Function
End Module
