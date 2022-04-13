const worker as function() {
	inner1();
	downstream();
}

const inner1 as function() {
	inner2();
}

const inner2 as function() {
	inner3();
}

const inner3 as function() {
	internalException();
}

const internalException as function() {
	return(stop());
}

using dir1 as workdir("./") {
	x = worker();
	print(x);
}

