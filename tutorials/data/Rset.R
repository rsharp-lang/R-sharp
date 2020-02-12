# R object dataset operation

# create two dataset
let x <- ["abc","de","f","g","hi","jk","lm","no","pqr","stu","vw","xyz"];
let y <- ["abc","d","e","f","g","hi","jk","l","m","no","p","q","r","stu","v","w","xyz"];

# set intersection
print(intersect(x,y));
# set union
print(union(x,y));
# set union is equals to set append and then do unique
print(unique(x << y));
