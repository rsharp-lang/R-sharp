Module Module1

    Sub Main()
        Call TokenIcer.Parse("
# run commandline using @ operator in R
var prot.fasta = ""/home/biostack/sample.fasta"";
var [exitCode, std_out] <- @""makeblastdb -in \""{prot.fasta}\"" -dbtype prot"";

test.integer <- function(x as integer) {
    # the type constraint means the parameter only allow the integer vector type
	# if the parameter is a string vector, then the interpreter will throw exceptions.
}

var name <- first.name & "" "" & last.name;
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
