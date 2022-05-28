observations = data.frame(v = c(2, 3, 7, 2, 6, 10, 8, 7, 5, 10, 10, 8, 7, 5, 10, 2, 3, 7, 2, 6), group = append(rep("A", 5), append(rep("B", 10), rep("C", 5))));

print(observations);

print(aov( observations , formula = v ~ group));