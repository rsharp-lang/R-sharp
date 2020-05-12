let x as integer = [1024,2,3,4,5];
let s as integer = 100;

unit(x) = new unit(name = "kg");
unit(s) = new unit(name = "m");

let density = x / (s ^ 2);

print(density);

unit(x) = new unit(name = "km");
unit(s) = new unit(name = "h");

let speed = x * s ^ -1;

print(speed);
