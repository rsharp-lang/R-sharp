imports "Parallel" from "snowFall";

`${@dir}/argv.cache`
|> readBin()
|> Parallel::parseSymbolPayload()
|> str()
;