let x as integer = [1024,2,3,4,5];
let s as integer = 100;

unit(x) = new unit(name = "kg");
unit(s) = new unit(name = "m");

let speed = x / (s ^ 2);



# print("unit of the result vector is:");
# print(unit(speed));

# cat("\n\n\n\n\n");

# print("------------------------------------------------------");
# print("vector content:");
print(speed);
