#Region "Microsoft.VisualBasic::8266fe2c8cafc8ebb2dea38fe820f61a, Rscript\Program.vb"

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
    '     Function: Main, Run
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

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
        Return GetType(CLI).RunCLI(App.CommandLine, executeFile:=AddressOf Run, executeEmpty:=AddressOf Run)
    End Function

    ''' <summary>
    ''' Run R script from std_input 
    ''' </summary>
    ''' <returns></returns>
    Private Function Run() As Integer
        Dim text As String = App.std
    End Function

    Private Function Run(filepath$, args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim ignoreMissingStartupPackages As Boolean = args("--ignore-missing-startup-packages")

        If args("--debug") Then
            R.debug = True
        End If

        If R.debug Then
            Call Console.WriteLine(args.ToString)
            Call Console.WriteLine()
        End If

        Call R.LoadLibrary("base", ignoreMissingStartupPackages:=ignoreMissingStartupPackages)
        Call R.LoadLibrary("utils", ignoreMissingStartupPackages:=ignoreMissingStartupPackages)
        Call R.LoadLibrary("grDevices", ignoreMissingStartupPackages:=ignoreMissingStartupPackages)
        Call R.LoadLibrary("math", ignoreMissingStartupPackages:=ignoreMissingStartupPackages)

        Call Console.WriteLine()

        'For Each arg As NamedValue(Of String) In args.ToArgumentVector
        '    Call R.Add(CommandLine.TrimNamePrefix(arg.Name), arg.Value, TypeCodes.generic)
        'Next

        Dim result As Object = R.Source(filepath)

        If (Not result Is Nothing) AndAlso result.GetType Is GetType(Message) Then
            Return DirectCast(result, Message).level
        Else
            Return 0
        End If
    End Function
End Module
