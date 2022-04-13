let add1 = x -> x + 1;
let add2 = x -> x + 2;
let add5 = x -> x + 5;
let addWith as function(x, y) {x + y};

let x as integer <- [99,8,5, 11];
let [a,b,c,d] = x :> [add1, add2, add5, addWith(9)];

print(a);
print(b);
print(c);
print(d);