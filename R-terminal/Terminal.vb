#Region "Microsoft.VisualBasic::f27c33e7d5eaf84ab38c2c8559234329, R-terminal\Terminal.vb"

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
'     Function: isImports, isInvisible, isValueAssign, RunTerminal
' 
'     Sub: doRunScript, doRunScriptWithSpecialCommand
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Terminal
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Configuration
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Terminal

    Dim R As RInterpreter
    Dim echo As Index(Of String) = {"print", "cat", "echo", "q", "quit"}

    Sub New()
        Dim Rcore = GetType(RInterpreter).Assembly.FromAssembly
        Dim framework = GetType(App).Assembly.FromAssembly

        Call MarkdownRender.Print($"
  `` , __         ``  | 
  ``/|/  \  |  |  ``  | Documentation: https://r_lang.dev.SMRUCC.org/
  `` |___/--+--+--``  |
  `` | \  --+--+--``  | Version ``{Rcore.AssemblyVersion}`` (**{Rcore.BuiltTime.ToString}**)
  `` |  \_/ |  |  ``  | sciBASIC.NET Runtime: ``{framework.AssemblyVersion}``         
                  
Welcome to the R# language
")
        Call Console.WriteLine()
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

        Console.WriteLine()
        Console.Title = "R# language"

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

        Console.Title = "R# language"
    End Sub

    Private Sub doRunScript(script As String)
        Dim program As RProgram = RProgram.BuildProgram(script)
        Dim result As Object = REnv.TryCatch(Function() R.Run(program))

        If RProgram.isException(result, R.globalEnvir) Then
            Return
        End If

        If program.Count = 1 AndAlso program.EndWithFuncCalls(echo.Objects) Then
            ' do nothing
            Dim funcName As Literal = DirectCast(program.First, FunctionInvoke).funcName

            If funcName = "cat" Then
                Call Console.WriteLine()
            End If
        ElseIf Not program.isValueAssign AndAlso Not program.isImports Then
            If Not isInvisible(result) Then
                Call base.print(result, R.globalEnvir)
            End If
        End If
    End Sub

    Private Function isInvisible(result As Object) As Boolean
        If result Is Nothing Then
            Return False
        ElseIf result.GetType Is GetType(RReturn) Then
            Return DirectCast(result, RReturn).invisible
        ElseIf result.GetType Is GetType(invisible) Then
            Return True
        Else
            Return False
        End If
    End Function

    <DebuggerStepThrough>
    <Extension>
    Private Function isImports(program As RProgram) As Boolean
        Return program.Count = 1 AndAlso TypeOf program.First Is [Imports]
    End Function

    <DebuggerStepThrough>
    <Extension>
    Private Function isValueAssign(program As RProgram) As Boolean
        ' 如果是赋值表达式的话，也不会在终端上打印结果值
        Return TypeOf program.Last Is ValueAssign OrElse TypeOf program.Last Is DeclareNewVariable
    End Function
End Module
