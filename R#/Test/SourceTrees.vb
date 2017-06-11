Imports SMRUCC.Rsharp

Module SourceTrees

    Sub Main()
        Console.WriteLine(TokenIcer.Parse("
# Generic type variable
var x <- 123;").GetSourceTree)

        Console.WriteLine(TokenIcer.Parse("
# Type constraint variable
var x as integer <- {1, 2, 3};").GetSourceTree)

        Pause()
    End Sub
End Module
