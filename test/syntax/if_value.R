let flag as boolean = ?"--switch";

# due to the reason of if closure could
# produce value, so that we can use 
# the if closre as function parameter
# and pipe with the next function call

# the real code for produce value could be more complex
if (flag) {
	100;
} else {
	500;
}
# [1] 500
:> print;
