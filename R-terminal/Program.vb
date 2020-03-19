#Region "Microsoft.VisualBasic::1b9f6bfad14f28b511a7f6d01c677521, R-terminal\Program.vb"

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
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Configuration
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Program

    <DebuggerStepThrough>
    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(
            args:=App.CommandLine,
            executeFile:=AddressOf RunScript,
            executeEmpty:=AddressOf Terminal.RunTerminal,
            executeNotFound:=AddressOf RunExpression
        )
    End Function

    Private Function RunExpression(args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim [error] As String = Nothing
        Dim program As RProgram = RProgram.BuildProgram(args.cli, [error]:=[error])
        Dim result As Object

        If Not [error] Is Nothing Then
            result = REnv.Internal.debug.stop([error], R.globalEnvir)
        Else
            result = REnv.TryCatch(Function() R.Run(program))
        End If

        Return Rscript.handleResult(result, R.globalEnvir, program)
    End Function

    Private Function RunScript(filepath$, args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)

        If args("--debug") Then
            R.debug = True
        End If

        Call Console.WriteLine(args.ToString)
        Call Console.WriteLine()

        ' Call R.LoadLibrary("base")
        ' Call R.LoadLibrary("utils")
        ' Call R.LoadLibrary("grDevices")
        ' Call R.LoadLibrary("stats")
        For Each pkgName As String In R.configFile.GetStartupLoadingPackages
            Call R.LoadLibrary(packageName:=pkgName)
        Next

        Call Console.WriteLine()

        Dim result As Object = R.Source(filepath)

        Return Rscript.handleResult(result, R.globalEnvir, Nothing)
    End Function
End Module
