let outer as function(x) {

	let triggerErr as function(x) {
	   stop(x);
	}
	
	triggerErr(x);
}

outer("12345");