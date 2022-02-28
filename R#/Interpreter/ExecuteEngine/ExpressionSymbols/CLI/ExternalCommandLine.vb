#Region "Microsoft.VisualBasic::6a7c4e41bfb9585f161149f1c306d4af, R#\Interpreter\ExecuteEngine\ExpressionSymbols\CLI\ExternalCommandLine.vb"

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

    '     Class ExternalCommandLine
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate, possibleInterpolationFailure
    ' 
    '         Sub: SetAttribute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Parsers
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
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
        Friend ioRedirect As Boolean = True

        Sub New(shell As Expression)
            cli = shell
        End Sub

        Public Sub SetAttribute(data As NamedValue(Of String))
            Select Case data.Name.ToLower
                Case "ioredirect"
                    ioRedirect = data.Value.ParseBoolean
            End Select
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim commandlineStr$ = CType(REnv.getFirst(cli.Evaluate(envir)), String) _
                .LineTokens _
                .Select(Function(line) CLIParser.GetTokens(line)) _
                .IteratesALL _
                .Select(Function(str)
                            Return str.Trim(ASCII.TAB, " "c, ASCII.CR, ASCII.LF).CLIToken
                        End Function) _
                .JoinBy(" ")

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

            Return Internal.Invokes.utils.system(commandlineStr, env:=envir)
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
