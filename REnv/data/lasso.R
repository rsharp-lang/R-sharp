# glmnet package needs to be installed and loaded.

diab <- read.table("diabetes.data",header=T)
fit1 <- glmnet(as.matrix(diab[,-11]), diab[,11])
