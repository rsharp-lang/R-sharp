# Learning a sgt embedding as a matrix with 
# rows and columns as the sequence alphabets. 
# This embedding shows the relationship between 
# the alphabets. The higher the value the 
# stronger the relationship.

imports "dataset" from "MLkit";

sgt = SGT();
sequence = "BBACACAABA";
str(fit(sgt, sequence = sequence));

# (A, A)    0.090616
# (A, B)    0.131002
# (A, C)    0.261849
# (B, A)    0.086569
# (B, B)    0.123042
# (B, C)    0.052544
# (C, A)    0.137142
# (C, B)    0.028263
# (C, C)    0.135335

print(fit(sgt, sequence = sequence, df = TRUE));

#                 A          B          C
# ----------------------------------------
# <mode>   <double>   <double>   <double>
# A       0.0906163  0.0865694   0.137142
# B        0.131002   0.123042  0.0282625
# C        0.261849   0.052544   0.135335