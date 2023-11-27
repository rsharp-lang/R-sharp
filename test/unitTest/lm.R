const x = [1  1  1  1  1  1];
const cals = [0.05  0.1   0.2   0.5   1     2];
let f = lm(cals ~ x, data.frame(x = as.numeric(x), cals), weights = 1/(x^2));
let s = [f]::summary;
let ab = s$factors;

# str(s);
print(f);

print([f]::R2);
print(s$F);
print(ab);
print(s$"f(x)");