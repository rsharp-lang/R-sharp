Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Terminal
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Package
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Terminal

    Public Function RunTerminal() As Integer
        Dim ps1 As New PS1("> ")
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(LocalPackageDatabase.localDb)
        Dim exec As Action(Of String) =
            Sub(script)
                Dim program As RProgram = RProgram.BuildProgram(script)
                Dim result = R.Run(program)

                If Not RProgram.isException(result) Then
                    If program.Count = 1 AndAlso program.isSimplePrintCall Then
                        ' do nothing
                        If DirectCast(program.First, FunctionInvoke).funcName = "cat" Then
                            Call Console.WriteLine()
                        End If
                    Else
                        Call Internal.base.print(result, R.globalEnvir)
                    End If
                End If
            End Sub

        Call Console.WriteLine("Type 'demo()' for some demos, 'help()' for on-line help, or
'help.start()' for an HTML browser interface to help.
Type 'q()' to quit R.
")
        Call R.LoadLibrary("base")
        Call R.LoadLibrary("utils")

        Call New Shell(ps1, exec) With {
            .Quite = "q()"
        }.Run()

        Return 0
    End Function

    ReadOnly echo As Index(Of String) = {"print", "cat", "echo"}

    <Extension>
    Private Function isSimplePrintCall(program As RProgram) As Boolean
        Return TypeOf program.First Is FunctionInvoke AndAlso DirectCast(program.First, FunctionInvoke).funcName Like echo
    End Function
End Module
