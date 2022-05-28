observations = data.frame(v = c(20, 3, 7, 2, 6, 10, 8, 7, 5, 10, 10, 8, 7, 5, 10, 2, 3, 7, 2, 6), group = append(rep("A", 5), append(rep("B", 10), rep("C", 5))));

print(observations);

test = aov( observations , formula = v ~ group);

str(test);
summary(test);