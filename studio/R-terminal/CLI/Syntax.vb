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