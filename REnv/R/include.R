const include = function(file) {
    if (file.ext(file) == "json") {
        include_jsonconfig(file);
    }
}

#' load json file and set value into environment options
const include_jsonconfig = function(file) {
    let configs = file 
        |> readText() 
        |> JSON::json_decode()
        ;

    # set json list to environment runtime config
    options(configs);
}