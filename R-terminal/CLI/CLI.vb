#Region "Microsoft.VisualBasic::19bf4b6e2d78b5fb37d253f6089e081a, R-terminal\CLI\CLI.vb"

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
    '     Function: Info, Install, Version
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
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
              Description:=".NET Framework 4.7 assembly module file.")>
    Public Function Install(args As CommandLine) As Integer
        Dim module$ = args <= "/module"
        Dim config As New Options(ConfigFile.localConfigs)

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {ConfigFile.localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        If [module].StringEmpty Then
            Return "Missing '/module' argument!".PrintException
        Else
            Dim pkgMgr As New PackageManager(config)

            Call pkgMgr.InstallLocals(dllFile:=[module])
            Call pkgMgr.Flush()
        End If

        Return 0
    End Function

    <ExportAPI("--version")>
    <Description("Print R# interpreter version")>
    Public Function Version(args As CommandLine) As Integer
        Console.Write(GetType(RInterpreter).Assembly.FromAssembly.AssemblyVersion)
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
