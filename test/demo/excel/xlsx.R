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

write.xlsx( list(aaaaa = data.frame(
	col1 = [1,2,3,4,5], 
	text = "hello", 
	target = ["world", "word", "package", "ABCDEFG", "this is a test string!"
]), sheet333333 = data.frame(
	col1 = 1:10,
	col2 = TRUE,
	col5 = ["sfa","s","dad","ad","ee","ee","eesd","asdas","da","sd"]
)), 
	file = "./test.xlsx",
	row.names = FALSE
);