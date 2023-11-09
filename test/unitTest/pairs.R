let n1 = 1:100;
let n2 = 1:100;

let x = sapply(n1, function(x) {
    lapply(n2, function(y) {
        let a = as.integer(`${x}${y}`);
        let sub = a - x;

        if (sub == as.integer(`${y}${x}`)) {
           str(  list(x, y));
        } else {
            NULL;
        }
    });
});

# print(x);


