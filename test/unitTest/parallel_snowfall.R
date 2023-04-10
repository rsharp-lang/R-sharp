imports "Parallel" from "snowFall";

parallel(x = [1,3,5,7,9], n_threads = 2) {
	print(x);
	sleep(100);
	pause();
}