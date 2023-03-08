# split vector
v = 1:100;

str(split(v, size = 13));

# split list
l = list(a=1,b=2,c=3,d=NULL,e=NULL,f=NULL,g=TRUE,h=FALSE,i=FALSE,j=NULL,k=4566.4,l="asdsdd");

str(l);
str(split(l, size = 3));