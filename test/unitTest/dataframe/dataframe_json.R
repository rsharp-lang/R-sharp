require(JSON);

let df = data.frame(x = 1:10, y = 5, z = ((1:10) ^ 2) / 5, row.names = 'a':'j');

print(df);
print(JSON::json_encode(df, maskReadonly = TRUE, row.names = TRUE, full.vector = TRUE));