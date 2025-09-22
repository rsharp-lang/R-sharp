
const test_run = function(a,b) {
    on.exit(print(b));
    print(a);
}

test_run("111","222");