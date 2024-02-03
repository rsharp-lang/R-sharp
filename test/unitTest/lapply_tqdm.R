
x = list(a=1,b=2,c=3,d=4,e=5,f=6,g=7,h=99,i=555,j=222,k=234534,l=53534,m=43534,n=112312);

str(x);

x = lapply(tqdm(x), function(xi) {
    sleep(2);
    xi;
});


str(x);