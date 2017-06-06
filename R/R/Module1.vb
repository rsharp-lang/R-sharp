Module Module1

    Sub Main()
        Call TokenIcer.Parse("
var x <- {1, 2, 3, 4, 5};
var x <- ""33333333"" & 33:ToString(""F2"");

if (x:Length <= 10) {
    println(x);
    
    test <- function(...) {
        var gg <- ...;
        var x  <- ...;
        var s  <- x & global$x;        
    }

    test(x = x, gg = x, s = x);
}

do while(TRUE andalso t == ""123 + 555"") {
    cat(""."");
}
").ToArray _
.GetSourceTree _
.SaveTo("x:\test.xml")
    End Sub
End Module
