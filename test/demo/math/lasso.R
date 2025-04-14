let data = read.csv("G:\GCModeller\src\R-sharp\REnv\data\diabetes.data", row.names = NULL, check.names = FALSE, tsv = TRUE);

# print(data);
print(data[,-11]);
print(data[,11]);

let fit = lasso(data[,-11], data[,11]);

print(as.data.frame(fit));

write.csv(as.data.frame(fit), file = file.path(@dir,"diabetes_lasso.csv"), row.names = FALSE);