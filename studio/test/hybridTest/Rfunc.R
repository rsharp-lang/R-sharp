imports "base.py";
imports "ifTest.py";

const print2 as function(x) {
	print(" --> print2");
	printHello(x * 2);
}

const veryDeep as function(x) {
	print(" --> veryDeep");
	
	branchTest(as.numeric(x));	
	stopRun();
}