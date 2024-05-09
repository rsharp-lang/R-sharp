imports "apriori" from "MLkit";

apriori::transactions(

    t1 = [A,B,C],  
t2 = [A,B,D],  
t3 = [A,C,D]  ,
t4 = [A,B,C,E],
t5 = [A,C,E]  ,
t6 = [B,D,E]  ,
t7 = [A,B,C,D],

)
|> apriori()
|> str()
;