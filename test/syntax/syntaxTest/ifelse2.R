options(strict = FALSE);    

# header = "123";
# header = "97.5%-tile:";
# header = "2.5%-tile:";
for(header in ["123", "97.5%-tile:", "2.5%-tile:", "xxx"]) {

	print(header);
	
	if (header == "2.5%-tile:") {
		print(4);
	} else if (header == "97.5%-tile:") {
		print(400);
	} else if (header == $"\d+") {
		print("is a number");
	} else {
		stop("test");
	}
}


