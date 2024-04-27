#Region "Microsoft.VisualBasic::8f1a70faabf11315cdc9e4d070ee3e98, E:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/CLI/ExternalCommandLine.vb"

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

    '   Total Lines: 197
    '    Code Lines: 132
    ' Comment Lines: 38
    '   Blank Lines: 27
    '     File Size: 7.14 KB


    '     Class ExternalCommandLine
    ' 
    '         Properties: expressionName, ShellRscriptHost, ShellRsharpCmd, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate, getCommandlineString, possibleInterpolationFailure, ToString
    ' 
    '         Sub: SetAttribute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    ''' <summary>
    ''' External commandline invoke
    ''' 
    ''' ```
    ''' @"cli"
    ''' ```
    ''' </summary>
    ''' <remarks>
    ''' Supports the commandline string input in multiple lines
    ''' </remarks>
    Public Class ExternalCommandLine : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.list
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Shell
            End Get
        End Property

        ''' <summary>
        ''' expression object should generates the commandline string value
        ''' </summary>
        Friend cli As Expression
        Friend ioRedirect As Boolean = True

        ''' <summary>
        ''' do not should the std_out content of child process?
        ''' </summary>
        Friend silent As Boolean = False

        ''' <summary>
        ''' A special path to indicate the R#.exe file path
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ShellRsharpCmd As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Get
#If NETCOREAPP And Not WINDOWS Then
                Return $"{App.HOME}/R#.dll"
#Else
                Return $"{App.HOME}/R#.exe"
#End If
            End Get
        End Property

        ''' <summary>
        ''' A special path to indicate the Rscript.exe file path
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ShellRscriptHost As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Get
#If NETCOREAPP And Not WINDOWS Then
                Return $"{App.HOME}/Rscript.dll"
#Else
                Return $"{App.HOME}/Rscript.exe"
#End If
            End Get
        End Property

        Sub New(shell As Expression)
            cli = shell
        End Sub

        Public Overrides Function ToString() As String
            Return cli.ToString
        End Function

        Public Sub SetAttribute(data As NamedValue(Of String))
            Dim flag As Boolean = data.Value.ParseBoolean

            Select Case data.Name.ToLower
                Case "ioredirect" : ioRedirect = flag
                Case "silent" : silent = flag
            End Select
        End Sub

        Private Function getCommandlineString(env As Environment, ByRef clr As Boolean) As [Variant](Of String, Message)
            Dim val As Object = cli.Evaluate(env)

            If Program.isException(val) Then
                Return DirectCast(val, Message)
            End If

            Dim valStr As String = CType(REnv.getFirst(val), String)
            Dim tokens As String() = valStr.LineTokens _
                .Select(Function(line) DelimiterParser.GetTokens(line)) _
                .IteratesALL _
                .Select(Function(str)
                            Return str.Trim(ASCII.TAB, " "c, ASCII.CR, ASCII.LF)
                        End Function) _
                .ToArray

            If tokens.IsNullOrEmpty Then
                Return Internal.debug.stop("Empty commandline to shell, no target to run!", env)
            End If

            ' 20230214 the first token of the commandline may
            ' be some special object in the R# interpreter
            ' commandline shell module:
            '
            ' 1. %RENV% means R-cmd shell
            ' 2. %REXEC% means Rscript host

            Select Case LCase(tokens(Scan0))
                Case "%rexec%"
                    clr = True
                    tokens(0) = ShellRscriptHost

                Case "%renv%"
                    clr = True
                    tokens(0) = ShellRsharpCmd

            End Select

            Return tokens _
                .Select(Function(t) t.CLIToken) _
                .JoinBy(" ")
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' handling of the multiple line commandline string input
            Dim clr As Boolean = False
            Dim commandline = getCommandlineString(envir, clr)
            Dim commandlineStr$ = commandline.TryCast(Of String)

            If commandline Like GetType(Message) Then
                Return commandline.TryCast(Of Message)
            End If

            If envir.globalEnvironment.debugMode Then
                Call base.print("get the raw commandline string input:", , envir)
                Call base.print(commandlineStr,, envir)
            End If

            If commandlineStr.DoCall(AddressOf SyntaxImplements.isInterpolation) Then
                Call commandlineStr _
                    .DoCall(Function(cli)
                                Return possibleInterpolationFailure(cli, envir)
                            End Function) _
                    .DoCall(AddressOf envir.globalEnvironment.messages.Add)
            End If

            Return Internal.Invokes.utils.system(
                command:=commandlineStr,
                show_output_on_console:=Not silent,
                env:=envir,
                clr:=clr
            )
        End Function

        ''' <summary>
        ''' create a warning message about the commandline 
        ''' string in string interpolate syntax
        ''' </summary>
        ''' <param name="commandline"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Private Shared Function possibleInterpolationFailure(commandline As String, envir As Environment) As Message
            Return New Message With {
                .message = {
                    $"The input commandline string contains string interpolation syntax tag...",
                    $"commandline: " & commandline
                },
                .level = MSG_TYPES.WRN,
                .environmentStack = debug.getEnvironmentStack(envir),
                .trace = devtools.ExceptionData.GetCurrentStackTrace
            }
        End Function
    End Class
End Namespace
