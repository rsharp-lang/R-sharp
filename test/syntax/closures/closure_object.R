# Use closure simulate a class object in R# scripting
# just like js function object

let closure as function() {

    let x as integer = 999;

    let setX as function(value) {
        x = value;
    }

    let getX as function() {
        x;
    }

    list(getX = getX, setX = setX);
}

let holder = closure();

# 999
print(holder$getX());

holder$setX([123,233,333]);
# 123 233 333
print(holder$getX());

print(holder$setX('123 AAAAA'));
# 123 AAAAA
print(holder$getX());