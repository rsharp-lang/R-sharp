imports "http" from "Rstudio";
imports "help" from "Rstudio";

# title: R# web http server
# author: xieguigang
# description: a commandline R# script for running a http web server.

[@info "the http port for listen, 80 port number is used by default."]
const httpPort as integer  = ?"--listen"  || 80;
[@info "A directory path that contains the R script for running in this R# web server."]
[@type "directory"]
const webContext as string = ?"--wwwroot" || "./";

help::http_load();

# /search?q=keyword

#' Route url as local R script file
#' 
#' @param url the url object that parsed from the
#'     http request.
#' 
const router = function(url, headers) {
  const relpath as string = trim(url$path, ".");
  const httpHelp = {
	"/search": any -> help::search(url$query$q),
	"/browse": any -> help::browse(url$query$pkg),
	"/vignettes": any -> help::vignettes(url$query$q)
  };
  
  if ([relpath == ""] || [relpath == "/"]) {
    list(
      file = help::index(),
      is_script = FALSE,
	  is_html   = TRUE
    );
  } else {
    if (!is.null(httpHelp[[relpath]])) {
	  const api = httpHelp[[relpath]];
	  
	  list(
		file = api(url),
		is_script = FALSE,
	    is_html   = TRUE
	  );
	} else {
	  localfile_router(relpath, url, headers);
	}
  }  
}

const localfile_router = function(relpath, url, headers) {
	const refer = http::parseUrl(headers$Referer);

	str(refer);

	if ([file.ext(relpath) == "html"] && [refer$path == "vignettes"]) {
		list(
			file = help::vignettes(relpath, context = refer$query$q),
			is_script = FALSE,
			is_html   = TRUE
		);
	} else {
		const isMap_temp as boolean = startsWith(url$path, "@temp");
		const tempfile as string = {
			if(isMap_temp) {
				gsub(relpath, "@temp", getOption("system_tempdir"))
			} else {
				`${webContext}/${relpath}`;
			} 
		}
		
		print("non-script file:");
		print(tempfile);

		list(
			file = normalizePath(tempfile), 
			is_script = FALSE,
			is_html   = FALSE
		);
	}
}

#' Handle http GET request
#' 
const handleHttpGet = function(req, response) {
  const local = router(getUrl(req), getHeaders(req));

  print("request from the browser client:");
  str(getUrl(req));

  print("view the request data headers:");
  str(getHeaders(req));

  print("this is the unparsed raw text of the http header message:");
  print(getHttpRaw(req));

  if ([local$is_script] && file.exists(local$file)) {
	print(`run script: '${local$file}'!`);
    writeLines(source(local$file), con = response);
  } else {
    if (!local$is_script) {
	  if (local$is_html) {
		writeLines(local$file, con = response);      
	  } else {
	      if (file.ext(local$file) in ["html","htm","txt"]) {
			print("Response a html text file!");
			writeLines(readText(local$file), con = response);      
		  } else {
			print(`Push file downloads: '${local$file}'!`);
			response 
			|> pushDownload(local$file)
			;
		  }	  
	  }
    } else {
      response
      |> httpError(404, `the required Rscript file is not found on filesystem location: '${ normalizePath(local$file) }'!`)
      ;
    }
  }
}

#' Handle http POST request
#' 
const handleHttpPost = function(req, response) {
  const R as string = router(getUrl(req));

  str(getUrl(req));
  str(getHeaders(req));

  print(getHttpRaw(req));

  if (file.exists(R)) {
    writeLines(source(R), con = response);
  } else {
    response
    |> httpError(404, `the required Rscript file is not found on filesystem location: '${ normalizePath(R) }'!`)
    ;
  }
}

cat("\n\n");

http::http_socket()
|> headers(
  "X-Powered-By" = "R# web server",
  "Author"       = "xieguigang <xie.guigang@gcmodeller.org>",
  "Github"       = "https://github.com/rsharp-lang/Rserver",
  "Organization" = "R# language <https://github.com/rsharp-lang/>"
)
|> httpMethod("GET",  handleHttpGet, accessAny = TRUE)
|> httpMethod("POST", handleHttpPost, accessAny = TRUE)
|> httpMethod("PUT", [req, response] => writeLines("HTTP PUT test success!", con = response), accessAny = TRUE)
|> listen(port = httpPort)
;
