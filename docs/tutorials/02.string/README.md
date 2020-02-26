[&lt; Back to index](../)

# String in R#

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

### Regular Expression

```R
# Please notice that, the ``string`` type is a kind of 
# primitive type(character vector type) in R# language
let text as string = ["ABC", "123", "333"];
let numbers = $"\d+"(text);
# [3] "" "123" "333"
```

The syntax of declare a regular expression in R# language is much simple, you just required put a ``$`` symbol in front of your pattern string, example as ``$"\d"`` is a regular expression literal in R# language. For make the R# code more readable, using the string interpolation for the literal of regular expression pattern string is not allowed, so the syntax of <code>$`\d+`</code> is illegal.

### String pattern comparision

You can test of the string pattern in R# language in two kind of pattern expression: wildcard and regular expression. There are two kind of pattern match operator in R# language: like pattern and is pattern.

#### like pattern

The like pattern should following this syntax rule: ``<string-text-to-test> like [patterns]``, example as:

```R
# wildcards pattern match
["abc.txt", "a.txt", "b.R"] like "*.R"
# [1] FALSE FALSE TRUE

# regular expression pattern match
["abc.R", "a.txt", "B.R", "_a.R_"] like $"[a-z]\.R"
# [1] TRUE FALSE FALSE TRUE
```

#### is pattern

The is pattern should following this syntax rule: ``<string-text-to-test> (==|!=) [patterns]``, example as:



