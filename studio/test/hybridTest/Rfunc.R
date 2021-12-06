imports "base.py";
imports "ifTest.py";

print2 = function(x) {
	print(" --> print2");
	printHello(x * 2);
}

veryDeep = function(x) {
	print(" --> veryDeep");
	
	branchTest(as.numeric(x));	
	stopRun();
}