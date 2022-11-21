const print_line = function() {
	print("package function from 'pkg2'!");
	trace();
	stop();
}

const echo_pkg2 = function() {
	print_line();
}