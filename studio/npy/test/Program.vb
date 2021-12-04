Imports System
Imports SMRUCC.Python.Language

Module Program
    Sub Main(args As String())

        Call parseHelloWorld()

    End Sub

    Sub parseHelloWorld()
        Dim hello = "print(""Hello World!"")"
        Dim scanner As New PyScanner(hello)
        Dim tokens = scanner.GetTokens.ToArray

        Pause()
    End Sub

End Module
