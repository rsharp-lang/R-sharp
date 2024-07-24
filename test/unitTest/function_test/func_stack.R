let xref = function() {
    print("symbol outside the closure!");
}

let metaReport = function() {
    let xref = list(a = function() {
        print("symbol inside the closure!");
    });

    let mapping = function() {
        if (isTRUE(TRUE)) {
            print(xref);
        }
    }

    let call = function() {
        sapply(1:3, i -> mapping());
    }

    tqdm(1:5) |> sapply(i -> call());
}

metaReport();