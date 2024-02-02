
x = list(a=1,b=2,c=3,d=4,e=5,f=6,g=7);

str(x);

x = lapply(tqdm(x), function(xi) {
    sleep(2);
    xi;
});


str(x);