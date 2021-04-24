
# create a new dataframe that contains 3 fields
const table = data.frame(
    X = [1,2,3,4,5],
    Y = [5,4,3,2,1],
    Z = runif(5, min = -10000, max = 8000000)
);

print("the raw data matrix is:");
print(table);

# run LINQ query on the given table
let runQuery = {
    FROM [X, Y, Z] 
    IN table
    # create a table subset
    WHERE Z > 100 && Z <= 7900000
    ORDER BY Z DESCENDING
    TAKE 10
    SKIP 1
}

print("Get result data table after subset:");
print(runQuery);