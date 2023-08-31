strs = ["aa","bbb","ccc","ddd2","eeXe","fff","ggg","ddd","eee","ssss","qqqq","!!!!!!","asdasdasdasdadasd","eeeee","dfgdfgdfgdgdf","rtyrt","ppp","o"];
t0 = now();
n = 500;

for(i in 1:n) {
    null <- strs in strs;
    NULL;
}

t1 = now() - t0;
t0 = now();

index2 <- index_of(strs);

for(i in 1:n) {
    null <- strs in index2;
    NULL;
}

t2 = now() - t0;

print(t1);
print(t2);


strs = as.character(1:500000);

print(strs);

t0=now();

null <- strs in strs;

t1 = now() - t0;
t0 = now();

null <- strs in index_of(strs);

t2 = now() - t0;

print(t1);
print(t2);