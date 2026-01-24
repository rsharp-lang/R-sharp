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
  const http.cache_dir = getOption("http.cache_dir") || stop("You should set of the 'http.cache_dir' option at first!");
  const http.debug as boolean = getOption("http.debug", default = FALSE);
  const is.local_dir = ![
    startsWith(http.cache_dir, "$") 
    || startsWith(http.cache_dir, "!") 
    || startsWith(http.cache_dir, "&")
  ];

  if (http.debug) {
    print(`[request] ${url}`);
    
    if (is.local_dir) {
      print("the cache repository is a local directory.");
    } else {
      print(`use data pack '${http.cache_dir}' as cache repository!`);
    }    
  }

  const cacheKey   = md5(url);
  const prefix     = substr(cacheKey, 1, 2);
  const cache_file = {
     if (is.local_dir) {
        `${http.cache_dir}/${prefix}/${cacheKey}.${filetype}`;
     } else {
        # create a file reference closure object
        let fs_path = `/${prefix}/${cacheKey}.${filetype}`;
        let fs_obj = env::get(http.cache_dir, globalenv());

        file.allocate(fs_path, fs = fs_obj);
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




