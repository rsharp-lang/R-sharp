let hello_world as function(words as string = ["user", "world"]) {
	print("hello");
	print(words);
	
	print(`hello ${words}!`);
}

let emptyFunc as function() {
	"do nothing!";
}