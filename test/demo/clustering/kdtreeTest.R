require(geometry2D);


bitmap(file = `${@dir}/kd-tree.png`) {
	Kdtest(n = 1200, knn = 30, size = [3800,2400]);
}