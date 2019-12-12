#Region "Microsoft.VisualBasic::032d6a3ff0328cbb8a851984bf4903da, R-terminal\Terminal.vb"

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
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: isSimplePrintCall, isValueAssign, RunTerminal
    ' 
    '     Sub: doRunScript
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Terminal
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Terminal

    Dim R As RInterpreter

    Sub New()
        Call Console.WriteLine("Type 'demo()' for some demos, 'help()' for on-line help, or
'help.start()' for an HTML browser interface to help.
Type 'q()' to quit R.
")
    End Sub

    Public Function RunTerminal() As Integer
        R = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)

        Call R.LoadLibrary("base")
        Call R.LoadLibrary("utils")
        Call R.LoadLibrary("grDevices")

        Call Console.WriteLine()

        Call New Shell(New PS1("> "), AddressOf doRunScriptWithSpecialCommand) With {
            .Quite = "!.R#::quit"
        }.Run()

        Return 0
    End Function

    Private Sub doRunScriptWithSpecialCommand(script As String)
        Select Case script
            Case "CLS"
                Call Console.Clear()
            Case Else
                Call doRunScript(script)
        End Select
    End Sub

    Private Sub doRunScript(script As String)
        Dim program As RProgram = RProgram.BuildProgram(script)
        Dim result = R.Run(program)

        If RProgram.isException(result) Then
            Return
        End If

        If program.Count = 1 AndAlso program.isSimplePrintCall Then
            ' do nothing
            Dim funcName As Literal = DirectCast(program.First, FunctionInvoke).funcName

            If funcName = "cat" Then
                Call Console.WriteLine()
            End If
        ElseIf Not program.isValueAssign Then
            Call base.print(result, R.globalEnvir)
        End If
    End Sub

    ReadOnly echo As Index(Of String) = {"print", "cat", "echo"}

    <DebuggerStepThrough>
    <Extension>
    Private Function isValueAssign(program As RProgram) As Boolean
        ' 如果是赋值表达式的话，也不会在终端上打印结果值
        Return TypeOf program.Last Is ValueAssign
    End Function

    <DebuggerStepThrough>
    <Extension>
    Private Function isSimplePrintCall(program As RProgram) As Boolean
        If Not TypeOf program.First Is FunctionInvoke Then
            Return False
        End If

        Dim funcName As Expression = DirectCast(program.First, FunctionInvoke).funcName

        If Not TypeOf funcName Is Literal Then
            Return False
        Else
            Return DirectCast(funcName, Literal).ToString Like echo
        End If
    End Function
End Module
