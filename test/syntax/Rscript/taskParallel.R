imports "Parallel" from "snowFall";

const seqVals = [1,3,5,6,7,8,9];
const bbb     = [2,2,2,22,2,1,9999];
const g as  double = 2.888888;

print("start run parallel....");

const y = parallel(x = seqVals, z = bbb, n_threads = 2) {#, debug = TRUE, master = 3305, bootstrap = 5566) {
	print(x);
	sleep(1 + rnd() * 10);
	(x + 5 + z) ^ g;
};

print("parallel loop:");
print(y);

print("vector loop:");
print((seqVals + 5 + bbb) ^ g);

print(y == (seqVals + 5 + bbb) ^ g);

print("result is all identical:");
print(!any(y != (seqVals + 5 + bbb) ^ g));