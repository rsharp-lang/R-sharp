> [WARNING] This project is a work in progress and is not recommended for production use.

The ``R#`` language its syntax is original derived from the ``R`` language, but with more modernized programming styles. The ``R#`` language its interpreter and .NET compiler is original writen in VisualBasic language, with native support for the .NET runtime.

## R# language design

###### Code comments

```R
## This is code comments, it just only allow single line comments.
```

###### Variable

Variable in ``R#`` should be declared by ``var`` keyword, and its value assign is force using ``<-`` operator:

```R
var s <- "12345";
var x <- {1, 2, 3, 4, 5};
var m <- {
   {1, 2, 3},
   {4, 5, 6},
   {7, 8, 9}
};
```

Delcare a vector or matrix will no longer required of the ``c(...)`` function or ``matrix`` function. Almost keeps the same as the VisualBasic language it does:

```vbnet
Dim s = "12345"
Dim x = {1, 2, 3, 4, 5}
Dim m = {
   {1, 2, 3},
   {4, 5, 6},
   {7, 8, 9}
}
```

###### Types

``R#`` language have several primitive type:

|primitive type in R|.NET type                |
|-------------------|-------------------------|
|``integer``        |**System.Int64** vector  |
|``double``         |**System.Double** vector |
|``uinteger``       |**System.UInt64** vector |
|``string``         |**System.String** vector |
|``char``           |**System.Char** vector   |
|``boolean``        |**System.Boolean** vector|

Generally, the R language is not designed as an OOP language, and the R# language is not designed as an OOP lnaguge too. But you can still declare the user type by using ``list()`` function, example like:

```R
var obj <- list();

# using with for object property initialize
var obj <- list() with {
    $a <- 123;
    $b <- "+++";
}
```

Using ``with{}`` closure can makes the property initialize at the same time when you create your user type by using ``list()`` function. Just like what you does in VisualBasic: 

```vbnet
Dim obj As New <userType> With {
    .a = 123,
    .b = "+++"
}
```

generally, the parameter in a function is generic type, so that a function its definition like:

```R
test <- function(x) {
}
```

can accept any type you have input. but you can using the ``param as <type>`` for constraint the type to a specific type(currently the user type that produced by ``list()`` function is not supported by this type constraint feature):

```R
test.integer <- function(x as integer) {
    # the type constraint means the parameter only allow the integer vector type
    # if the parameter is a string vector, then the interpreter will throw exceptions.
}
```

###### Get/Set value

Get/Set property value keeps the same as the R language: 

```R
var names <- dataframe[, "name"];
dataframe[, "name"] <- new.names;
```

###### String

Add new string contact and string interploate feature for ``R#``, makes you more easier in the string manipulation:

```R
var name     <- first.name & " " & last.name;
# or
var my.name  <- "{first.name} {last.name}"; 
# sprintf function is still avaliable
var his.name <- sprintf("%s %s", first.name, last.name); 
```

###### Logical operators

The ``R#`` language using the VisualBasic logical operator system, as the ``&`` operator is conflicts with the string contact and ``|`` operator is conflicts with the pipeline operator.

+ ``&&`` replaced by ``and``, ``andalso``
+ ``||`` replaced by ``or``, ``orelse``
+ ``!`` replaced by ``not``

```R
if (x <= 10 andalso y != 99) {
    # ......
} else if(not z is null) {
    # ......
}
```

###### Operator binding

Allows you bind operator on your custom type:

```R
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
```

Allows user operator

|Operator |Description          |
|---------|---------------------|
|``+``    | add                 |
|``-``    | substract           |
|``*``    | multiply            |
|``/``    | devide              |
|``\``    | integer devide      |
|``%``    | mod                 |
|``^``    | power               |
|``is``   | object equals       |
|``like`` | object similarity   |
|``in``   | collection set      |
|``which``| index list for true |

###### pipeline operator

Extension caller chain in VisualBasic is also named as function pipeline

