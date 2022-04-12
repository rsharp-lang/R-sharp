options(strict = FALSE);

i = list(a =1, b = [2,3,3], c = "chrs");

print("1. -------------------");
str(i);

i = [i];

print("2. -------------------");
str(i);

i = i[1];

print("3. -------------------");
str(i);
str(i$c == $"[a-z]+");

i = i[1];

print("4. -------------------");
str(i);