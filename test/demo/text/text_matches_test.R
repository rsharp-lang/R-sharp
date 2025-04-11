require(stringr);

let group1 = ["Sham_7" "Sham-7" "Sham-i" "Sham-z" "Sham-7"];
let group2 = ["Sham_2" "Sham-2" "Sham-z" "Sham_z" "Sham-s"];
let group3 = ["IL-8" "Ii-b" "Il_8" "Il-0" "il-8"];

let target = "Sham-7";

let s1 = levenshtein(group1, target);
let s2 = levenshtein(group2, target);
let s3 = levenshtein(group3, target);

let result = cbind( cbind(s1,s2), s3);

result = data.frame(
    group1, score_1 = result$score, group2, score_2 = result$score_1, group3, score_3 = result$score_1_1
);

print(result);

write.csv(result, file = "Z:/aaa.csv")