imports "Parallel" from "snowFall";

parallel(x = 1:10000, n_threads = 2, verbose = TRUE, debug = TRUE) {
	print(x);
	sleep(100);
	pause();
}