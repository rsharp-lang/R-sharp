Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.TypeScript

Module Program
    Sub Main(args As String())
        Dim ts = New TypeScriptLoader
        Dim script = ts.ParseScript("D:\GCModeller\src\R-sharp\test\jsTest\test3.js", GlobalEnvironment.defaultEmpty)

        Console.WriteLine("Hello World!")
    End Sub
End Module
