## Value Expression

The value expression is the very basic element in R# language: almost every word that you input will produce values, example like:

```R
# This statement will produce value 55 with integer type
# The value that produce by this statement will stored in 
# a system preserved variable ``.Last``
var x as integer <- 55;
```

There is a kind of special value expression in R# language: **closure expression**.

## Closure Expression

There are two kind of closure expression in R# language: primitive closure and user closure(function).

### Primitive closure

The primitive closure make up the basic flow control, like: ``if...elseif...else``, ``try...catch``, ``switch...case``, ``for``, ``repeat`` and ``do...while``. 

#### Branch control

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

#### Loop control

##### ``for``

##### ``repeat``

##### ``do...while``

### Primitive Closure as Value Expression

All of the primitive closure will using the last value expression in the closure code as the return value of that closure, example as:

```
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

#### Notice on the loop control

All of the loop control will produce a value collection, for example:

```
# The syntax maybe looks weired, but it is legal in R#
var sum.list <- for(x in 1:5) {
    x + 99;
}

# or you can using .Last system preserved variable:
for (x in 1:5) {
    # 
    x + 99;
}

var sum.list as integer <- .Last;
```

### User closure

The user closure just have one type: user function, example like:

```R
user.closure <- function(args) {

}
```

#### Function declare

```R
# allow function overloads
function funcName(args) {

}

# variable form not allows overloads
var funcName <- function(args) {

}
```

#### The main closure

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
