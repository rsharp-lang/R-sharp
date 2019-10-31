# list is a dictionary object in .NET runtime
# 
# You can assign a key name in list member creation
# and if the key name is missing, then index value will be used

let l <- list(a = 1, b= 2, 5, c=33, 99);

print("The initialize value of the list value is:");
print(l);

# The lapply function is a function for process the list members
# the first parameter for lapply is a list or sequence
# and the second parameter for lapply is a lambda function
# lambda function should be declare with a -> or => symbol

l <- lapply(l, x => (x -2) * 100);

print("list value after lapply the lambda function is:");
print(l);


let power3 as function(x) {
	x^ 3;
}

let n = lapply(l, power3);

print("list value after power3 function calls:");
print(n);