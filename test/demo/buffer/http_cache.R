imports "http" from "webKit";

require(HDS);
setwd(@dir);

let local = http::http.cache(fs = HDS::openStream(file = "./cache.dat", meta_size = 16*1024*1024));