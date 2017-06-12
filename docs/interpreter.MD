# How the interpreter works?

```
script -> [TokenIcer] -> SourceTree -> ListOf RStatements -> ExecuteMachine
```

## Statements in R#

#### Variable Declares

```R
# Generic type variable
var {variable_name|tuple} <- {initial_expression};
```

By default the R variable is generic type, but the user can constraint the variable to a specific primitive type. This type constraint feature only works on the R primitive type(which means the type constraint will not works on the list type or tuple type in R#), which their type codes can be found in this declare:

```vbnet
''' <summary>
''' The R# types
''' </summary>
Public Enum TypeCodes As Byte

    ''' <summary>
    ''' Object type in R#
    ''' </summary>
    [generic] = 0

    ''' <summary>
    ''' Class type in R#
    ''' </summary>
    [list] = 10

    ''' <summary>
    ''' <see cref="Integer"/> vector
    ''' </summary>
    [integer] = 100
    ''' <summary>
    ''' <see cref="ULong"/> vector
    ''' </summary>
    [uinteger]
    ''' <summary>
    ''' <see cref="Double"/> numeric vector
    ''' </summary>
    [double]
    ''' <summary>
    ''' <see cref="String"/> vector
    ''' </summary>
    [string]
    ''' <summary>
    ''' <see cref="Char"/> vector
    ''' </summary>
    [char]
    ''' <summary>
    ''' <see cref="Boolean"/> vector
    ''' </summary>
    [boolean]

End Enum
```

Example:

```R
# R variable with primitive type constraint
# type = {integer, uinteger, double, string, char, boolean}
var {variable} as {type} <- {initial_expression};
```

#### ``if`` branch statement

```R
if ({bool_expression}) {
    # branch statements for TRUE
} else {
    # branch statements for FALSE
    # branch FALSE can be optional
}
```

A more complex ``if`` statement:

```R
if ({bool_expression}) {
    # branch statements for TRUE = {bool_expression}
} else if ({bool_expression2}) {
    # branch statements for TRUE = {bool_expression2}
} else {
    # branch statements for FALSE
    # branch FALSE can be optional
}
```

#### ``function`` declares statement

```R
{function_name} <- function(parameter1 [as {type}], [...]) {
    # function body closure
}
```

The function parameters, can be:

+ (generic type) only an object identifier name: ``{identifier}``
+ (primitive type) identifier with type constraint: ``{identifier} as {type}``
+ (parameter list) generic list parameters: ``...``
+ (optional parameter) parameter have its default value is the parameter is not specific: ``{identifier} [as {type}] = {value_expression}``

Supports of the optional parameter scripting:

```R
test <- function(a, b, c = (a + b)^2) {
    return [a ^ 3, -b * c, c];
}
```

#### ``for`` loop statement

The R language its ``for`` loop is equals to the ``For Each`` Loop in VisualBasic:

```R
for (i in 1:50) {

}

for (n in [1:50, 0.1]) {

}

var x <- [1:50,0.01];

for (d in x) {

}
```

```vbnet
For Each i In (1, 50).Range
Next

For Each n In (1, 50).Range(step:=0.1)
Next

Dim x = (1, 50).Range(0.01)

For Each d In x
Next
```

#### ``do`` loop statement

```R
do(TRUE) {

}

do {

} while(TRUE);
```