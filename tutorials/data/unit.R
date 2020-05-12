let x as integer = [1024,2,3,4,5];
let s as integer = 100;

# create unit by R# activator
unit(x) = new unit(name = "kg");
unit(s) = new unit(name = "m");

let density = x / (s ^ 2);

print(density);

# create unit by name directly
unit(x) = "KM";
unit(s) = "h";

let speed = x * s ^ -1;

print(speed);

unit(x) = "GB";
unit(s) = "sec";

print(x /s);
