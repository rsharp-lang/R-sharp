# R# language design

<!-- vscode-markdown-toc -->
* 1. [Code comments](#Codecomments)
* 2. [Variable](#Variable)
		* 2.1. [Append Vector](#AppendVector)
* 3. [Types](#Types)
* 4. [Get/Set value](#GetSetvalue)
* 5. [String](#String)
* 6. [Operators in R#](#OperatorsinR)
	* 6.1. [Logical operators](#Logicaloperators)
	* 6.2. [Dynamics Operator Binding](#DynamicsOperatorBinding)
		* 6.2.1. [User operator](#Useroperator)
	* 6.3. [pipeline operator](#pipelineoperator)
	* 6.4. [IN operator](#INoperator)
		* 6.4.1. [combine with ``Which`` operator](#combinewithWhichoperator)
* 7. [``[]`` bracket in R language](#bracketinRlanguage)
* 8. [IO operation](#IOoperation)
	* 8.1. [Simple external calls](#Simpleexternalcalls)
* 9. [Using tuple](#Usingtuple)
	* 9.1. [R object to tuple](#Robjecttotuple)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Codecomments'></a>Code comments

```R
## This is code comments, it just only allow single line comments.
```

##  2. <a name='Variable'></a>Variable

Variable in ``R#`` should be declared by a ``local``/``global`` keyword, and using ``<-`` or ``=`` operator for value initialize by a expression. If the variable declaration not follow by a value initialize expression, then by default its value is set to ``NULL``:

```R
local s <- "12345";
local x <- |1, 2, 3, 4, 5|;
local matrix <-
  [|1, 2, 3|,
   |4, 5, 6|,
   |7, 8, 9|];

local x;
# is equals to
local x <- NULL;
```

> + ``local`` keyword is only allowed appears in a closure(function/if/loop, etc) body
> + ``global`` keyword is not allowed used in any closure body for variable decalred.

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

By default, all of the primitive types in R# is an vector, and user defined type is a single value. So that in R#, ``integer`` means an integer type vector, ``char`` means a character type vector, or string value. ``string`` type in R# is a ``character`` vector.

In the traditional R language, you can using both ``=`` or ``<-`` operator for value assign, all of these two operator are both OK. But in ``R#`` language these two operator have slightly difference: value asssign using ``<-`` operator means ``ByVal``, and value assign using ``=`` operator means ``ByRef``:

```R
local a <- |1, 2, 3, 4, 5|;

local b1, b2;

b1 <- a;  # ByVal
b2 =  a;  # ByRef

a[6] = 99;

b1;
# ByVal means clone the source values, so that when the source have been change, 
# the cloned variable its value will not changed too. 
# ByVal means values remains the same as the source:
# [1]  1  2  3  4  5

b2;
# ByRef means reference to the source memory location pointer, 
# so that when the source have been changed, then the reference variable will be changed too. 
# ByRef means reference of the source memory.
# [1]  1  2  3  4  5 99

# So that if you have change the ByRef b2 its value, the source is also changed:
b2[1] = 88;
b2;
# [1] 88  2  3  4  5 99

a;
# [1] 88  2  3  4  5 99

b1[2] = -10;
b1;
# [1]  1  -10  3  4  5

a;
# [1] 88  2  3  4  5 99
```

####  2.1. <a name='AppendVector'></a>Append Vector

You can using ``append()`` function for append a vector in R language, and in R# you can using both ``append()`` function and left shift ``<<`` operator for append a vector:

```R
v <- append(a, b)
## left shift means append b into vector a and then creates a new vector
v <- a << b
```

NOTE: As the ``R#`` language is not designed for general programming, the most usaged of ``R#`` language is used for cli/data scripting in GCModeller, so ``<<`` and ``>>`` this two bit shift operator in VB.NET language is no longer means for bit operation any more. The ``<<`` operator is usually used for array push liked operation in ``R#`` language; And the ``>>`` operator is usually used for data file save operation.

##  3. <a name='Types'></a>Types

``R#`` language have several primitive type, **by default all of them are vector type**:

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
# it works, but too verbose
var obj <- list();

obj$a <- 123;
obj$b <- "+++";

# it also works, but this statement is not elegant when you have 
# a lot of list property slot required to put into the R# list object
# variable.
var obj <- list(a = 123, b = "+++");

# using with for object property initialize
var obj <- list(list = |TRUE, TRUE, TRUE, FALSE|, flag = 0) with {
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

Using the ``with{}`` closure you can also using for dynamics add/modify property value in a more brief way:

```R
create_Foo <- function() {
	return list() with {

		# please notice that, this operator required of the $name property in this user type
		# But there is no name in it at all????
		# Don't worried
		%in% <- function($, str) {
			return $name in str;
		}
	}
}

using_my_name <- function() {
	return create_Foo() with {
		$name <- "123";
	}
}
using_selected_name <- function(names as string) {
	return create_Foo() with {
		$name <- names;
	}
}

# The ``with{}`` closure which show above is equivalent to this code
# But this code function is too verbose
using_selected_name <- function(names as string) {
	var o <- create_Foo();
	o$name <- name;
	return o;
}
```

generally, the parameter in a R# function is generic type, so that a function its definition like:

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

##  4. <a name='GetSetvalue'></a>Get/Set value

Get/Set property value keeps the same as the R language: 

```R
var names <- dataframe[, "name"];
dataframe[, "name"] <- new.names;
```

##  5. <a name='String'></a>String

Add new string contact and string interploate feature for ``R#``, makes you more easier in the string manipulation:

```R
var name     <- first.name & " " & last.name;
# or
var my.name  <- "$first.name $last.name"; 
# sprintf function is still avaliable
var his.name <- sprintf("%s %s", first.name, last.name); 
```

##  6. <a name='OperatorsinR'></a>Operators in R#

###  6.1. <a name='Logicaloperators'></a>Logical operators

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

###  6.2. <a name='DynamicsOperatorBinding'></a>Dynamics Operator Binding

Allows you binding operator on your custom type in dynamics way when you create a user objec from ``list()`` function:

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

####  6.2.1. <a name='Useroperator'></a>User operator

In the R language, you can define a user operator, example like:

```R
`%NOT_IS%` <- function(x, y) x != y;
 
1 %NOT_IS% 1
# [1] FALSE

1 %NOT_IS% 2
# [1] TRUE
```

This user operator declare just enabled on the binary operator, and the unary operator does not. but in R# language, you can also enable the unary operator, example like:

```R
`%pp%` <- function(x) (x + 10)^2;

var y as integer <- 10;
var x <- %pp% y;

x;
# [1] 400
```

###  6.3. <a name='pipelineoperator'></a>pipeline operator

Extension caller chain in VisualBasic is also named as function pipeline

```vbnet
<Extension> Function test1(x) 
End Function

<Extension> Function test2(x, y) 
End Function

<Extension> Function test3(a) 
End Function

Dim result = "hello world!" _
    .test1 _ 
    .test2(99) _ 
    .test3
```

All of the R function which have at least one parameter can be using in pipeline mode, using ``|`` as the pipeline operator:

```bash
# pipeline in linux bash
ps -ef | grep R.exe
```

and you can do this pipeline programming in ``R#``

```R
# application foo print its content output to standard output on the console 
# and then calling the replace function, at last capitalize all 
# of the string result

"foo = bar" 
|replace("foo", "bar") 
|capitalize

# BAR = BAR
```

```R
test1 <- function(x) {
}
test2 <- function(x, y) {
}
test3 <- function(a) {
}

# Doing the exactly the same as VisualBasic pipeline in R language:
var result <- 

"hello world!" 
|test1() 
|test2(99) 
|test3()
;
    
# or you can just using the R function in normal way, and it is too much complicated to read:
var result <- test3(test2(test1("hello world"), 99));
```

Note: Unlike the unix bash pipeline, operations can be keeps in the sample line, the R# pipeline syntax, require all of the pipeline content should be in different lines:

```R
# This is the correct pipeline syntax in R#
# Pipeline in multiple line mode will makes your code comment more elegant, and more easy to understand
list(a=123, b= TRUE, c="123")
|rep(10)                      # replicate 10 times of the value from list functiuon
|rbind()                      # rbind these replicated values as a dataframe
|write.csv(file="./abc.csv")  # save the resulted data frame as csv file
;

# The pipeline example is equals to these code in R:
x <- list(a=123, b= TRUE, c="123");
x <- rep(x, 10);
x <- rbind(x);
x <- write.csv(x, file="./abc.csv");

# Invalid syntax example
# The R# interpreter can not recognized it as the pipeline, if all of the pipeline operation in the same line.
# And it is not so easy to add code comments for each function calls if all of the function are 
# in same line:
list(a=123, b= TRUE, c="123")|rep(10)|rbind|write.csv(file="./abc.csv");
```

In VisualBasic, the function pipeline required user imports all of the namespace for the extension function. But in R# function this is not reuqired, and you can reference the package namespace in your R function pipeline, example as:

```R
# assume we have two function with the same name: func1, but in different namespace
# so we can apply these two function in pipeline mode, like

foo_value
|namespace.1::func1()    # using the func1 in namespace.1 
|namespace.2::func1()    # using the func1 in namespace.2
;
```

###  6.4. <a name='INoperator'></a>IN operator

The ``in`` operator means does the element in the target collection? returns a boolean vector for indicate exists or not exists.

```R
# in list
var booleans <- name in names(obj);
# in range
# means x >= min and x <= max
var booleans <- x in [min, max];
```

####  6.4.1. <a name='combinewithWhichoperator'></a>combine with ``Which`` operator 

The ``which`` operator gets the index of the value ``TRUE`` in a boolean vector:

```R
var x <- |1, 2, 3, 4, 5|;
var indices.true <- which x in [min, max];
```

##  7. <a name='bracketinRlanguage'></a>``[]`` bracket in R language

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

``[min:max,step]``

```R
if (mz in [mz.min:mz.max]) {
    # range generator only allows numeric type
}
```

tuple variable:

```R
# run commandline using @ operator in R
var prot.fasta = "/home/biostack/sample.fasta";
var [exitCode, std_out] <- @'makeblastdb -in "{prot.fasta}" -dbtype prot';
```

``list`` element accessor:

The R# list element accessor is different with the R language, example as R language:

```R
l <- list(a = 11, b = 22, c = TRUE);

l[["a"]];
# [1] 11

l[[1]];
# [1] 11
```

R# try to make it more simple, example as:

```R
l <- list(a = 11, b = 22, c = TRUE) with {
	$"1" <- |3, 4, 5, 6|;
};

# access list element by name
l["a"];
# [1] 11

l["1"];
# [4] 3 4 5 6

# access list element by index
l[%1%];
# [1] 11
```

Please notice that the term ``"1"`` is totaly differently with ``%1%``, as the term ``"1"`` means accessor by property name, and the term ``%1%`` means accessor by vector element index.

##  8. <a name='IOoperation'></a>IO operation

```R
## You can using right shift operator for write data into file
x >> [options]
x >> path
```

The right shift write operator is based on the data type of x:

1. If type of x is dataframe or matrix, then ``write.csv`` function will be called.
2. If type of x is generic type, then ``save`` function will be called.

###  8.1. <a name='Simpleexternalcalls'></a>Simple external calls

The ``R#`` language makes more easier for calling external command from CLI, apply a ``@`` operator on a string vector will makes an external system calls:

```R
var [exitCode, stdout] <- @'/bin/GCModeller/localblast /blastp /query "{query.fasta}" /subject "{COG_myva}" /out "{COG_myva.csv}"';

# or makes it more clear to read
var CLI <- '/bin/GCModeller/localblast /blastp /query "{query.fasta}" /subject "{COG_myva}" /out "{COG_myva.csv}"';
var [exitCode, stdout] <- @CLI;
```

##  9. <a name='Usingtuple'></a>Using tuple

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
    c <- |a, b, c|
         |sum()
	 ;
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

###  9.1. <a name='Robjecttotuple'></a>R object to tuple

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
    a = |   1,    2,     3|,
    b = | "a",  "g",   "y"|,
    t = |TRUE, TRUE, FALSE|);

# in a for loop, the tuple its member value is the cell value in dataframe
for([a, b, c as "t"] in d) {
    println("%s = %s ? (%s)", a, b, c);
}

# 1 = a ? (TRUE)
# 2 = g ? (TRUE)
# 3 = y ? (FALSE) 

# if directly convert the dataframe as tuple, 
# then the tuple member its value is the column value in the dataframe 
var [a, b, booleans as "t"] <- d;

a;
# [3] 1 2 3

b;
# [3] "a" "g" "y"

booleans;
# [3] TRUE TRUE FALSE
```

If the tuple is applied on a for loop, then it means convert each row in dataframe as tuple, or just applied the tuple on the var declaring, then it means converts the columns in dataframe as the tuple, so that the variable in tuple is a vector with nrows of the dataframe.
