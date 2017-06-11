Imports SMRUCC.Rsharp

Module SourceTrees

    Sub Main()
        Console.WriteLine(TokenIcer.Parse("
# Generic type variable
var x <- 123;").GetSourceTree)

        Console.WriteLine(TokenIcer.Parse("
# Type constraint variable
var x as integer <- {1, 2, 3};").GetSourceTree)

        Console.WriteLine(TokenIcer.Parse("
if(TRUE) {
    # blablabla
    println(""%s is TRUE"", TRUE);
}").GetSourceTree)

        Console.WriteLine(TokenIcer.Parse("
if(a == b) {
    println(""%s is equals to %s"", a, b);
} else if(a <= b) {
    println(""%s is less than or equals to %s"", a, b);
} else {
    println(""Not sure about this."");
}
").GetSourceTree)

        Pause()
    End Sub
End Module
