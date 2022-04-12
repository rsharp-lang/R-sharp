data = data.frame(id = [1,5,4,8,8], name = ["aaa","bbb","ccc","ddd","eee"], row.names = ["abc","ddd","fff","www","bbb"]);

print(data);


require(JSON);

json_encode(data) |> writeLines(con = `${@dir}/dataframe.json`);