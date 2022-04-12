require(strings);

let a as string = "AATGCCCCTAAA";
let b as string = "ATATGCCCCAAA";

let compares = levenshtein(a, b) :> as.object;

print("string compares of:");
cat("\n");

print(a);
print(compares$DistEdits);
print(b);

cat("\n");
print("string similarity:");
print(compares$MatchSimilarity);

html(compares) :> writeLines(con = "./index.html");