
# create a new dataframe that contains 3 fields
const table = data.frame(
    X = [1,2,3,4,5],
    Y = [5,4,3,2,1],
    Z = runif(5, min = -10000, max = 8000000),
	"A+3" = TRUE
);

const populateTable as function() {
	table;
}

print("the raw data matrix is:");
print(table);

# [1] "the raw data matrix is:"
#         X          Y          Z
# <mode>  <integer>  <integer>  <double>
# [1, ]   1          5           2055353.9741856
# [2, ]   2          4           1029857.3195328
# [3, ]   3          3           4535123.5024608
# [4, ]   4          2           4153364.6332209
# [5, ]   5          1           721031.6091827

const offset = 0;

# run LINQ query on the given table
let runQuery = {
    FROM [X, Y, Z, "A+3"] 
    IN populateTable()
	# create a table subset with given condition
    WHERE abs(Z) > 100 && Z <= 7950000 + X
    SELECT X, Y, zz = log(Z) * X, Z, sum("A+3") 
    ORDER BY zz DESCENDING
    TAKE 10
    SKIP 1 + offset
}

print("Get result data table after subset:");
print(runQuery);

# [1] "Get result data table after subset:"
#         X          Y          Z
# <mode>  <integer>  <integer>  <double>
# [1, ]   4          2           4153364.6332209
# [2, ]   1          5           2055353.9741856
# [3, ]   2          4           1029857.3195328
# [4, ]   5          1           721031.6091827

write.csv(runQuery, file = `${dirname(@script)}/queryOut.csv`);