```vbnet
<Extension> Function test1(x) 
End Function

<Extension> Function test2(x, y) 
End Function

<Extension> Function test3(a) 
End Function

Dim result = "hello world!" 
    .test1 
    .test2(99) 
    .test3
```

All of the R function which have at least one parameter can be using in pipeline mode, using ``|`` as the pipeline operator:

```R
test1 <- function(x) {
}
test2 <- function(x, y) {
}
test3 <- function(a) {
}

# Doing the exactly the same as VisualBasic pipeline in R language:
var result <- "hello world!" 
    |test1 
    |test2(99) 
    |test3;
# or you can just using the R function in normal way, and it is much complicated to read:
var result <- test3(test2(test1("hello world"), 99));
```

###### IN operator

```R
# in list
var booleans <- name in names(obj);
# in range
var booleans <- x in [min, max];
```

###### combine with ``Which`` operator 

```R
var x <- {1, 2, 3, 4, 5};
var indices.true <- which x in [min, max];
```

###### ``[]`` bracket in R language

Global variable:

```R
var g <- "test";

test <- function(g as integer) {
    # just like the VisualBasic language, you can using [] bracket 
    # for eliminates the object identifier conflicts in R language.
    # string contact of the parameter g with global variable [g]
    return g:ToString("F2") & [g];
}
```

Range generator:

```R
if (mz in [mz.min, mz.max]) {
    # range generator only allows numeric type
}
```

tuple variable:

```R
# run commandline using @ operator in R
var prot.fasta = "/home/biostack/sample.fasta";
var [exitCode, std_out] <- @"makeblastdb -in \"{prot.fasta}\" -dbtype prot";
```

###### Simple external calls

The ``R#`` language makes more easier for calling external command from CLI, apply a ``@`` operator on a string vector will makes an external system calls:

```R
var [exitCode, stdout] <- @"/bin/GCModeller/localblast /blastp /query \"{query.fasta}\" /subject \"{COG_myva}\" /out \"{COG_myva.csv}\"";

# or makes it more clear to read
var CLI <- "/bin/GCModeller/localblast /blastp /query \"{query.fasta}\" /subject \"{COG_myva}\" /out \"{COG_myva.csv}\"";
var [exitCode, stdout] <- @CLI;
```

###### Using tuple

Tuple enable the R function returns multiple value at once:

```R
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
```

```vbnet
Dim tuple_test = Function(a As Integer, b As Integer)
                     Return (a, b, a ^ b)
                 End Function
Dim x As (a, b, c) = tuple_test(3, 2)

If x.a = 3 Then 
    ' using pipeline
    Dim c = {x.a, x.b, x.c}.Sum
End If
```

###### R object to tuple

You can naturally convert the object as tuple value. The member in the tuple their name should matched the names in an object, so that you can doing something like this example in ``R#``:

```R
var obj <- list() with {
    $a <- 333;
    $b <- 999;
}
# the tuple its member name should match the property name in you custom object type
# no order required in your tuple declaration: 
var [a, b] <- obj;
```

But, wait, if the property in an object is not a valid identifier name in ``R#``? Don't worried, you can using alias:

```R
var obj <- list() with {
    $"112233+5" <- 999;
    $x <- 1;
}
var [a as "112233+5", b as "x"] <- obj;
```

The tuple feature is espacially useful in operates the dataframe:

```R
var d <- data.frame(
    a = {1, 2, 3},
    b = {"a", "g", "y"},
    t = {TRUE, TRUE, FALSE});

# in a for loop, the tuple its member value is the cell value in dataframe
for([a, b, c as "t"] in d) {
    println("%s = %s ? (%s)", a, b, c);
}

# if directly convert the dataframe as tuple, 
# then the tuple member is value is the column value in the dataframe 
var [a, b, booleans as "t"] <- d;
```

If the tuple is applied on a for loop, then it means convert each row in dataframe as tuple, or just applied the tuple on the var declaring, then it means converts the columns in dataframe as the tuple, so that the variable in tuple is a vector with nrows of the dataframe.
