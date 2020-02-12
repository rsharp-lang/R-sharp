# Module Concept in R#

The most important different between the R# and R language is that the variable should be declared at first in R# script, but in R language does not. As the R# language required the variable should be declared at first, so it is more easily to understand how the R# closure it works. Once you have understand the R# cosure, then you will known everything about the R# module.

Consider such a simple function in R#

```R
let count as function() {
    # declare a new variable in the environment 
    # of the closure object count function:
    let hit as integer = 0;

    # utilize the variable
    # this is a lambda function
    # and the last expression in R# will be used as the 
    # closure result value.
    #
    # So this count function returns a callable function without
    # any parameter input
    [] -> hit = hit + 1;
}

let counter = count();

print(counter());
# [1] 1
print(counter());
# [1] 2
print(counter());
# [1] 3
```

As the hit variable is only declared in ``count`` function body, so that every code in the ``count`` function is reference to the ``hit`` variable from the ``count`` function closure.