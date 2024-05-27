let seed = "A":"Z";
let offset = runif(900000, 1, length(seed));

let d = data.frame(char = seed[offset], data = TRUE, num = 555);

let t0 = now();

d= groupBy(d, "char");

let t1 = now();

print(t1-t0);
# 20240523 old
# [1] 00:00:06
# 20240523 new method
# [1] 00:00:02
# stop();

for(part in d) {
    print(part, max.print = 6);
    stop();
}