#Region "Microsoft.VisualBasic::144741f3b5fda91ce8f10003cda2524e, studio\R-terminal\CLI\Syntax.vb"

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

    '   Total Lines: 43
    '    Code Lines: 37 (86.05%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (13.95%)
    '     File Size: 1.53 KB


    ' Module CLI
    ' 
    '     Function: SyntaxText
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.My
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Interpreter
Imports RlangScript = SMRUCC.Rsharp.Runtime.Components.Rscript
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Partial Module CLI

    <ExportAPI("--syntax")>
    <Description("Show syntax parser result of the input script.")>
    <Usage("--syntax /script <script.R;*.py>")>
    Public Function SyntaxText(args As CommandLine) As Integer
        Dim script$ = args <= "/script"

        If script.ExtensionSuffix("R") Then
            Dim Rscript As RlangScript = RlangScript.FromFile(script)
            Dim error$ = Nothing
            Dim debugMode As Boolean = args.IsTrue("--debug")
            Dim program As RProgram = RProgram.CreateProgram(
                Rscript:=Rscript,
                [error]:=[error],
                debug:=debugMode
            )

            If Not [error].StringEmpty Then
                Call Log4VB.Println([error], ConsoleColor.Red)
                Call VBDebugger.WaitOutput()
            Else
                Call Console.WriteLine(program.ToString)
            End If
        Else
            Dim R As New RInterpreter

            Call R.Imports({"VisualStudio"}, "devkit.dll")
            Call R.Invoke("VisualStudio::inspect", script, R.globalEnvir)
        End If

        Return 0
    End Function
End Module
