# string wildcards pattern
#
# <string> like [wildcard pattern] 
#
print(["123.txt", "abc.R"] like "*.R");

# regular expression pattern
#
# <string> like [regexp pattern]
#
print(["123.txt", "abc.R"] like $"\d+\..+");

# string is (or not is) in given regular expression pattern
#
# <string> == [regexp pattern]
# <string> != [regexp pattern]
#
print(["123.txt", "1234.txt"] == $"\d{4}\.txt");
print(["123.txt", "1234.txt"] != $"\d{4}\.txt");

# regexp pattern match
#
# [regexp pattern](string)
#
let number.pattern = $"\d{4}";
print(number.pattern(["12144123@1231233", "333333333"]) :> str);
# or 
print($"\d{4}"(["12144123@1231233", "333333333"]) :> str);