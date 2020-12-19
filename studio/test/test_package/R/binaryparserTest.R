imports "package_utils" from "devkit";

const expression = readBin(`${!script$dir}/../../LipidSearch_1.0.0.1254/src/8873003873514dda0c83f683dfa27a1d`)
:> read
:> eval(globalenv())
:> print
;