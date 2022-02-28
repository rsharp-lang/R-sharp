#Region "Microsoft.VisualBasic::bec22c07e49b6af709ef2618e9c38ffe, studio\R-terminal\Terminal.vb"

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
    '     Function: RunTerminal
    ' 
    '     Sub: doRunScript, doRunScriptWithSpecialCommand
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Language.UnixBash
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Terminal

    Dim R As RInterpreter
    Dim Rtask As Thread
    Dim cancel As New ManualResetEvent(initialState:=False)

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
            Call R.LoadLibrary(packageName:=pkgName)
        Next

        Console.WriteLine()
        Console.Title = "R# language"

        AddHandler Console.CancelKeyPress,
            Sub(sender, terminate)
                ' ctrl + C just break the current executation
                ' not exit program running
                terminate.Cancel = True
                cancel.Set()

                If Not Rtask Is Nothing Then
                    Rtask.Abort()
                End If
            End Sub

        Call New Shell(New PS1("> "), AddressOf doRunScriptWithSpecialCommand) With {
            .Quite = "!.R#::quit" & Rnd()
        }.Run()

        Return 0
    End Function

    Private Sub doRunScriptWithSpecialCommand(script As String)
        Select Case script
            Case "CLS"
                Call Console.Clear()
            Case Else
                If Not script Is Nothing Then
                    Rtask = New Thread(Sub() Call doRunScript(script))
                    Rtask.Start()

                    ' block the running task thread at here
                    cancel.Reset()
                    cancel.WaitOne()
                Else
                    Console.WriteLine()
                End If
        End Select

        Console.Title = "R# language"
    End Sub

    Private Sub doRunScript(script As String)
        Dim error$ = Nothing
        Dim program As RProgram = RProgram.BuildProgram(script, [error]:=[error])
        Dim result As Object

        If Not [error].StringEmpty Then
            result = REnv.Internal.debug.stop([error], R.globalEnvir)
        Else
            result = REnv.TryCatch(Function() R.Run(program), debug:=R.debug)
        End If

        Call Rscript.handleResult(result, R.globalEnvir, program)
        Call cancel.Set()
    End Sub
End Module
