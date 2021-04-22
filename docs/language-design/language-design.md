# 1. R# language design

<!-- vscode-markdown-toc -->
* 1. [Code comments](#Codecomments)
* 2. [Variable](#Variable)
		* 2.1. [ Append Vector](#AppendVector)
* 3. [ Types](#Types)
	* 3.1. [ imports .NET type](#imports.NETtype)
	* 3.2. [unit type](#unittype)
* 4. [Get/Set value](#GetSetvalue)
* 5. [1.5. Function and lambda function](#Functionandlambdafunction)
* 6. [String](#String)
* 7. [Operators in R#](#OperatorsinR)
	* 7.1. [Logical operators](#Logicaloperators)
	* 7.2. [Dynamics Operator Binding](#DynamicsOperatorBinding)
		* 7.2.1. [User operator](#Useroperator)
	* 7.3. [pipeline operator](#pipelineoperator)
	* 7.4. [IN operator](#INoperator)
		* 7.4.1. [combine with ``Which`` operator](#combinewithWhichoperator)
		* 7.4.2. [Difference of ``in`` and ``between``](#Differenceofinandbetween)
* 8. [``[]`` bracket in R language](#bracketinRlanguage)
* 9. [IO operation](#IOoperation)
	* 9.1. [Simple external calls](#Simpleexternalcalls)
* 10. [Using tuple](#Usingtuple)
	* 10.1. [R object to tuple](#Robjecttotuple)
* 11. [Linq Query](#LinqQuery)
	* 11.1. [Query R dataframe](#QueryRdataframe)
	* 11.2. [Join two data source](#Jointwodatasource)

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

Variable in ``R#`` should be declared by a ``let``/``const`` keyword, and using ``<-`` or ``=`` operator for value initialize by a expression. If the variable declaration not follow by a value initialize expression, then by default its value is set to ``NULL``:

```R
let s <- "12345";
const x <- [1, 2, 3, 4, 5];

let matrix <- [
   [1, 2, 3],
   [4, 5, 6],
   [7, 8, 9]
];

let x;
# is equals to
let x <- NULL;
```

> + ``let`` keyword is allowed appears in a closure(function/if/loop, etc) body or top level scope
> + ``const`` keyword is almost doing the same thing as the ``let`` keyword it does, but the different of ``const`` keyword it does is mark the target symbol is readonly. Run ``lockBinding`` on target symbol automatically.

```R
let x = 9;

print(x);
# [1] 9
x <- FALSE;
print(x);
# [1] FALSE

const y = 9;

print(y);
#[1] 9
y <- FALSE;
# error, symbol y is readonly
```

Delcare a vector or matrix will no longer required of the ``c(...)`` function or ``matrix()`` function. Almost keeps the same as the VB.NET language it does:

```vbnet
Dim s = "12345"
Dim x = {1, 2, 3, 4, 5}
Dim m = {
   {1, 2, 3},
   {4, 5, 6},
   {7, 8, 9}
}
```

```R
let s = "12345";
let x = [1, 2, 3, 4, 5];
let m = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
];
```

By default, all of the primitive types in R# is an vector, and user defined type is a single value. So that in R#, ``integer`` means an integer type vector, ``char`` means a character type vector, or string value. ``string`` type in R# is a ``character`` vector.

In the traditional R language, you can using both ``=`` or ``<-`` operator for value assign, all of these two operator are both OK. But in ``R#`` language these two operator have slightly difference: value asssign using ``<-`` operator means ``ByVal``, and value assign using ``=`` operator means ``ByRef``:

```R
let a <- [1, 2, 3, 4, 5];
let b1, b2 as integer;

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

####  2.1. <a name='AppendVector'></a> Append Vector

You can using ``append()`` function for append a vector in R language, and in R# you can using both ``append()`` function and left shift ``<<`` operator for append a vector:

```R
v <- append(a, b)
## left shift means append b into vector a and then creates a new vector
v <- a << b
```

NOTE: As the ``R#`` language is not designed for general programming, the most usaged of ``R#`` language is used for cli/data scripting inside GCModeller environment, so ``<<`` and ``>>`` these two bit shift operator in VB.NET language is no longer means for bit operation any more in ``R#`` language. 

+ The ``<<`` operator is usually used for array push liked operation in ``R#`` language; 
+ And the ``>>`` operator is usually used for data file save operation.

##  3. <a name='Types'></a> Types

``R#`` language have several primitive type, **by default all of them are vector type**:

| primitive type in R | .NET type                 |
|---------------------|---------------------------|
| ``integer``         | **System.Int64** vector   |
| ``double``          | **System.Double** vector  |
| ``uinteger``        | **System.UInt64** vector  |
| ``string``          | **System.String** vector  |
| ``char``            | **System.Char** vector    |
| ``boolean``         | **System.Boolean** vector |

Generally, the R language is not designed as an OOP language, and the R# language is not designed as an OOP lnaguge too. But you can still declare the user type by using ``list()`` function, example like:

```R
# it works, but too verbose
let obj <- list();

obj$a <- 123;
obj$b <- "+++";

# it also works, but this statement is not elegant when you have 
# a lot of list property slot required to put into the R# list object
# variable.
let obj <- list(a = 123, b = "+++");

# using with for object property initialize
let obj <- list(list = [TRUE, TRUE, TRUE, FALSE], flag = 0) with {
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
let create_Foo as function() {
	return list() with {

		# please notice that, this operator required of the $name property in this user type
		# But there is no name in it at all????
		# Don't worried
		%in% <- function($, str) {
			return $name in str;
		}
	}
}

let using_my_name as function() {
	return create_Foo() with {
		$name <- "123";
	}
}
let using_selected_name as function(names as string) {
	return create_Foo() with {
		$name <- names;
	}
}

# The ``with{}`` closure which show above is equivalent to this code
# But this code function is too verbose
let using_selected_name as function(names as string) {
	let o <- create_Foo();
	o$name <- name;
	return o;
}
```

generally, the parameter in a R# function is generic type, so that a function its definition like:

```R
let test as function(x) {
    # ...
}
```

can accept any type you have input. but you can using the ``param as <type>`` for constraint the type to a specific type(currently the user type that produced by ``list()`` function is not supported by this type constraint feature):

```R
let test.integer as function(x as integer) {
    # the type constraint means the parameter only allow the integer vector type
    # if the parameter is a string vector, then the interpreter will throw exceptions.
}
```

###  3.1. <a name='imports.NETtype'></a> imports .NET type

you can use the ``new`` keyword for create the imported .NET type in R# language:

```R
let a = new <name>(...);
let b = new <name>() with {
    $a = ...;
    $b = ...;
    $c = $a + $b;
};
```

for implements such programming feature, then you should make sure about something:

1. the .NET object type should be public visible
2. target .NET object type should have one parameterless constructor
3. export type at the top of your package module.

###  3.2. <a name='unittype'></a>unit type

There is a unit type R# language feature can let you mark the numeric data

```R
let x as integer = [1024, 2, 3, 4, 5];
let s as integer = 30;

unit(x) <- "GB";
unit(s) <- "sec";

print(x /s);
```

##  4. <a name='GetSetvalue'></a>Get/Set value

Get/Set property value keeps the same as the R language: 

```R
let names <- dataframe[, "name"];
let new.names <- ["a", "b", "c"];

dataframe[, "name"] <- new.names;
```

##  5. <a name='Functionandlambdafunction'></a>1.5. Function and lambda function

For declare a function, use

```R
let add as function(a, b) {
    a + b;
}
```

If your function is simply enough, and didn't modify the environment, example like didn't have operation of variable value assignment. Then you should consider use the lambda function.
The lambda function is more light weight and elegant than the normal function. The lambda function just allows one expression in its body, and only allows one parameter.

```R
let add.lambda = [a,b] -> a+b;
```

##  6. <a name='String'></a>String

Add new string contact and string interploate feature for ``R#``, makes you more easier in the string manipulation:

```R
let first.name <- "a";
let last.name  <- "b";

let my.name  <- `${first.name} ${last.name}`; 
# sprintf function is still avaliable
let his.name <- sprintf("%s %s", first.name, last.name); 
```

##  7. <a name='OperatorsinR'></a>Operators in R#

###  7.1. <a name='Logicaloperators'></a>Logical operators

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

###  7.2. <a name='DynamicsOperatorBinding'></a>Dynamics Operator Binding

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

| Operator    | Description         |
|-------------|---------------------|
| ``+``       | add                 |
| ``-``       | substract           |
| ``*``       | multiply            |
| ``/``       | devide              |
| ``\``       | integer devide      |
| ``%``       | mod                 |
| ``^``       | power               |
| ``is``      | object equals       |
| ``like``    | object similarity   |
| ``in``      | in collection set   |
| ``which``   | index list for true |
| ``between`` | in a given range    |

####  7.2.1. <a name='Useroperator'></a>User operator

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

###  7.3. <a name='pipelineoperator'></a>pipeline operator

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

["foo = bar"]
:> replace("foo", "bar")
:> capitalize

# BAR = BAR
```

```R
let test1 as function(x) {
    # ...
}
let test2 as function(x, y) {
    # ...
}
let test3 as function(a) {
    # ...
}

# Doing the exactly the same as VisualBasic pipeline in R language:
let result <- ["hello world!"]
:> test1
:> test2(99)
:> test3
;

# or you can just using the R function in normal way, and it is too much complicated to read:
let result <- test3(test2(test1("hello world"), 99));
```

Note: Unlike the unix bash pipeline, operations can be keeps in the sample line, the R# pipeline syntax, require all of the pipeline content should be in different lines:

```R
# This is the correct pipeline syntax in R#
# Pipeline in multiple line mode will makes your code comment more elegant, and more easy to understand
list(a=123, b= TRUE, c="123")
:> rep(10)                      # replicate 10 times of the value from list functiuon
:> rbind()                      # rbind these replicated values as a dataframe
:> write.csv(file="./abc.csv")  # save the resulted data frame as csv file
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
:> namespace.1::func1()    # using the func1 in namespace.1 
:> namespace.2::func1()    # using the func1 in namespace.2
;
```

###  7.4. <a name='INoperator'></a>IN operator

The ``in`` operator means does the element in the target collection? returns a boolean vector for indicate exists or not exists.

```R
# in list
local booleans <- name in names(obj);
# in range
# means x >= min and x <= max
local booleans <- x between [min, max];
```

####  7.4.1. <a name='combinewithWhichoperator'></a>combine with ``Which`` operator 

The ``which`` operator gets the index of the value ``TRUE`` in a boolean vector:

```R
let x <- [1, 2, 3, 4, 5];
let indices.true <- which x between [min, max];
```

####  7.4.2. <a name='Differenceofinandbetween'></a>Difference of ``in`` and ``between``

the ``in`` operator is apply for collection set element exact match and the ``between`` operator is apply for the numeric range test,
example as:

```R
# in test each element in x collection
# for exact match in collection b
#
# each x in b? 
[1, 2, 2.5, 3, 4, 5] in [2, 3]
# result:
# [1] FALSE TRUE FALSE TRUE FALSE FALSE

# between test each element in x collection
# is in a given value range?
[1, 2, 2.5, 3, 4, 5] between [2, 3]
# result:
# [1] FALSE TRUE TRUE TRUE FALSE FALSE
```

##  8. <a name='bracketinRlanguage'></a>``[]`` bracket in R language

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
let prot.fasta = "/home/biostack/sample.fasta";
let [exitCode, std_out] <- @`makeblastdb -in "${prot.fasta}" -dbtype prot`;
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
l[&1];
# [1] 11
```

Please notice that the term ``"1"`` is totaly differently with ``%1%``, as the term ``"1"`` means accessor by property name, and the term ``%1%`` means accessor by vector element index.

##  9. <a name='IOoperation'></a>IO operation

```R
## You can using right shift operator for write data into file
x >> [options]
x >> path
```

The right shift write operator is based on the data type of object ``x``:

1. If type of ``x`` is a dataframe or matrix, then ``write.csv``/``write.table`` function will be called.
2. If type of ``x`` is generic type, then ``save`` function will be called.
3. If type of ``x`` is vector of the primitive types, then the data it will be saved as ``json``/``txt``/``csv`` file.

> The file format is depends on the file extension shuffix, and this feature is only works for ``rule 1`` and ``rule 3``, ``rule 2`` for generic type is only can be saved in ``rda`` binary file.

Example as:

```R
let vector = [1,2,3,4,5,6];

vector >> "./index.json";
vector >> "./index.csv";
vector >> "./index.txt";

let matrix = 
    [|1,2,3|,
     |4,5,6|,
     |8,8,8|];

colnames(matrix) <- |"A","B","C"|;
matrix >> "./matrix.csv"; # A csv file will be generated.
matrix >> "./matrix.txt"; # A tsv file will be generated.
```

###  9.1. <a name='Simpleexternalcalls'></a>Simple external calls

The ``R#`` language makes more easier for calling external command from CLI, apply a ``@`` operator on a string vector will makes an external system calls:

```R
let [exitCode, stdout] <- @'/bin/GCModeller/localblast /blastp /query "{query.fasta}" /subject "{COG_myva}" /out "{COG_myva.csv}"';

# or makes it more clear to read
let CLI <- '/bin/GCModeller/localblast /blastp /query "{query.fasta}" /subject "{COG_myva}" /out "{COG_myva.csv}"';
let [exitCode, stdout] <- @CLI;
```

##  10. <a name='Usingtuple'></a>Using tuple

Tuple enable the R function returns multiple value at once:

```R
# this R function returns multiple value by using tuple:
let tuple.test as function(a as integer, b as integer) {
    [a, b, a ^ b];
}

let tuple.test2 as function(a, b) {
    # list is also works fine
    list(a, b, c = a^ b);
}

# and you can using tuple its member as the normal variable
let [a, b, c] <- tuple.test(3, 2);

[a, b, c] = tuple.test2(100, 1); 

if (a == 3) {
    c <- c + a + b;
    # or using pipeline
    c <- [a, b, c]
         :> sum
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

###  10.1. <a name='Robjecttotuple'></a>R object to tuple

You can naturally convert the object as tuple value. The member in the tuple their name should matched the names in an object, so that you can doing something like this example in ``R#``:

```R
let obj <- list() with {
    $a <- 333;
    $b <- 999;
}
# the tuple its member name should match the property name in you custom object type
# no order required in your tuple declaration: 
let [a, b] <- obj;
```

But, wait, if the property in an object is not a valid identifier name in ``R#``? Don't worried, you can using alias:

```R
let obj <- list() with {
    $"112233+5" <- 999;
    $x <- 1;
}
let [a as "112233+5", b as "x"] <- obj;
```

The tuple feature is espacially useful in operates the dataframe:

```R
let d <- data.frame(
    a = [   1,    2,     3],
    b = [ "a",  "g",   "y"],
    t = [TRUE, TRUE, FALSE]);

# in a for loop, the tuple its member value is the cell value in dataframe
for([a, b, c as "t"] in d) {
    println("%s = %s ? (%s)", a, b, c);
}

# 1 = a ? (TRUE)
# 2 = g ? (TRUE)
# 3 = y ? (FALSE) 

# if directly convert the dataframe as tuple, 
# then the tuple member its value is the column value in the dataframe 
let [a, b, booleans as "t"] <- d;

a;
# [3] 1 2 3

b;
# [3] "a" "g" "y"

booleans;
# [3] TRUE TRUE FALSE
```

If the tuple is applied on a for loop, then it means convert each row in dataframe as tuple, or just applied the tuple on the var declaring, then it means converts the columns in dataframe as the tuple, so that the variable in tuple is a vector with nrows of the dataframe.

##  11. <a name='LinqQuery'></a>Linq Query

The ``R#`` language have native supports of the Linq query syntax like VisualBasic.NET language:

```R
let result = FROM x as double      # query object
             IN [a,b,c,d,e]        # data source
             WHERE predicate(x)    # subset
             ORDER BY x DESCENDING # option pipeline
             SKIP 100              # option pipeline
             TAKE 20               # option pipeline
             ;
```

Linq query in R# follows the rule for generate output result based on different data source:

| from data source | produce     | equals to                          | note                                                                                  |
|------------------|-------------|------------------------------------|---------------------------------------------------------------------------------------|
| list             | list        | lapply                             | always produce a new list                                                             |
| vector           | vector/list | sapply/lapply                      | based on the projection result: single is vector and compounds data will produce list |
| dataframe        | dataframe   | dataframe subset/projection syntax | linq query is working as SQL, always produce a new dataframe table                    |

###  11.1. <a name='QueryRdataframe'></a>Query R dataframe

The linq query of the dataframe object in ``R#`` is by row, and the column names that used in query shoud be selected in the ``FROM`` closure, example as:

```R
# query dataframe
const demo = data.frame(x = x(), y = y(), z = z());

# make a table subset by rows
let subset = FROM [x, y, z] 
             IN demo
             WHERE predicate(x, y, z)
             ORDER BY z + x DESCENDING
             ;
# will produce a subset output
# x, y, z

# make a table projection by and subset by row
let proj = FROM [x, y, z]
           IN demo
           WHERE predicate(x, y, z)
           SELECT x, z # project x and z field produce a new dataframe subset
           ;
# will produce a table projection output
# x, z
```

###  11.2. <a name='Jointwodatasource'></a>Join two data source

As the mysql language, linq in R# language has the join function on two data source too:

```R
# make query after join two table
const employee = data.frame(
    ID = [1,2,3,4,5,6],
    Name = ["Preety", "Priyanka", "Anurag", "Pranaya", "Hina", "Sambit"],
    AddressId = [1, 2, 0,0,5,6]
);

const address = data.frame(
    ID = [1, 2, 5 , 6], 
    AddressLine = ["AddressLine1","AddressLine2","AddressLine5","AddressLine6"]
);

# the join operation
let result = FROM [ID, Name, AddressId] IN employee
             JOIN [ID, AddressLine] IN address
             ON employee.AddressId == address.ID
             WHERE employee.ID > 5
             ;
# will produce a join table output
# employee.ID, Name, AddressId, address.ID, AddressLine
```