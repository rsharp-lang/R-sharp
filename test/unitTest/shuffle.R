v = list(a=24234,b=TRUE,c="fsdhfhsdf",d=[4.63,4867,3495,823.3],e=FALSE,f=["H","Z"],g=list(x=5,y=999.6));

set.seed(001); # just to make it reproducible

str(v);

let i = sample(v);

print(i);

str(v[i]);