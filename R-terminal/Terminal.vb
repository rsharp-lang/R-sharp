#Region "Microsoft.VisualBasic::95fb893dd55670a6ef06f2b07f1b93e7, R-terminal\Terminal.vb"

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

    ' Module Terminal
    ' 
    '     Function: isSimplePrintCall, RunTerminal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Terminal
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Terminal

    Public Function RunTerminal() As Integer
        Dim ps1 As New PS1("> ")
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim exec As Action(Of String) =
            Sub(script)
                Dim program As RProgram = RProgram.BuildProgram(script)
                Dim result = R.Run(program)

                If Not RProgram.isException(result) Then
                    If program.Count = 1 AndAlso program.isSimplePrintCall Then
                        ' do nothing
                        Dim funcName As Literal = DirectCast(program.First, FunctionInvoke).funcName

                        If funcName = "cat" Then
                            Call Console.WriteLine()
                        End If
                    Else
                        Call Internal.base.print(result, R.globalEnvir)
                    End If
                End If
            End Sub

        Call Console.WriteLine("Type 'demo()' for some demos, 'help()' for on-line help, or
'help.start()' for an HTML browser interface to help.
Type 'q()' to quit R.
")
        Call R.LoadLibrary("base")
        Call R.LoadLibrary("utils")

        Call New Shell(ps1, exec) With {
            .Quite = "q()"
        }.Run()

        Return 0
    End Function

    ReadOnly echo As Index(Of String) = {"print", "cat", "echo"}

    <Extension>
    Private Function isSimplePrintCall(program As RProgram) As Boolean
        If Not TypeOf program.First Is FunctionInvoke Then
            Return False
        End If

        Dim funcName = DirectCast(program.First, FunctionInvoke).funcName

        If Not TypeOf funcName Is Literal Then
            Return False
        Else
            Return DirectCast(funcName, Literal).ToString Like echo
        End If
    End Function
End Module

