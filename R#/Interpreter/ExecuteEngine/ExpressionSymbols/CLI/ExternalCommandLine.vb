#Region "Microsoft.VisualBasic::4a643fba7d90157b42cc25b7d8ab095e, R#\Interpreter\ExecuteEngine\ExpressionSymbols\CLI\CommandLine.vb"

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

'     Class CommandLine
' 
'         Properties: expressionName, type
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Evaluate, possibleInterpolationFailure
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
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


        Friend cli As Expression

        Sub New(shell As Expression)
            cli = shell
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim commandlineStr$ = CType(REnv.getFirst(cli.Evaluate(envir)), String) _
                .LineTokens _
                .Select(Function(str)
                            Return str.Trim(ASCII.TAB, " "c, ASCII.CR, ASCII.LF)
                        End Function) _
                .JoinBy(" ")
            ' Dim process As New IORedirectFile(commandline, isShellCommand:=True, debug:=envir.globalEnvironment.debugMode)
            Dim process As CommandLine = CommandLine.Parse(commandlineStr)
            Dim error_code%
            Dim arguments As String = process.Tokens _
                .Skip(1) _
                .Select(Function(i) i.CLIToken) _
                .JoinBy(" ")

            If commandlineStr.DoCall(AddressOf SyntaxImplements.isInterpolation) Then
                Call commandlineStr _
                    .DoCall(Function(cli)
                                Return possibleInterpolationFailure(cli, envir)
                            End Function) _
                    .DoCall(AddressOf envir.globalEnvironment.messages.Add)
            End If

            Dim std_out$() = PipelineProcess.Call(process.Name, arguments, exitCode:=error_code).LineTokens

            If Not envir.globalEnvironment.options.stdout_multipline Then
                std_out = {std_out.JoinBy(vbCrLf)}
            End If

            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"std_out", std_out},
                    {"error_code", error_code},
                    {"command", commandlineStr}
                }
            }
        End Function

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
