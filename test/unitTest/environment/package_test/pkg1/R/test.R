const print_line = function() {
	print("package function from 'pkg1'!");
	trace();
}

const echo_pkg1 = function() {
	print_line();
}

const trace = function() {
	print(traceback());
}