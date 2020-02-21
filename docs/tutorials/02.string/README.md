[&lt; Back to index](../)

# String in R#

### Regular Expression

```R
let text as string = ["ABC", "123", "333"];
let numbers = $"\d+"(text);
# [3] "" "123" "333"
```

> Please notice that, the primitive ``string`` character type is an vector type in R# 

### Literal a string

Like the R language, the string literal value can be wrapped by a paired ``"`` or ``'`` symbol, example like:

```R
"Hello world!"
'Hello world!'
```

### Interpolation of the string

The string interpolation syntax in R# language is a kind of new language feature compares with the R language. The syntax of the string interpolation in R# language is introduce from the typescript language, where the interpolation expression should be wrapped by a paired of <code>`</code> symbol, and the value expression for do string interpolation should be wrapped by ``${value}``. A simple example of the string interpolation syntax could be:

```R
`Hello ${["World", "User"]}!`
```

Where the string vector value ``["World", "User"]`` is the value interpolation for the example expression. And the example expression that show above is roughly equals to the ``sprintf`` in R language:

```R
sprintf("Hello %s!", c("World", "User"))
```