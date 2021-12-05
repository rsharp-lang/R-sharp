Imports SMRUCC.Python
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Module Programa
    Sub Main(args As String())
        Call parseFunction()
        Call parseHelloWorld()

    End Sub

    Sub parseFunction()
        Dim func = "
def hello(): 

   print(""hello world!"")
   x = message
    
f = x -> print(x)
"
        Dim scanner As New PyScanner(func)
        Dim tokens = scanner.GetTokens.ToArray

        Dim text As Rscript = Rscript.FromText(func)
        Dim py As Program = text.ParsePyScript

        Pause()
    End Sub

    Sub parseHelloWorld()
        Dim hello = "print(""Hello World!"")"
        Dim scanner As New PyScanner(hello)
        Dim tokens = scanner.GetTokens.ToArray


        Pause()
    End Sub

End Module
