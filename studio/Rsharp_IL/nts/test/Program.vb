Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.TypeScript
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Program
    Sub Main(args As String())
        Dim ts = New TypeScriptLoader
        ' Dim script1 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test1.js", GlobalEnvironment.defaultEmpty)
        Dim script3 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test3.js", GlobalEnvironment.defaultEmpty)
        ' Dim script4 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test4.js", GlobalEnvironment.defaultEmpty)
        ' Dim script5 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test5.js", GlobalEnvironment.defaultEmpty)
        ' Dim script6 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test6.js", GlobalEnvironment.defaultEmpty)
        ' Dim script7 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\run_test.js", GlobalEnvironment.defaultEmpty)
        ' Dim script7 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\run_test.js", GlobalEnvironment.defaultEmpty)
        Dim script8 As RProgram = ts.ParseScript("/GCModeller\src\R-sharp\test\jsTest\test_for.js", GlobalEnvironment.defaultEmpty)

        Console.WriteLine("Hello World!")
    End Sub
End Module
