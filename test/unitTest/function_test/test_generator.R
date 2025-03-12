let pops = function() {
    for(i in 1:10) {
        print(`get offset: ${i}`);
        yield(i);
    }
}

for(let xi in pops()) {
    print(xi ^ 2);
}