const table = data.frame(a = 1, b = 2:9, c = runif(8), d = "F");

print("the original data table:");
print(table);

print("the renamed table:");
table
|> rename(
	a -> id,
	b -> index,
	c -> "unify random",
	logical = "d"
)
|> print()
;