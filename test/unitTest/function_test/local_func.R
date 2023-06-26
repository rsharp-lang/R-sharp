const run = function() {
    print(basename(!script));

    let basename = function(aaa) {
        print(aaa);
        print("ok");
    }

    print(basename(!script));
}

const load = function() {
    return(function(aaa) {
        print(aaa);
        print("ok");
    });
}

const run2 = function() {
    print(basename(!script));

    let basename = load();

    print(basename(!script));
}

const run3 = function() {
    str(description('REnv'));

    let description = function(x) {
        x;
    }

    str(description('REnv'));
}

run3();


