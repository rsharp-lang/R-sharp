let add1 as function(x) {
	print(`x + 1 -> ${x+1}`);
}

const all = for(i in 1:10 step 2.5) {
	add1(x = i);
}

print("all contents:");
print(all);

# break test of the for loop
for(i in 1:10) {
	break;
	print("this message should never display!");
}

for(i in 1:10) {
	if (i > 5) {
		break;
		print("this message should never display!");
	}

	if (i > 5) {
		print("this message should never display!");
	}
}

# should print 999
print((function() {
	# test of break in function
	for(i in 1:10) {
		if (i > 5) {
			break;
			print("this message should never display!");
		} else {
			cat(i);
			cat("\t");
		}
	}

	for(i in 1:10) {
		if (i > 3) {
			return 999;
		} else {
			print(i);
		}
	}
})());