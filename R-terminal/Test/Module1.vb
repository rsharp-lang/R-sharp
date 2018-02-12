#Region "Microsoft.VisualBasic::6e3b929ac3af58b9835c921d957b6390, R-terminal\Test\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.Language
Imports SMRUCC.Rsharp.Library.Internal
Imports SMRUCC.Rsharp.Runtime

Module Module1

    Sub Main()

        ' Call Module2.BenchmarkTest()

        Dim dbl As New PrimitiveTypes.numeric

        Dim result As Object

        With New RInterpreter
            '   Call Interpreter.Evaluate("3+[x];", New NamedValue(Of Object)("x", 10))
            Call .globalEnvir.Closures.Add("list", Function(args) base.list(args))

            Call .globalEnvir.Push("x", 12344444, TypeCodes.integer)
            Call .globalEnvir.Push("y", -12344449, TypeCodes.integer)
            Call .globalEnvir.Push("Z", {-10, 110}, TypeCodes.integer)
            Call .globalEnvir.Push("t", True, TypeCodes.boolean)

            result = .Evaluate("var [t.a as a,t.b as b, t.c as y] <- list(a=not TRUE, b = [x] ^ -3 , y = |y , 555, 9|);")

            result = .Evaluate("var v as integer <- 123;")
            result = .Evaluate("v <- Z + (x+y)*2;")
            result = .Evaluate("var b as boolean <- t;")

            result = .Evaluate("var vector.test as double <- |1,2| * (|v| + ||Z||);  # vector |1,2| multiply the sum vector that produced by abs value of vector y add numeric value vector norm result ||x||.")
            result = .Evaluate("var norm.test <- ||Z||;")
            result = .Evaluate("var abs.test <- |Z|;")
            result = .Evaluate("var logical.vector as boolean <- |TRUE, TRUE, TRUE, FALSE, FALSE, FALSE|;")
            result = .Evaluate("var not.test <- not logical.vector;")
            result = .Evaluate("var and.test <- logical.vector and TRUE;")
            result = .Evaluate("var or.test <- logical.vector or TRUE;")
            result = .Evaluate("var not.test2 <- not or.test;")
            result = .Evaluate("var or.test2 <- (logical.vector or FALSE) * 10;")

            Call Console.WriteLine()
            Call Console.WriteLine()


            Call .PrintMemory(Console.Out)


            result = .Evaluate("var f <- function(x) { x ^2; }")
            result = .Evaluate("var f2 <- function(x,y) x+y;")

            Pause()

            Dim str$ = result.GetJson(True)

            Call str.__DEBUG_ECHO


            Pause()

        End With



        result = New RInterpreter().Evaluate("var [a,b] <- (1+2)*3;")



        Call TokenIcer.Parse("(1+2)*3;").GetSourceTree.SaveTo("../design/sourceTree\math-expression.XML")


        Pause()

        Call TokenIcer.Parse("
var m <- {
   {1, 2, 3},
   {4, 5, 6},
   {7, 8, 9}
};").GetSourceTree.SaveTo("../design/sourceTree\matrix.xml")
        Call TokenIcer.Parse("
var d <- data.frame(
    a = {1, 2, 3},
    b = {""a"", ""g"", ""y""},
    t = {TRUE, TRUE, FALSE});").GetSourceTree.SaveTo("../design/sourceTree\parameters.xml")
        Call TokenIcer.Parse("var x as integer <- {1,2,3,4,5};").GetSourceTree.SaveTo("../design/sourceTree\vector.xml")
        Call TokenIcer.Parse("var x as integer <- func(a=""233"") + 5 * (2+33) * list() with {
    $a <- 123;
    $b <- ""hello world"";
};").GetSourceTree.SaveTo("../design/sourceTree\function.xml")

        Pause()

        Call TokenIcer.Parse("
var d <- data.frame(
    a = {1, 2, 3},
    b = {""a"", ""g"", ""y""},
    t = {TRUE, TRUE, FALSE});

var gg <- a(33) + b(99, zzz.ZZ= 88);

# in a for loop, the tuple its member value is the cell value in dataframe
for([a, b, c as ""t""] in d) {
    println(""%s = %s ? (%s)"", a, b, c);
}

# if directly convert the dataframe as tuple, 
# then the tuple member is value is the column value in the dataframe 
var [a, b, booleans as ""t""] <- d;

# this R function returns multiple value by using tuple:
tuple.test <- function(a as integer, b as integer) {
    return [a, b, a ^ b];
}

# and you can using tuple its member as the normal variable
var [a, b, c] <- tuple.test(3, 2);

if (a == 3) {
    c <- c + a + b;
    # or using pipeline
    c <- {a, b, c} | sum;
}

var g <- ""test"";

test <- function(g as integer) {
    # just like the VisualBasic language, you can using [] bracket 
    # for eliminates the object identifier conflicts in R language.
    # string contact of the parameter g with global variable [g]
    return g:ToString(""F2"") & [g];
}

var x <- {1, 2, 3, 4, 5};
var indices.true <- which x in [min, max];

test1 <- function(x) {
}
test2 <- function(x, y) {
}
test3 <- function(a) {
}

# Doing the exactly the same as VisualBasic pipeline in R language:
var result <- ""hello world!"" 
    |test1 
    |test2(99) 
    |test3;
# or you can just using the R function in normal way, and it is much complicated to read:
var result <- test3(test2(test1(""hello world""), 99));

# binding operator only allows in the with closure in the object declare statement
var me <- list() with {
   %+%  <- function($, other) {
   }
   %is% <- function($, other) {
   }
}

# and then using the operator

var new.me    <- me + other;
var predicate <- me is other;

if (not me is him) {
    # ......
}

if (x <= 10 andalso y != 99) {
    # ......
} else if(not z is null) {
    # ......
}

var names <- dataframe[, ""name""];
dataframe[, ""name""] <- new.names;

var m <- {
   {1, 2, 3},
   {4, 5, 6},
   {7, 8, 9}
};

var obj <- list();

# using with for object property initialize
var obj <- list() with {
    $a <- 123;
    $b <- ""+++"";
}

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
.SaveTo("../design/\sourceTree.xml")
    End Sub
End Module
