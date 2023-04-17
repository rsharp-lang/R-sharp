Imports System
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.TypeScript

Module Program
    Sub Main(args As String())
        Dim ts = New TypeScriptLoader
        Dim script = ts.ParseScript("E:\GCModeller\src\R-sharp\test\jsTest\test1.js", GlobalEnvironment.defaultEmpty)

        Console.WriteLine("Hello World!")
    End Sub
End Module
