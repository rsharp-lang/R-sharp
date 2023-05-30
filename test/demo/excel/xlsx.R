require(xlsx);

setwd(@dir);

write.xlsx(data.frame(
	col1 = [1,2,3,4,5], 
	text = "hello", 
	target = ["world", "word", "package", "ABCDEFG", "this is a test string!"
]), 
	file = "./test.xls",
	sheetName = "hello_world"
);

write.xlsx(data.frame(
	col1 = [1,2,3,4,5], 
	text = "hello", 
	target = ["world", "word", "package", "ABCDEFG", "this is a test string!"
]), 
	file = "./test.xlsx",
	sheetName = "hello_world"
);