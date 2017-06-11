Imports SMRUCC.Rsharp

Module SourceTrees

    Sub Main()
        Console.WriteLine(TokenIcer.Parse("
# Generic type variable
var x <- 123;").GetSourceTree)


        Pause()
    End Sub
End Module
