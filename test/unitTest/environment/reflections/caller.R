let runner = function() {
    sys.call(-1);
}

let workflow_caller = function() {
    runner();
}

# [1] workflow_caller
print(workflow_caller());