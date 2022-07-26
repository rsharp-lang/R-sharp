require(geometry2D);


bitmap(file = `${@dir}/kd-tree.png`) {
	Kdtest(n = 1000, knn = 40, size = [3300,2100]);
}