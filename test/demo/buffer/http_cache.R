imports "http" from "webKit";

require(HDS);
setwd(@dir);

let local = http::http.cache(fs = HDS::openStream(file = "./cache.dat", meta_size = 16*1024*1024, allowCreate = TRUE));

print(http::requests.get("https://stack.xieguigang.me/sitemap.xml", cache = local));
print(http::requests.get("https://stack.xieguigang.me/sitemap-pt-post-p1-2022-08.xml", cache = local));
print(http::requests.get("https://stack.xieguigang.me/sitemap-tax-post_tag-12.xml", cache = local));
print(http::requests.get("https://stack.xieguigang.me/sitemap-misc.xml", cache = local));
print(http::requests.get("https://stack.xieguigang.me/sitemap.html", cache = local));
print(http::requests.get("https://stack.xieguigang.me/sitemap-tax-category-1.html", cache = local));
print(http::requests.get("https://stack.xieguigang.me/sitemap-tax-category-6.html", cache = local));
print(http::requests.get("https://stack.xieguigang.me/sitemap-authors.html", cache = local));
 
close(local);