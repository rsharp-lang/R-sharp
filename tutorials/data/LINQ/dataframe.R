const table = data.frame(
    X = [1,2,3,4,5],
    Y = [5,4,3,2,1],
    Z = runif(5, min = 600, max = 80000)
);

let runQuery = {
    FROM [X, Y, Z] 
    IN table
    WHERE Z between 7000 and 9000
    ORDER BY Z
    TAKE 10
}

print(runQuery);