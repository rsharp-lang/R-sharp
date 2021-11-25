options(strict = FALSE);    

header = "123";
# header = "97.5%-tile:";
# header = "2.5%-tile:";

	if (header == "2.5%-tile:") {
                print(4);
            } else if (header == "97.5%-tile:") {
                print(400);
            } else {
				stop("test");
			}