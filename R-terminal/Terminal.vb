Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Terminal
Imports SMRUCC.Rsharp.Interpreter

Module Terminal

    Public Function RunTerminal() As Integer
        Dim ps1 As PS1 = PS1.Fedora12
        Dim R As New RInterpreter
        Dim exec As Action(Of String) =
            Sub(script)
                R.Evaluate(script)
            End Sub

        Call New Shell(ps1, exec).Run()

        Return 0
    End Function
End Module
