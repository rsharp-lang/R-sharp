#Region "Microsoft.VisualBasic::f50bacecbf11b728420ede809e546935, R-sharp\Rscript\Program.vb"

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

    '   Total Lines: 116
    '    Code Lines: 80
    ' Comment Lines: 17
    '   Blank Lines: 19
    '     File Size: 4.56 KB


    ' Module Program
    ' 
    '     Function: Main, Run, RunRscriptFile
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
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

            Dim Rscript As RscriptText = RscriptText.AutoHandleScript(script.ToString)
            Dim [error] As String = Nothing
            Dim program As RProgram = RProgram.CreateProgram(Rscript, debug:=False, [error]:=[error])
            Dim ignoreMissingStartupPackages As Boolean = False
            Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)

            If Not [error].StringEmpty Then
                Call App.LogException([error])
                Call handleResult(Internal.debug.stop([error], R.globalEnvir), R.globalEnvir, Nothing)

                Return 500
            Else
                Call R.LoadLibrary("base", ignoreMissingStartupPackages:=ignoreMissingStartupPackages)
                Call R.LoadLibrary("utils", ignoreMissingStartupPackages:=ignoreMissingStartupPackages)
                Call R.LoadLibrary("grDevices", ignoreMissingStartupPackages:=ignoreMissingStartupPackages)
                Call R.LoadLibrary("math", ignoreMissingStartupPackages:=ignoreMissingStartupPackages)

                Call Console.WriteLine()
            End If

            Dim result As Object = R.Run(program)

            If (Not result Is Nothing) AndAlso result.GetType Is GetType(Message) Then
                Return DirectCast(result, Message).level
            Else
                Return 0
            End If
        End Using
    End Function

    Private Function RunRscriptFile(filepath As String, args As CommandLine) As Integer
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

        If RProgram.isException(result) Then
            Return Rscript.handleResult(result, R.globalEnvir, Nothing)
        Else
            Return 0
        End If
    End Function
End Module
