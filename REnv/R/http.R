imports ["Html", "http"] from "webKit";

require(JSON);

#' A general method for Http get request
#'
#' @param streamTo A lambda function that accept a url and a parameter of file cache location.
#'     this parameter should run http get request from the remote server and then
#'     save file to local cache.
#'
#' @details using of the http get function required of set options of
#'     ``http.cache_dir`` at first! The ``http.cache_dir`` could a directory folder
#'     path to the local filesystem, or a reference symbol name in the global environment
#'     for get a filesystem wrapper object.
#'
const http_get = function(url, streamTo, interval = 3, filetype = "html") {
  const http.cache_dir as string = getOption("http.cache_dir") || stop("You should set of the 'http.cache_dir' option at first!");
  const http.debug as boolean = getOption("http.debug", default = FALSE);
  const is.local_dir = ![
    startsWith(http.cache_dir, "$") 
    || startsWith(http.cache_dir, "!") 
    || startsWith(http.cache_dir, "&")
  ];

  if (http.debug) {
    print(`[request] ${url}`);
  }

  const cacheKey   = md5(url);
  const prefix     = substr(cacheKey, 1, 2);
  const cache_file = {
     if (is.local_dir) {
        `${http.cache_dir}/${prefix}/${cacheKey}.${filetype}`;
     } else {
        # create a file reference closure object
        file.allocate(`/${prefix}/${cacheKey}.${filetype}`, fs = get(http.cache_dir, globalenv()));
     }
  };
  const hit_cache = list(hit = "yes");
  
  test_cache = (!file.exists(cache_file)) || (file.size(cache_file) <= 0);

  if (length(test_cache) == 0) {
    warning(print(`invalid cache key for [${url}]!`));
    
    print(prefix);
    print(cacheKey);
    
    test_cache = FALSE;
  }

  if (test_cache) {
    # request data from the remote server
    streamTo(url, cache_file);
    # sleep for a seconds after request resource data
    # from the remote server
    sleep(interval);

    if (http.debug) {
      print(`[cache] ${cache_file}`);
    }

    hit_cache$hit = "no";
  }

  if (http.debug) {
    if (as.logical(hit_cache$hit)) {
      print(`[hit_cache] ${cache_file}`);
    }
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