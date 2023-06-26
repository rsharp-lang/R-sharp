const run = function() {
    print(basename(!script));

    let basename = function(aaa) {
        print(aaa);
        print("ok");
    }

    print(basename(!script));
}

run();


