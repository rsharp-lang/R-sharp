let runner = function() {
    sys.call(-1);
}

let workflow_caller = function() {
    runner();
}

let runner_trace = function() {
    sys.calls();
}

let get_tracknames = function() {
    runner_trace();
}

# [1] workflow_caller
print(workflow_caller());
str(get_tracknames());