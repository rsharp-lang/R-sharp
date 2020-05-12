let x as integer = [1,2,3,4,5];
let s as integer = 100;

unit(x) = new unit(name = "MB");
unit(s) = new unit(name = "sec");

let speed = x / s;

print(unit(speed));
print(speed);
