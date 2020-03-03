#Region "Microsoft.VisualBasic::ed8c002ba8958fcfb6fa1dcac8f74080, R-terminal\Terminal.vb"

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

Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Terminal
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.System.Configuration
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Terminal

    Dim R As RInterpreter

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
        Call R.LoadLibrary("stats")

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
        Dim error$ = Nothing
        Dim program As RProgram = RProgram.BuildProgram(script, [error]:=[error])
        Dim result As Object

        If Not [error].StringEmpty Then
            result = REnv.Internal.debug.stop([error], R.globalEnvir)
        Else
            result = REnv.TryCatch(Function() R.Run(program))
        End If

        Call Rscript.handleResult(result, R.globalEnvir, program)
    End Sub
End Module
