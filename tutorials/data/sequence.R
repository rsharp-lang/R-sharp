# The numeric sequence generator demo

print('An integer sequence');
print(1:10);

print('An integer sequence with offset 5');
print(1:100 step 5);

print('A numeric sequence with step 0.5');
print(1:10 step 0.5);

print('A numeric sequence with step 1.0');
print(1.0:10.0);

for(x in 1:5 step 0.5) {
	print(`x -> ${x}`);
}