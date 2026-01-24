#' Request image from remote server or local cache
#'
#' @param url image url
#'
#' @details using of the http get function required of set options of
#'     ``http.cache_dir`` at first!
#'
const getImage = function(url, interval = 3) {
  readImage(
    http_get(
      url = url,
      streamTo = function(url, cache_file) {
        # request from remote server
        # if the cache is not hit,
        # and then write it to the cache repository
        wget(url, cache_file);
      },
      interval = interval,
      filetype = "png")
  );
}