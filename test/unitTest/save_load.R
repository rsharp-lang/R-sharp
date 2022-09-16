

setwd(@dir);


data = list(
	
	null = NULL,
	empty = list(),
	blank = "",
	zero = [],
	value = "123",
	empty_vector = [NULL,NULL,NULL,NULL,NULL]

);

str(data);

save(data, file = "./empty.dat");

data = NULL;

load("./empty.dat");

str(data);
