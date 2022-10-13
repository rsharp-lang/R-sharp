#Region "Microsoft.VisualBasic::774b22ad33c699eaa6c1d49ddfc14380, R-sharp\studio\R-terminal\Terminal.vb"

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

'   Total Lines: 219
'    Code Lines: 126
' Comment Lines: 60
'   Blank Lines: 33
'     File Size: 7.46 KB


' Module Terminal
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: RunTerminal
' 
'     Sub: [exit], doRunScript, doRunScriptWithSpecialCommand, q, quit
' 
' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

''' <summary>
''' the ``R#`` terminal console
''' </summary>
Module Terminal

    Dim R As RInterpreter
    Dim Rtask As Task
    Dim cts As CancellationTokenSource

#Region "enable quit the R# environment in the terminal console mode"

    ''' <summary>
    ''' # Terminate an R Session
    ''' 
    ''' The function ``quit`` or its alias ``q`` terminate the current R session.
    ''' </summary>
    ''' <param name="save">
    ''' a character string indicating whether the environment (workspace) should be saved, 
    ''' one of ``"no"``, ``"yes"``, ``"ask"`` or ``"default"``.
    ''' </param>
    ''' <param name="status">
    ''' the (numerical) error status to be returned to the operating system, where relevant. 
    ''' Conventionally 0 indicates successful completion.
    ''' </param>
    ''' <param name="runLast">
    ''' should ``.Last()`` be executed?
    ''' </param>
    ''' 
    <ExportAPI("quit")>
    Public Sub quit(Optional save$ = "default",
                    Optional status% = 0,
                    Optional runLast As Boolean = True,
                    Optional envir As Environment = Nothing)

        Call q(save, status, runLast, envir)
    End Sub

    ''' <summary>
    ''' # Terminate an R Session
    ''' 
    ''' The function ``quit`` or its alias ``q`` terminate the current R session.
    ''' </summary>
    ''' <param name="save">
    ''' a character string indicating whether the environment (workspace) should be saved, 
    ''' one of ``"no"``, ``"yes"``, ``"ask"`` or ``"default"``.
    ''' </param>
    ''' <param name="status">
    ''' the (numerical) error status to be returned to the operating system, where relevant. 
    ''' Conventionally 0 indicates successful completion.
    ''' </param>
    ''' <param name="runLast">
    ''' should ``.Last()`` be executed?
    ''' </param>
    ''' 
    <ExportAPI("q")>
    Public Sub q(Optional save$ = "default",
                 Optional status% = 0,
                 Optional runLast As Boolean = True,
                 Optional envir As Environment = Nothing)

        Call Console.Write("Save workspace image? [y/n/c]: ")

        Dim input As String = Console.ReadLine.Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB)

        If input = "c" Then
            ' cancel
            Return
        End If

        If input = "y" Then
            ' save image for yes
            Dim saveImage As Symbol = envir.FindSymbol("save.image")

            If Not saveImage Is Nothing AndAlso TypeOf saveImage.value Is RMethodInfo Then
                Call DirectCast(saveImage.value, RMethodInfo).Invoke(envir, {})
            End If
        Else
            ' do nothing for no
        End If

        If runLast Then
            Dim last = envir.FindSymbol(".Last")

            If Not last Is Nothing Then
                Call DirectCast(last, RFunction).Invoke(envir, {})
            End If
        End If

        Call App.Exit(status)
    End Sub

    ''' <summary>
    ''' force quit of current R# session without confirm
    ''' </summary>
    ''' <param name="status"></param>
    ''' 
    <ExportAPI("exit")>
    Public Sub [exit](status As Integer)
        Call App.Exit(status)
    End Sub
#End Region

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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 在R终端模式下，是关闭严格模式的，即变量不需要首先使用let进行声明就可以直接赋值
    ''' </remarks>
    Public Function RunTerminal() As Integer
        Dim engineConfig As String = System.Environment.GetEnvironmentVariable("R_LIBS_USER")

        R = RInterpreter.FromEnvironmentConfiguration(
            configs:=If(engineConfig.StringEmpty, ConfigFile.localConfigs, engineConfig)
        )
        R.strict = False

        ' Call R.LoadLibrary("base")
        ' Call R.LoadLibrary("utils")
        ' Call R.LoadLibrary("grDevices")
        ' Call R.LoadLibrary("stats")
        For Each pkgName As String In R.configFile.GetStartupLoadingPackages
            Call R.LoadLibrary(packageName:=pkgName, silent:=True)
        Next

        Console.WriteLine()
        Console.Title = "R# language"

        Call Internal.invoke.pushEnvir(GetType(Terminal))

        AddHandler Console.CancelKeyPress,
            Sub(sender, terminate)
                ' ctrl + C just break the current executation
                ' not exit program running
                terminate.Cancel = True
                cts.Cancel()
            End Sub

        Call New Shell(New PS1("> "), AddressOf doRunScriptWithSpecialCommand) With {
            .Quite = "!.R#::quit" & Rnd()
        }.Run()

        Return 0
    End Function

    Private Sub doRunScriptWithSpecialCommand(script As String)
        cts = New CancellationTokenSource

        Select Case script
            Case "CLS"
                Call Console.Clear()
            Case Else
                If Not script Is Nothing Then
                    Rtask = New RunScript(script).doRunScript(cts.Token)

                    Do While Not Rtask.IsCompleted
                        If cts.IsCancellationRequested Then
                            Exit Do
                        Else
                            Call Thread.Sleep(1)
                        End If
                    Loop
                Else
                    Console.WriteLine()
                End If
        End Select

        Console.Title = "R# language"
    End Sub

    Private Class RunScript

        ReadOnly script As String

        Sub New(script As String)
            Me.script = script
        End Sub

        Public Async Function doRunScript(ct As CancellationToken) As Task(Of Integer)
            Dim error$ = Nothing
            Dim program As RProgram = RProgram.BuildProgram(script, [error]:=[error])
            Dim result As Object

            Await Task.Delay(1)

            If Not [error].StringEmpty Then
                result = REnv.Internal.debug.stop([error], R.globalEnvir)
            Else
                result = REnv.TryCatch(Function() R.Run(program), debug:=R.debug)
            End If

            Return Rscript.handleResult(result, R.globalEnvir, program)
        End Function

    End Class
End Module
