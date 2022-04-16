require(dataframe);

table = [];

table = append(table, [entityRow("asasdas", dfd= "sdsdf", s55= 8988,ddd =TRUE )]);

table = append(table, [entityRow("asasdas-2", dfd= "sdsdf", s55= 8888988,ddd1 =TRUE )]);
table = append(table, [entityRow("asasdas-3", dfd= "33sdsdf", s55= 88888988,ddd2 =TRUE )]);
table = append(table, [entityRow("asasdas-4", dfd= "sd3454sdf", s55= 898000008,ddd3 =TRUE )]);

write.csv(table, file = `${@dir}/table.csv`, row.names = TRUE);