let call = FUNC -> FUNC();

# call anonymous function
call(function() {
	print("call a anonymous function");
	
	# print(traceback());
print(as.data.frame(traceback()));
	
	});


let a <- function(x) {
	call(x);
}

a(function() {
     print(traceback());
});

print(a);

(function() {

print("XXXXXXXXX");

})();