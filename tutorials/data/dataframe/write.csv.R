const a = [1,2,3,4,4,4,33334];
const b = [TRUE,TRUE,TRUE,TRUE,TRUE,FALSE,TRUE];
const C = "single string";

data.frame(
	a1    = (a + 6) / 2, 
	cc    = b, 
	dd    = C, 
	index = 1:length(a)
)
|> write.csv(
	file      = `${dirname(@script)}/dataframe.csv`, 
	row.names = ["a","b","c","d","e","f","g"]
)
;