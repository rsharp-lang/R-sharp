﻿#Region "Microsoft.VisualBasic::442fdaf8b98c45f97caf20e7c3d16844, studio\R-terminal\Terminal.vb"

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

    '   Total Lines: 305
    '    Code Lines: 189 (61.97%)
    ' Comment Lines: 73 (23.93%)
    '    - Xml Docs: 79.45%
    ' 
    '   Blank Lines: 43 (14.10%)
    '     File Size: 11.00 KB


    ' Module Terminal
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: example, help, RunTerminal
    ' 
    '     Sub: doRunScriptWithSpecialCommandSync, q, quit
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.LineEdit
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' the ``R#`` terminal console
''' </summary>
Module Terminal

    Dim R As RInterpreter
    Dim Rtask As Task
    Dim exec As Boolean = False

    Friend cts As CancellationTokenSource

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
    ''' <keywords>terminal,interactive</keywords>
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
RE0:
        Call Console.Write("Save workspace image? [y/n/c]: ")

        ' null string will be return if ctrl+C was pressed
        Dim input As String = Strings.Trim(Console.ReadLine).Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB)

        If input = "c" Then
            ' cancel
            Return
        ElseIf cts.IsCancellationRequested Then
            Call Console.WriteLine()
            Return
        End If

        If input = "y" Then
            ' save image for yes
            Dim saveImage As Symbol = envir.FindSymbol("save.image")

            If Not saveImage Is Nothing AndAlso TypeOf saveImage.value Is RMethodInfo Then
                Call DirectCast(saveImage.value, RMethodInfo).Invoke(envir, {})
            End If
        ElseIf input = "n" Then
            ' do nothing for no
        Else
            GoTo RE0
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
    ''' get example usage of the specific function
    ''' </summary>
    ''' <param name="x">should be a R# function</param>
    ''' <param name="env"></param>
    ''' <example>
    ''' # view the example code for function c();
    ''' example(c);
    ''' </example>
    <ExportAPI("example")>
    Public Function example(x As Object, Optional env As Environment = Nothing) As Object
        If x Is Nothing OrElse TypeOf x IsNot RMethodInfo Then
            Return Nothing
        End If

        Dim rdocumentation = env.globalEnvironment.packages.packageDocs
        Dim docs As ProjectMember = rdocumentation.GetAnnotations(DirectCast(x, RMethodInfo).GetNetCoreCLRDeclaration)

        If docs Is Nothing Then
            Return Nothing
        Else
            Dim code_str As String = vbCrLf & " " & docs.example
            Call ConsoleSyntaxHighlightPrinter.PrintCode(code_str, env.globalEnvironment.stdout)
            Return code_str
        End If
    End Function

    ''' <summary>
    ''' open the help document file
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function open the html help document file on windows and
    ''' open the unix man page file on linux system.
    ''' </remarks>
    <ExportAPI("help")>
    Public Function help(x As Object, Optional env As Environment = Nothing) As Object
        Dim f As RFunction
        Dim globalPkg As SymbolNamespaceSolver = env.globalEnvironment.attachedNamespace

        If x Is Nothing Then
            Return invisible.NULL
        ElseIf Not x.GetType.ImplementInterface(Of RFunction) Then
            Return Message.InCompatibleType(GetType(RFunction), x.GetType, env)
        Else
            f = x
        End If

        If Platform = PlatformID.Unix Then
            Dim ns_str As String = globalPkg.FindNamespace(f.name).FirstOrDefault
            Dim manfile As String

            If ns_str.StringEmpty Then
                Return invisible.NULL
            Else
                manfile = $"/etc/r_env/library/{ns_str}/package/man/{f.name}.1"
            End If

            If Not manfile.FileExists Then
                Return invisible.NULL
            Else
                Call VBDebugger.EchoLine("")
                Call PipelineProcess.ExecSub("man", manfile, Sub(line) Call VBDebugger.EchoLine(line))
                Call VBDebugger.EchoLine("")
                ' Call Interaction.Shell($"man {manfile}", AppWinStyle.MaximizedFocus, Wait:=True)
            End If
        Else

        End If

        Return invisible.NULL
    End Function
#End Region

    Sub New()
        Dim Rcore = GetType(RInterpreter).Assembly.FromAssembly
        Dim framework = GetType(App).Assembly.FromAssembly
        Dim Rterm = GetType(Terminal).Assembly.FromAssembly

        Call MarkdownRender.Print($"
  `` , __         ``  | 
  ``/|/  \  |  |  ``  | Documentation: https://r_lang.dev.SMRUCC.org/
  `` |___/--+--+--``  |
  `` | \  --+--+--``  | Version ``{Rcore.AssemblyVersion}`` (**{Rterm.BuiltTime.ToString}**)
  `` |  \_/ |  |  ``  | sciBASIC.NET Runtime: ``{framework.AssemblyVersion}``         
                  
Welcome to the R# language
")
        Call Console.WriteLine()
        Call MarkdownRender.Print("Type ``ls(""REnv"")`` for get internal functions, ``example`` for view function 
usage example code, example as ``example(example)``, or ``help.start()`` for 
an HTML browser interface to help. Type ``q()`` to quit R#.
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
        Dim R_exec As Action(Of String) = AddressOf doRunScriptWithSpecialCommandSync
        Dim editor As New LineEditor("Rscript", 5000) With {
            .HeuristicsMode = True,
            .TabAtStartCompletes = False,
            .MaxWidth = 120
        }
        Dim file_config As String = If(engineConfig.StringEmpty, ConfigFile.localConfigs, engineConfig)

        R = RInterpreter.FromEnvironmentConfiguration(configs:=file_config)
        R.strict = False

        editor.AutoCompleteEvent = AddressOf New IntelliSense(R).AutoCompletion

        If R.verbose Then
            Call VBDebugger.EchoLine($"application platform id: {App.Platform}(is_microsoft_platform: {App.IsMicrosoftPlatform})")
            Call VBDebugger.EchoLine($"load config file: {file_config}")
        End If

        ' Call R.LoadLibrary("base")
        ' Call R.LoadLibrary("utils")
        ' Call R.LoadLibrary("grDevices")
        ' Call R.LoadLibrary("stats")
        For Each pkgName As String In R.configFile _
            .GetStartupLoadingPackages _
            .Join({"REnv"}) _
            .Distinct

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

        Call New Shell(New PS1("> "), R_exec, dev:=New LineReader(editor)) With {
            .Quite = "!.R#::quit" & Rnd()
        }.Run()

        Return 0
    End Function

    Private Sub doRunScriptWithSpecialCommandSync(script As String)
        Call doRunScriptWithSpecialCommand(script)

        Do While exec
            Call Thread.Sleep(10)
        Loop
    End Sub

    Private Async Sub doRunScriptWithSpecialCommand(script As String)
        cts = New CancellationTokenSource
        exec = True

        Select Case script
            Case "CLS"
                Call Console.Clear()
            Case Else
                If Not script.StringEmpty Then
                    Await New RunScript(R, script) _
                        .doRunScript(cts.Token) _
                        .CancelWith(
                            cancellationToken:=cts.Token,
                            swallowCancellationException:=True
                         )
                Else
                    Console.WriteLine()
                End If
        End Select

        exec = False
        Console.Title = "R# language"
    End Sub
End Module
