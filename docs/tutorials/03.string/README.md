[&lt; Back to index](../)

# String in ``R#``

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
# and then we could
# do sub-string matches by the given regex pattern:
let numbers = $"\d+"(text);
# [3] "" "123" "333"
```

The syntax of declare a regular expression in R# language is much simple, you just required put a ``$`` symbol in front of your pattern string, example as ``$"\d"`` is a regular expression literal in R# language. For make the R# code more readable, using the string interpolation for the literal of regular expression pattern string is not allowed, so the syntax of <code>$`\d+`</code> is illegal.

#### Regular Expression options?

There is no way to use literal syntax for create a new regular expression pattern object with tweaks of some options, such as ignore case, multiple line mode, etc. The regular expression object literal syntax in R# language is a kind of syntax sugar for the internal api ``base::regexp(pattern, options)``. So if you want to create a new regular expression object with tweaks of the pattern, please do following:

```R
# The regex pattern object will match all alphabets
# character with ignore case options.
let alphabets = base::regexp("[a-z]", "i");

# The example demo show above is not equals to the 
# object literal demo:
# The lowercase_alphabets literal object have the same 
# pattern input with the alphabets symbol. But due to
# the reason of missing i option in lowercase_alphabets 
# literal object, the regexp lowercase_alphabets can only 
# match the lower case alphabets.
let lowercase_alphabets = $"[a-z]";
```

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

The definition of the ``is pattern`` match test in R# could be: the regular expression matched result of the given string is equals to the original input string value. Such string pattern test is espacially useful in . The ``is pattern`` for the string pattern test should following this syntax rule: ``<string-text-to-test> (==|!=) [patterns]``, example as:

```R
# is pattern match test only allowes regular expression
["123", "55555", "234.999"] == $"\d+"
# [1] TRUE TRUE FALSE

["123", "55555", "234.999"] != $"\d+"
# [1] FALSE FALSE TRUE
```


