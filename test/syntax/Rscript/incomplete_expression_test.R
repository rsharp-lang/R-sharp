let exp1 = incomplete_expression();

exp1("1+scan(a");
exp1(",x");
print([exp1]::Check);