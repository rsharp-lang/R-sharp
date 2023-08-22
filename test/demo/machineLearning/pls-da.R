require(REnv);

let ds = sacurine();

str(ds);

let x = ds$dataMatrix;

pls = plsda(x, NULL, ncomp = 3);

str(pls);