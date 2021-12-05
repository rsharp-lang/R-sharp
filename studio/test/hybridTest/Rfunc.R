imports "base.py";

const print2 as function(x) {
	print(" --> print2");
	printHello(x * 2);
}

const veryDeep as function() {
	print(" --> veryDeep");
	stopRun();
}