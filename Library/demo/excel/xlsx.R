require(xlsx);

write.xlsx(data.frame(
	col1 = [1,2,3,4,5], 
	text = "hello", 
	target = ["world", "word", "package", "ABCDEFG", "this is a test string!"
]), 
	file = `${!script$dir}/test.xls`,
	sheetName = "hello_world"
);
