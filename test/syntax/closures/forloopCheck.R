const test as function(n = 100) {

	for(i in 1:n) {
	
		if (i > 10) {
		# 11 * 9 = 99
			return(i * 9);
		}
	}

    return(-100);
}


print(test());