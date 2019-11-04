let echo = x -> print(x + 5);

# create pipeline with a single parameter function

[555,666,777,888,999] :> echo;


let print3 as function(x, y, z) {

print(x ^ y);
print(x * z - y);

}

# by default is the first parameter
[1,2,3]+5 :> print3([9,9,3], 500);

# for specific the parameter z to received the pipeline data
[z = 999] :> print3(1,2);