Generally, the vector style programming in ``R#`` just enables on the ``R#`` primitive types.

## Declare Vector

You can declare vector using ``|...|``, example as:

```R
var x as integer <- |1, 2, 3, 4, 5|;
```

Vector its element type can be all of the R# primitive type: integer, numeric, character, string, boolean. For non-numeric type, like character, string, boolean, the numeric value was converted at first, like convert the string or character as the factor type. For boolean type, R# convert value ``TRUE`` to value 1, ``FALSE`` was convert to 0.

The vector declare expression can be split into multiple line, such as:

```R
var x as double <-
|
    123.003,
    124.596,
    125.365
|;
```

This will makes much easier for you to put code comment on the vector elements.

## Vector math

Only allowes on the integer, numeric and uinteger types.

### abs()

```R
var x as double <- |1,2,3,4|;
var abs.value <- |x|;
```

### norm()

```R
var x as double <- |1,2,3,4|;
var norm.value <- ||x||;
```

