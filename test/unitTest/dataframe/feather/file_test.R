let df = read.feather(file = file.path(@dir, "r-feather-test-nullable.feather"));

rownames(df) <- `R${1:nrow(df)}`;
print(df);

write.feather(df, file = file.path(@dir, "demo_write.fbs"), row.names = TRUE);

print("view data read from file:");
print(read.feather(file = file.path(@dir, "demo_write.fbs")));