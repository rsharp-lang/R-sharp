# R# Closure Expression

<!-- vscode-markdown-toc -->
* 1. [Value Expression](#ValueExpression)
* 2. [Closure Expression](#ClosureExpression)
	* 2.1. [Primitive closure](#Primitiveclosure)
		* 2.1.1. [Branch control](#Branchcontrol)
		* 2.1.2. [Loop control](#Loopcontrol)
	* 2.2. [Primitive Closure as Value Expression](#PrimitiveClosureasValueExpression)
		* 2.2.1. [Notice on the loop control](#Noticeontheloopcontrol)
		* 2.2.2. [Loop and operator](#Loopandoperator)
	* 2.3. [User closure](#Userclosure)
		* 2.3.1. [Function declare](#Functiondeclare)
		* 2.3.2. [The main closure](#Themainclosure)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='ValueExpression'></a>Value Expression

The value expression is the very basic element in R# language: almost every word that you input will produce values, example like:

```R
# This statement will produce value 55 with integer type
# The value that produce by this statement will stored in 
# a system preserved variable ``.Last``
var x as integer <- 55;
```

There is a kind of special value expression in R# language: **closure expression**.

##  2. <a name='ClosureExpression'></a>Closure Expression

There are two kind of closure expression in R# language: primitive closure and user closure(function).

###  2.1. <a name='Primitiveclosure'></a>Primitive closure

The primitive closure make up the basic flow control, like: ``if...elseif...else``, ``try...catch``, ``switch...case``, ``for``, ``repeat`` and ``do...while``. 

####  2.1.1. <a name='Branchcontrol'></a>Branch control

##### ``if...elseif...else``

```R
if (logical_condition) {
    for_true();
} elseif (logical_condition) {
    for_true();
} else {
    for_false();
}
```

##### ``try...catch``

```R
try {
    # balbalbal
} catch(e) {

}
```

##### ``switch...case``

The ``switch...case`` is very similar to the ``if...elseif...else``:

```R
switch(value_expression) {

    case condition1 {
    
    }
    
    case condition2 {
    
    }
    
    case else {
    
    }
}
```

####  2.1.2. <a name='Loopcontrol'></a>Loop control

##### ``for``

##### ``repeat``

##### ``do...while``

###  2.2. <a name='PrimitiveClosureasValueExpression'></a>Primitive Closure as Value Expression

All of the primitive closure will using the last value expression in the closure code as the return value of that closure, example as:

```R
# The if closure can using the last value expression last_value or last_value2 
# as its returns value based on the condition logical_condition.
var value <- if(logical_condition) {
    # blablabla ...
    last_value;
} else {
    # blablabla
    last_value2;
}

# so if a if closure just have one condition?
# the false branch will return NULL as default if it is not presented:
var valueOrNULL <- if(logical_condition) {
    # blablabla ...
    last_value;
}
```

####  2.2.1. <a name='Noticeontheloopcontrol'></a>Notice on the loop control

All of the loop control will produce a value collection, for example:

```R
# The syntax maybe looks weired, but it is legal in R#
var sum.list as integer <- for(x in 1:5) {
    x + 99;
}

# or you can using .Last system preserved variable:
for (x in 1:5) {
    # 
    x + 99;
}

var sum.list as integer <- .Last;

sum.list;
# [5] 100 101 102 103 104
```

As we have known that all of the loop closure will produce a value collection, so that please be carefull when you are using a possible infinite loop control like ``repeat`` and ``do...while``. As the infinite loop may break your computer's memory and exhaust a lot of memory for stores the closure values that produced from each loop iteration. 

```R
# This infinite loop will fill up your computer's memory, and shutdown your system.
repeat {

    NULL;
}

var nothing <- .Last;
```

If you don't want the loop control closure produced values, then you can using the ``suppress`` option:

```R
options(suppress = TRUE);

# So that this infinite loop just stuck you program at here, 
# and your computer's memory will not be fill up.
repeat {

    NULL;
}
```

Or you can just apply ``suppress`` in separate infinite loop, as the ``options()`` will disable the value produce in the current environment and current environment's child stack.

```R
var sum.list as integer <- for(x in 1:5) {
    x + 99;
    suppress;
}

sum.list;
# NULL

var i as integer = 0;

sum.list <- repeat {

    if (i < 10) {
        i +=1;
    } else {
        break;
    }

    i ^ 2;

    suppress;
}

sum.list;
# NULL
```

####  2.2.2. <a name='Loopandoperator'></a>Loop and operator

As the loop closure is a kind of 

###  2.3. <a name='Userclosure'></a>User closure

The user closure just have one type: user function, example like:

```R
user.closure <- function(args) {

}
```

####  2.3.1. <a name='Functiondeclare'></a>Function declare

**Please remenber that there is no real function in R# language!** The function in R# language is a kind of user defined closure that can produced values as other value expression. All of the function that you declard is kind of variable value, so that there is no function overloads in R# language, because the function variable with same name will overrides each other, the function declare in R# is a procedure of variable value assign.

```R
# This is how you are doing a function declare:
# variable form not allows overloads
var funcName <- function(args) {

}

# variable value assign
var x <- 123;
x;
# [1] 123

x <- FALSE;
x;
# [1] FALSE

# function declare just a variable value assign procedure, 
# so that, there is no function overloads in R# language.
var func <- function(args) {
    # ...
}
func;
# function(args) {
#    # ...
# }
# <environment: .globalEnvir::func>

# variable func its value was overrides by new closure value
func <- function(x, y) { x+y; }
func;
# function(x, y) {
#    x + y;    
# }
# <environment: .globalEnvir::func>

# func is a normal variable in generic type, 
# so that it can be function closure or vector.
func <- |TRUE, FALSE, FALSE|;
func;
# [3] TRUE FALSE FALSE
```

> Why not allowed function overloads:
> 1. function is a kind of data type in R# (closure type).
> 2. Unlike VisualBasic language, R# is not a kind of strong type language, so that function overloads will makes user confused when they are calling a function that may have different version which is distinct from parameter type.

But you can declare function with the same name in different R# namespace, and then use these function by add namespace prefix, example like:

```R
var x <- namespace1::function1();
var y <- namespace2::function2(x);
```

####  2.3.2. <a name='Themainclosure'></a>The main closure

Every R# script is a very large implicit function: main.

```R
# This is legal
main <- function(commandline) {
    # your script content goes here
}
```

But writing a R# script without a main function declare probably much more convenient. Whatever, on your choice.

```R
# your script content goes here
```
