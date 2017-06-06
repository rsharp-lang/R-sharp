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



###### Get/Set value

Get/Set property value keeps the same as the R language: 

```R
var names <- dataframe[, "name"];
dataframe[, "name"] <- new.names;
```

###### String

Add new string contact and string interploate feature for ``R#``:

```R
var name     <- first.name & " " & last.name;
# or
var my.name  <- "{first.name} {last.name}"; 
# sprintf function is still avaliable
var his.name <- sprintf("%s %s", first.name, last.name); 
```

###### Logical operators

+ and, andalso
+ or, orelse
+ not

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
   %+% <- function($, other) {
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

###### Simple external calls

Makes more easier for calls external command from CLI, apply a ``@`` operator on a string vector will makes an external system calls:

```R
var exitCode <- @"/bin/GCModeller/localblast /blastp /query \"{query.fasta}\" /subject \"{COG_myva}\" /out \"{COG_myva.csv}\"";
# or
var CLI <- "/bin/GCModeller/localblast /blastp /query \"{query.fasta}\" /subject \"{COG_myva}\" /out \"{COG_myva.csv}\"";
var exitCode <- @CLI;
```
