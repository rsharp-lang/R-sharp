Imports SMRUCC.Python
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Module Programa
    Sub Main(args As String())

        Call forLoop()
        Call parseFile()

        Call parseFunction()
        Call parseHelloWorld()

    End Sub

    Sub forLoop()
        Dim forL = "
fruits = [""apple"", ""banana"", ""cherry""]

for x in fruits:
  print(x)

"

        Dim text As Rscript = Rscript.AutoHandleScript(forL)
        Dim py As Program = text.ParsePyScript

        Pause()
    End Sub

    Sub parseFile()
        ' Dim text As Rscript = Rscript.FromFile("E:\GCModeller\src\R-sharp\studio\test\hybridTest\base.py")
        Dim text As Rscript = Rscript.FromFile("D:\GCModeller\src\R-sharp\studio\test\hybridTest\ifTest.py")
        Dim py As Program = text.ParsePyScript

        Pause()
    End Sub

    Sub parseFunction()
        Dim func = "
def hello(x = [1,2,3], zzz = TRUE): 
   
   print(x)
   print(""hello world!"")
   x = zzz
   
   return x
    
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
