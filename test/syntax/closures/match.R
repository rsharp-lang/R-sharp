
let sendTestAlert = function() {
    "this is test alert";
}

let sendNuclearAlert = function() {
    "Nuclear warning";
}

# php 8 match expression syntax

let run = function(code = ["test" "send" "err"]) {

    let response = match(test) {
        'test' => sendTestAlert(),
        'send' => sendNuclearAlert()
    };

print(response);
}

run("send");

run("test");

