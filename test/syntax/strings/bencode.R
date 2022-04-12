require(stringr);

let obj = list(abc = "1234", c = 999, x = list(a=1,b=2,ccccc= FALSE));

print("the raw object is:");
str(obj);

let b = bencode(obj);
let j = json(obj);

print(b);
print(nchar(b));

str(bdecode(b));

print(j);
print(nchar(j));

str(fromJSON(j));


