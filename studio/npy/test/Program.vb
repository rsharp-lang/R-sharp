Imports System
Imports SMRUCC.Python.Language

Module Program
    Sub Main(args As String())
        Call parseFunction()
        Call parseHelloWorld()

    End Sub

    Sub parseFunction()
        Dim func = "
def hello(): 

   print(""hello world!"")
   x = message

"
        Dim scanner As New PyScanner(func)
        Dim tokens = scanner.GetTokens.ToArray

        Pause()
    End Sub

    Sub parseHelloWorld()
        Dim hello = "print(""Hello World!"")"
        Dim scanner As New PyScanner(hello)
        Dim tokens = scanner.GetTokens.ToArray


        Pause()
    End Sub

End Module
