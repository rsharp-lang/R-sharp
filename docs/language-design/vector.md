Generally, the vector style programming in ``R#`` just enables on the ``R#`` primitive types.

## Declare Vector

You can declare vector using ``[...]``, example as:

```R
let x as integer <- [1, 2, 3, 4, 5];
```

Vector its element type can be all of the R# primitive type: integer, numeric, character, string, boolean. For non-numeric type, like character, string, boolean, the numeric value was converted at first, like convert the string or character as the factor type. For boolean type, R# convert value ``TRUE`` to value 1, ``FALSE`` was convert to 0.

The vector declare expression can be split into multiple line, such as:

```R
let x as double <-
[
    123.003,
    124.596,
    125.365
];
```

This will makes much easier for you to put code comment on the vector elements.

### Declare a matrix

Matrix in ``R#`` is a kind of vector collection, and you can declare the matrix by using tuple like syntax in R#:

```R
# matrix declare can be break into multiple lines, this will makes the code comment more easier
# and clear on recognize the matrix structure.
var matrix as integer <- 
[|1,2,3|,
 |4,5,6|,
 |7,8,9|];
```

NOTE: all of the vector element in a matrix should be equals on length, or a runtime exception will throw. If you are using identifier in this matrix declare, then the R# parser will unable to recognize the syntax:

```
var a,b,c;
a <- b <- c <- |1,2,3|; 

# This expression will throw a runtime exception, as the left side probably is a integer vector/matrix, 
# but the right side is a tuple declaration expression, tuple can not be directly convert to a vector/matrix
var matrix as integer <- [a,b,c];

# This value expression will also throw an runtime exception, due to the reason of an identifier xyz
# was present in the expression, the interpreter can be recognized it as matrix nor tuple.
[|1,2,3|,
 |5,5,5|,
 xyz
]
```

## Vector math

Only allowes on the integer, numeric and uinteger types.

### abs()

abs function just allow one identifer, if the expression have more than one identifier then it will be recognizsed as vector declare.

```R
# vector declare
let x as double <- [1,2,3,4];
# abs function
let abs.value <- |x|;
```

### norm()

norm function just allow one identifier

```R
let x as double <- [1,2,3,4];
let norm.value <- ||x||;
```

