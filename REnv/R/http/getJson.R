
#' get json web request
#' 
#' @param raw_text this parameter determine that the 
#'    function return value is the raw json character 
#'    text data or ``R#`` object value.
#' @param url the web resource of the target json 
#'    resource object.
#' 
#' @details this function works on the ``http_get`` function,
#'    so that the options for ``http_get``: http.cache_dir and 
#'    http.debug is required as well.
#' 
const getJSON = function(url, interval = 3, raw_text = FALSE) {
  const json_text as string = getHtml(url, interval, filetype = "json");

  if (raw_text) {
    json_text;
  } else {
    json_text |> JSON::json_decode();
  }
}