#Region "Microsoft.VisualBasic::f7ba0a07e8700a9027a141c3df9e4849, D:/GCModeller/src/R-sharp/studio/R-terminal//Terminal.vb"

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

'   Total Lines: 248
'    Code Lines: 150
' Comment Lines: 60
'   Blank Lines: 38
'     File Size: 8.35 KB


' Module Terminal
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: RunTerminal
' 
'     Sub: [exit], doRunScriptWithSpecialCommandSync, q, quit
'     Class RunScript
' 
'         Constructor: (+1 Overloads) Sub New
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.LineEdit
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
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
    Dim exec As Boolean = False

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
    ''' force quit of current R# session without confirm
    ''' </summary>
    ''' <param name="status"></param>
    ''' 
    <ExportAPI("exit")>
    Public Sub [exit](status As Integer)
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
            Return vbCrLf & " " & docs.example
        End If
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
            .AutoCompleteEvent = AddressOf AutoCompletion,
            .HeuristicsMode = True,
            .TabAtStartCompletes = True
        }

        R = RInterpreter.FromEnvironmentConfiguration(
            configs:=If(engineConfig.StringEmpty, ConfigFile.localConfigs, engineConfig)
        )
        R.strict = False

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

    Private Function AllSymbols() As IEnumerable(Of String)
        Dim globalEnv = R.globalEnvir
        Dim globalSymbols = globalEnv.EnumerateAllSymbols.JoinIterates(globalEnv.EnumerateAllFunctions)
        ' system internal hiddens
        Dim internals = Internal.invoke.getAllInternals
        Dim symbols As IEnumerable(Of String) = globalSymbols _
            .Select(Function(s) s.name) _
            .JoinIterates(internals.Select(Function(s) s.name)) _
            .Distinct

        Return symbols
    End Function

    Private Function AutoCompletion(s As String, pos As Integer) As Completion
        Dim prefix As String = s.Substring(0, pos)
        Dim ls As String()

        If prefix.StringEmpty Then
            ls = AllSymbols.ToArray
        Else
            ls = AllSymbols _
                .Where(Function(c) c.StartsWith(prefix, StringComparison.Ordinal)) _
                .Select(Function(c) c.Substring(pos)) _
                .ToArray
        End If

        Return New Completion(prefix, ls)
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
                    Await New RunScript(script) _
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
                result = REnv.TryCatch(
                    runScript:=Function() R.SetTaskCancelHook(Terminal.cts).Run(program),
                    debug:=R.debug
                )
            End If

            Return Rscript.handleResult(result, R.globalEnvir, program)
        End Function

    End Class
End Module
