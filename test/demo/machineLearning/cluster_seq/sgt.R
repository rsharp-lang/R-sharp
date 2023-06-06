# Learning a sgt embedding as a matrix with 
# rows and columns as the sequence alphabets. 
# This embedding shows the relationship between 
# the alphabets. The higher the value the 
# stronger the relationship.

imports "" from "MLkit";

sgt = SGT();
sequence = "BBACACAABA";
str(fit(sgt, sequence));

# (A, A)    0.090616
# (A, B)    0.131002
# (A, C)    0.261849
# (B, A)    0.086569
# (B, B)    0.123042
# (B, C)    0.052544
# (C, A)    0.137142
# (C, B)    0.028263
# (C, C)    0.135335