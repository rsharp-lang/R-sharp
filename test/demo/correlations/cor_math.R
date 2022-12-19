require(igraph.builder);
require(igraph);

# unit test of the correlation math

m = data.frame(
    a = [345,7436,52,37,47,23,412],
    b = [3.45,74362,52,327,427,23,-412],
    c = [1345,7436,52,37,497,23,412],
    d = [345,43,2,37,497,923,41],
    e = [34.5,74.36,52,37,47,923,9412]
);

rownames(m) = `Item_${1:7}`;

print(m);

m = corr(m);

print(m);

m = corr_sign(m) * (m ^ 2);

print(m);

m 
|> correlation.graph(0, 1)
|> save.network(file = `${@dir}/corr_graph/`, properties = "*")
;

print(read.csv(`${@dir}/corr_graph/network-edges.csv`));