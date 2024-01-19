print(1:10);
print(as.character(1:10));

cell_terms = data.frame(a = 1, b = TRUE, c = [56,4,56]);

rownames(cell_terms) <- 1:nrow(cell_terms);

print(cell_terms);