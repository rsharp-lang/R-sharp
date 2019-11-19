# Demo about error handling in R# script
#
# suppress keyword can make the interpreter ignore of the code exception
# and continue execute the code file after error was throw
# It is like the On Error Resume Next in VisualBasic.NET
# but have different:
# 1. On Error Resume Next in VB: affects all of the code block after
# 2. suppress in R#: only affects the current syntax token

let exception as function() {
    stop("just create a new exception!");
}

# will produce NULL without stop the whole script execution
let none = suppress exception();

print(none);

# script interpreter will stop at this line of code
let stop = exception();

# This expression will never be called,
# as the interpreter stop running at the code above...
print(stop);