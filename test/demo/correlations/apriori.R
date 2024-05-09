imports "apriori" from "MLkit";

let trans = apriori::transactions(

    t1 = [A,B,C],  
t2 = [A,B,D],  
t3 = [A,C,D]  ,
t4 = [A,B,C,E],
t5 = [A,C,E]  ,
t6 = [B,D,E]  ,
t7 = [A,B,C,D]
)
;

trans
|> apriori(support = 3/7, confidence = 5/7, minlen = 2)
|> as.data.frame()
|> print()
;