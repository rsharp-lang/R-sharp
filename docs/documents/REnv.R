# demo script for build REnv unix man pages.

setwd(@dir);

REnv = ls("REnv");

for(category in names(REnv)) {
	for(ref in REnv[[category]]) {
		man(get(ref)) :> writeLines(con = `./REnv/${category}/${ref}.1`);
	}
}