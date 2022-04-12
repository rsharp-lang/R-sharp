let runAction as function(action) {
	print(action());
}

# runAction(function() {
	# print([1,2,3,4,5]);
	# traceback();
# });

print("show stack traceback:");

cat("\n\n");

(function() {   
	# traceback function shows the function caller stack
	(function() {
		runAction([] -> ([] =>  traceback())());
	})();	
})();

