let add1 as function(x) {
	print(`x + 1 -> ${x+1}`);
}

for(i in 1:10 step 2.5) {
	add1(x = i);
}