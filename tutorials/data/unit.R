let x as integer = [1024, 2, 3, 4, 5];
let s as integer = 30;

# create unit by R# activator
unit(x) = new unit(name = "kg");
unit(s) = new unit(name = "m");

let density = [weight, width] -> weight / (width ^ 2);

print("formula: ");
print(density);
print("result of the given formula is:");
print(density(x, s));

cat("\n\n\n");

density = x / (s^2);
print(density);
cat("\n");

# create unit by name directly
unit(x) <- "KM";
unit(s) <- "h";

let speed = x * s ^ -1;

print(speed);
cat("\n");

unit(x) <- "GB";
unit(s) <- "sec";

print(x /s);
