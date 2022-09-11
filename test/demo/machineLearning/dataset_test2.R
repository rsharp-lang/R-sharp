imports "dataset" from "MLkit";

raw = data.frame(
    name = ["a","b","b","b","c","a"],
    flags = [TRUE,TRUE,TRUE,FALSE,TRUE,TRUE],
    z = [15,64,23,12,31,23],
    bins = [5689,56,12,2345,645,6164]
);

print(raw);

raw = raw 
|> toFeatureSet()
|> encoding(
    name  -> dataset::to_factors,
    flags -> dataset::to_ints,
    bins  -> dataset::to_bins
)
|> as.data.frame()
;

print(raw);
