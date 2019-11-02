let echo = x -> print(x + 5);

# create pipeline with a single parameter function

[555,666,777,888,999] :> echo;


let print3 as function(x, y, z) {

print(x ^ y);
print(x * z - y);

}


[1,2,3] :> print2([9,9,3], 500);