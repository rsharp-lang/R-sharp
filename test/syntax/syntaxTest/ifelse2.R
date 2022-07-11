options(strict = FALSE);    

if (1 > 0 ) {

	# header = "123";
	# header = "97.5%-tile:";
	# header = "2.5%-tile:";
	for(header in ['123', '97.5%-tile:', '2.5%-tile:', 'xxx']) {

		print(header);
		
		if (header == '2.5%-tile:') {
			print(4);
		} else if (header == '97.5%-tile:') {
			print(400);
		} else if (header == $"\d+") {
			print('is a number');
		} else {
			if (header == 'xxx') {
				print("xxx should be an error!");
			} else {
				print(`target '${header}' is not xxx?`);
			}
			
			stop('test');
		}
	}
}

if (0 > 1) {
   stop("invalid stack!");	
} else {
	print("envirnment stack test success!");
}
