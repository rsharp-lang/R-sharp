# The value of the vector v comes from the 
# result of the for loop closure
let v as double = 

# for loop will produce a vector
# the vector element value is comes 
# from the last expression in 
# loop closure 
for(i in 10000:10000000 step 10) %dopar% {
   i;
}

# config the number display format
options(digits = 2, f64.format = "G");
# and then print the for loop result
print(v);