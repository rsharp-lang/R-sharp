#' Http get html or from cache
#'
#' @param url the url of the resource data on the remote server
#' @param interval the time interval in seconds for sleep after
#'                 request data from the remote server.
#'
#' @details using of the http get function required of set options of
#'     ``http.cache_dir`` at first!
#'
const getHtml = function(url, interval = 3, filetype = "html") {
  # finally read data from cache
  readText(http_get(
    url = url,
    streamTo = function(url, cache_file) {
      writeLines(con = cache_file) {
        # request from remote server
        # if the cache is not hit,
        # and then write it to the cache repository
        http::content(requests.get(url), plain_text = TRUE);
      }
    },
    interval = interval,
    filetype = filetype)
  )
  ;
}