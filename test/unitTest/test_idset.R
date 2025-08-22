let idset = {
    group1: [1 2 3 1 3 4 5],
    group2: [3 4 5 8 3 3 4 0 6 1 1],
    group3: [7 7 1],
    group4: [9 9 1 9 0 9 3]
};

str(idset);

idset = venn_exclusives(idset);

str(idset);

print(attr(idset,"common"));