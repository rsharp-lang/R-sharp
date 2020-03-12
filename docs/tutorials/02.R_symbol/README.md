# Symbol object in ``R#``

## Declare a new symbol

A symbol in R# language is a concept which is corresponding to a variable in other programming language. In R language you can use a symbol without declare it explicitly, but the R# not allowes such implicit declaration on create a new symbol, which means all of the symbol that you used in your script must declared first. There are two key words that can be used for declare a new symbol, ``let`` and ``const``:

```R
# declare a mutable symbol
let x as double;
# declare a readonly symbol
const y as string;
```

From the code demo that show above, we've found that the ``let`` and ``const`` keyword their usage keeps the same, but produce different effect on the symbol access in your script code:

+ ``let`` keyword create a new symbol and mark it as mutable on its value.
+ ``const`` keyword create a new symbol and mark it as readonly on its value.

Actually, you can get the same effect in the traditional R language with the function ``lockBinding`` like:

```R
y <- NULL;
lockBinding("y");
```

And from the demo code show above, you can notice that the ``const`` keyword is doing the ``lockBinding`` on the new symbol actually:

```R
# const y as string = "readonly";
# is equals to the code show below:
let y as string = "readonly";
lockBinding("y");

# you also can unlock the symbol which is 
# declared by the const keyword:
const a as integer = [1, 2, 3, 4];
unlockBinding("a");
a <- 999;
```

## Tuple in ``R#``

The tuple syntax in R# allowes your R# function returns multiple symbol, example as:

```R
let returnsMultiple as function() {
    # demo for returns multiple symbol
    list(a = FALSE, b = "yes", c = [1,2,3,4,5]);
}

# this is what you are doing in current R# language
let [a, b, c] = returnsMultiple();

print(a);
# [1] FALSE
print(b);
# [1] "yes"
print(c);
# [1] 1 2 3 4 5

# so if your are not declare a tuple, then you can still get a list object in R#
let tmp <- returnsMultiple();

print(tmp);
# $a
# [1] FALSE
# $b
# [1] "yes"
# $c
# [1] 1 2 3 4 5
```

Such language feature is much convient when compares with the traditional R language:

```R
returnsMultiple <- function() {
    # demo for returns multiple symbol
    list(a = FALSE, b = "yes", c = c(1,2,3,4,5));
}

# this is what you are doing in current R language
tmp <- returnsMultiple();
a <- tmp$a;
b <- tmp$b;
c <- tmp$c;

print(a);
# [1] FALSE
print(b);
# [1] "yes"
print(c);
# [1] 1 2 3 4 5
```

### Syntax suger tips

The R# language is not allowes declares multiple symbol in one expression, such as

```R
# syntax error
let a, b, c;
~~~~~~~~~~~~
```

If you want to declares multiple symbols in one expression, you should use the tuple syntax:

```R
let [a, b, c];
```

## ByRef or ByVal

There are two symbol that you can use for assign value to a given symbol in R language, they are both the same in R language, example as:

```R
a <- 9;
b = 99;
```

But these two symbol for doing value assign in R# have different effects: ``<-`` means value assign(byval), and ``=`` means value reference(byref), example as:

```R
a <- 9;
# byval: do value assign
b <- a;

a[2] <- 8;
print(a);
# [1] 9 8
print(b);
# [1] 9

# byref: do assign of the value object reference
b = a;
b[3] <- 7;
a[4] <- 6;
print(a);
# [1] 9 8 7 6
print(b);
# [1] 9 8 7 6
```

> Please note that, the byref and byval is only affect the **primitive vector type and ``list`` type** in R# language. 