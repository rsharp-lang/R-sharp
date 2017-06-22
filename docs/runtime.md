# The R# runtime

In R# language, each source script is run in a function closure mode, which means each R# script just like a program statement in a virtual function which its named ``Main``, and this function have a parameter list which user can read the arguments from commandline.

```R
main <- function(...) {
    # R script statement running at here
    # inside a virtual function main closure
}
```

## Running a R# script

For example, assume that we have a R# script like:

```R
var message as string = ...;
var pages as integer  = ...;

printf("[%s] got %s pages.", message, pages);

return [message, pages]; 
```

### From ``source()``

```R
source <- function(path, ...) {
    # basically, the R# version source() function accept a required parameter
    # which is named path, for read the script content from a text file on local or internet resource.
    # and an optional parameter list
}
```

```R
# due to the reason of user not specific the arguments, so that 
# the variables message and pages in this script will be value NA
# so that the returns tuple result will be [NA, NA]
var result <- source("script.R");

result;
# tuple(result)
# $message = NA
# $pages = NA

# due to the reason of user have specific the arguments, so that 
# the variable in the script will no longer be NA
result <- source("script.R", message="Hello world!", pages=999);

result;
# tuple(result)
# $message = "Hello world!"
# pages = 999

# and of course, if you didn't want using the script return result, just call the source function:
source("script.R", message="Hello world!", pages=999);
```

### From commandline

You also can running the R# script directly from commandline, example as running a R# script from bash shell:

```bash
# If your R# script didn't required of the commandline arguments, then directly using a simple filename, like:
R ./script.R

# otherwise, the parameters should follow by a function calls like syntax in your commandline.
# the commandline argument name is the corresponding variable name in your R script
# and of course, the white space is not allowed in a variable name
# each commandline argument should seperated by at least one white space
# and the value was specific in syntax like: variable=value 
R ./script.R(message="Hello world!" pages=999);
```

As you have notice that, the example R script have a value ``return`` statement. So did this value ``return`` statement working in a commandline mode? No, it does not, but actually the ``return`` statement in commandline mode just partly functional in your script: break the function but did not returns the values. 

If you runing your script from ``source()`` function, then this value ``return`` statement will fully functional in your script: break the function and returns the values at the same time. Using the ``source()`` function invoke your script, just like invoke a virtual ``main()`` function and get its returns value, just like the Reflection operation on a function member in VisualBasic. 

![](./images/stackframes.png)
