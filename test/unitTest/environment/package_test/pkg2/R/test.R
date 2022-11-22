const print_line = function() {
	print("package function from 'pkg2'!");
	trace();
	
	const aaa = function() {
		invisible(1);
	}
	
	aaa();
	
	stop();
}

const echo_pkg2 = function() {
	print_line();
	
	
}