# make query after join two table
const employee = data.frame(
    ID        = [1,        2,          3,        4,         5,      6       ],
    Name      = ["Preety", "Priyanka", "Anurag", "Pranaya", "Hina", "Sambit"],
    AddressId = [1,        2,          0,        0,         5,      6       ]
);

const address = data.frame(
    ID          = [1,             2,             5,             6             ], 
    AddressLine = ["AddressLine1","AddressLine2","AddressLine5","AddressLine6"]
);

# the join operation
let result = {
    FROM [ID, Name, AddressId] IN employee
    JOIN [ID, AddressLine] IN address
    ON employee$AddressId == address$ID
    WHERE employee$ID > 5
}
# will produce a join table output
# employee.ID, Name, AddressId, address.ID, AddressLine

print(result);