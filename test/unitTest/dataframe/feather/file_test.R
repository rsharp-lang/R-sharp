let df = read.feather(file = file.path(@dir, "r-feather-test-nullable.feather"));

print(df);

write.feather(df, file = file.path(@dir, "demo_write.fbs"), row.names = FALSE);