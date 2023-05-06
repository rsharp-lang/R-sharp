imports "Parallel" from "snowFall";

oks = parallel(x = 1:8, n_threads = 10, verbose = TRUE, debug = FALSE) {
	print(x);
	sleep(6);
	# pause();
	"OK!"
}

print(oks);
print(length(oks));