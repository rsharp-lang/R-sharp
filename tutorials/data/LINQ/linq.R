let x = 8;

# The linq expression is a SQL language liked expression in R#
# use linq expression is better and faster than lapply and for loop
# and write less code on data analysis program
let zzz <- from x as double in list(skip = x + 5, A =5,B =1, C=2,D =3,E =4) 
           let y as double = x+6
           where x <= 5 
           let z = x + 5
           select [AA = z,BB = y, x^2];
		   
str(zzz);