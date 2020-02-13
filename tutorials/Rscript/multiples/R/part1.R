
let callers as function() {
    let internal as function() {
          doLoop();
    }

    internal();
}

let doLoop as function() {

	for(i in [1,2]) {
		if (i == 2) {
		 # get stack trace information
      return  traceback();
		}
	}

}