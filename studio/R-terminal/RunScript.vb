Imports System.Threading
Imports SMRUCC.Rsharp.Interpreter
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Friend Class RunScript

    ReadOnly script As String
    ReadOnly R As RInterpreter

    Sub New(R As RInterpreter, script As String)
        Me.R = R
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