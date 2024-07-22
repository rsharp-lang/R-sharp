let a = list(a = 1, b = 2, c = 3 );
let b = list(a = 2, b = 2, c = 3);
let c = list(a = 2, b = 2, c = 99);
let d = list(a = -1, b =444, c = 999);

print(as.data.frame(list(a,b,c,d),t=TRUE))  ;

print(

    as.data.frame(list(a,b,c,d),t=TRUE, row.names = c("ss","dd","aa","cc"))
);