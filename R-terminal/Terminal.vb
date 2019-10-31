Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Terminal
Imports SMRUCC.Rsharp.Interpreter

Module Terminal

    Public Function RunTerminal() As Integer
        Dim ps1 As New PS1("> ")
        Dim R As New RInterpreter
        Dim exec As Action(Of String) =
            Sub(script)
                R.Evaluate(script)
            End Sub

        Call Console.WriteLine("Type 'demo()' for some demos, 'help()' for on-line help, or
'help.start()' for an HTML browser interface to help.
Type 'q()' to quit R.
")
        Call New Shell(ps1, exec).Run()

        Return 0
    End Function
End Module
