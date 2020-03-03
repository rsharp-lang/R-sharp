Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter

<Package("aaaaa")>
Module argumentSequencetest

    Dim R As New RInterpreter With {.debug = False}

    Sub Main()
        Call R.LoadLibrary(GetType(argumentSequencetest))

        ' Call R.Evaluate("test(1, 2)")
        Call R.Evaluate("test(5, c = 6, d = 'a999')")

        Pause()
    End Sub

    <ExportAPI("test")>
    Sub method1(a$, b$, Optional c$ = "111", Optional d$ = "222")
        Call Console.WriteLine({a, b, c, d}.GetJson)
    End Sub
End Module
