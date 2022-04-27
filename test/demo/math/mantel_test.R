setwd(@dir);

varechem = read.csv("./vegan/varechem.csv", row.names = 1);
varespec = read.csv("./vegan/varespec.csv", row.names = 1);

print(varechem);
print(varespec);

mantel = lapply(list(Spec01 = 1:7,
                                         Spec02 = 8:18,
                                         Spec03 = 19:37,
                                         Spec04 = 38:44), function(i) {
										 
										 mantel.test(varespec[, i], varechem);
										 });

