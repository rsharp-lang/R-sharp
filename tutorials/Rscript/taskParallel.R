imports "Parallel" from "snowFall";

const seqVals = [1,3,5,6,7,8,9];
const bbb     = [2,2,2,22,2,1,9999];

const y = parallel(x = seqVals, z = bbb, n_threads = 2) {
	print(x);
	x + 5 + z;
};

print("parallel loop:");
print(y);

print("vector loop:");
print(seqVals + 5 + bbb);