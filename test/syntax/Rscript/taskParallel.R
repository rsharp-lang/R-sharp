imports "Parallel" from "snowFall";

const seqVals = [1,3,5,6,7,8,9];
const bbb     = [2,2,2,22,2,1,9999];
const g as  double = 2.888888;

print("start run parallel....");

const f = function(x, z, g) {
	(x + 5 + z) ^ g;
}

const y = parallel(x = seqVals, z = bbb, n_threads = 2, ignoreError = TRUE, debug = TRUE){# , debug = TRUE, master = 3305, bootstrap = 5566) {
	print(x);
	# sleep(1 + rnd() * 10);
	x = f(x, z, g);
	stop(x);
};

print("parallel loop:");
print(y);

print("vector loop:");
print(f(seqVals, bbb, g));

print(y == f(seqVals, bbb, g));

print("result is all identical:");
print(!any(y != f(seqVals, bbb,  g)));