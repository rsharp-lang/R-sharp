let test = function(i) {
    cat(`${i}    `);

    if (i % 2 == 0) {
        print("is even");
    } else if (i == 3) {
        print("i is 3");
    } else if (i == 7) {
        print("i is 7");
    } else {
        print(`is other number: ${i}`);
    }

    if (i > 5 && i < 9) {
        print("is bigger");
    } else if (i >= 9) {
        print("is top");
    } else {
        print("is smaller");
    }
}

for(let i in 0:13) {
    test(i);
}

