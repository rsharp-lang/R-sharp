data(bezdekIris);

print(bezdekIris);

let class = bezdekIris$class;
bezdekIris[,"class"] =NULL;

rownames(bezdekIris) = unique.names(class);

let dist_scale = dist(bezdekIris);

print(dist_scale);

let mds_result = cmdscale(dist_scale, k = 2);

print(mds_result );