imports ["Html", "http"] from "webKit";

#' A general method for Http get request
#'
#' @param streamTo A lambda function that accept a url and a parameter of file cache location.
#'     this parameter should run http get request from the remote server and then
#'     save file to local cache.
#'
#' @details using of the http get function required of set options of 
#'     ``http.cache_dir`` at first!
#' 
const http_get as function(url, streamTo, interval = 3, filetype = "html") {
  const http.cache_dir as string = getOption("http.cache_dir") || stop("You should set of the 'http.cache_dir' option at first!");

  const cacheKey as string   = md5(url);
  const prefix as string     = substr(cacheKey, 1, 2);
  const cache_file as string = `${http.cache_dir}/${prefix}/${cacheKey}.${filetype}`;

  if ((!file.exists(cache_file)) || (file.size(cache_file) <= 0)) {
    # request data from the remote server
    streamTo(url, cache_file);
    # sleep for a seconds after request resource data
    # from the remote server
    sleep(interval);
  }

  if (getOption("debug")) {
	print(`Cached_at: ${cache_file}`);
  }

  cache_file;
}

#' Request image from remote server or local cache
#'
#' @param url image url
#'
#' @details using of the http get function required of set options of 
#'     ``http.cache_dir`` at first!
#' 
const getImage as function(url, interval = 3) {
  readImage(
    http_get(
      url = url,
      streamTo = function(url, cache_file) {
        # request from remote server
        # if the cache is not hit,
        # and then write it to the cache repository
        wget(url, cache_file);
      },
      interval = 3,
      filetype = "png")
  );
}

#' Http get html or from cache
#'
#' @param url the url of the resource data on the remote server
#' @param interval the time interval in seconds for sleep after
#'                 request data from the remote server.
#'
#' @details using of the http get function required of set options of 
#'     ``http.cache_dir`` at first!
#' 
const getHtml as function(url, interval = 3) {
  # finally read data from cache
  readText(http_get(
    url = url,
    streamTo = function(url, cache_file) {
      writeLines(con = cache_file) {
        # request from remote server
        # if the cache is not hit,
        # and then write it to the cache repository
        content(requests.get(url));
      }
    },
    interval = 3,
    filetype = "html")
  )
  ;
}
