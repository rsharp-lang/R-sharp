print("loop continute test");

const printMods = for(i in 1:10) {
	if (i % 2 == 0) {
		next;
	}

	# never display 0 at here
	print(`${i} MOD 2 = ${i % 2}`);
}

print("loop continute test job done!");
print(printMods);

let add1 as function(x) {
	print(`x + 1 -> ${x+1}`);
}

# the last expression value will be the result
# value of each for loop iteration
const all = for(i in 1:10 step 2.5) {
	add1(x = i);
}

print("all contents:");
str(all);

# break test of the for loop
print("break at first, and then the loop break, message will never display:");
for(i in 1:10) {
	break;
	print("this message should never display!");
	stop("invaid stack info!");
}

print("test break from another if closure populate out to for loop closre");
for(i in 1:10) {
	if (i > 5) {
		print(i);
		print("for loop will break at here!");
		break;
		stop("this error message should never display!");
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
		if (i < 3) {
			print(i - 1);			
		} else {
			return 999;
		}
	}
})());


pause();