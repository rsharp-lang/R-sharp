let prepare_receptor = "/opt/mgltools/MGLToolsPckgs/AutoDockTools/Utilities24/prepare_receptor4.py";
let receptor_cmd <- list(
    prepare_receptor,
    prepare_receptor,
    "-r" = shQuote("input one"),
    "-o" = shQuote("output one"),
    "-A" = "hydrogens",
    "-U" = "nphs_lps_waters" # 删除非极性氢、配体和水分子
);

str(receptor_cmd);

options(list.symbol_names = FALSE);

str(list(
    prepare_receptor,
    prepare_receptor,
    "-r" = shQuote("input one"),
    "-o" = shQuote("output one"),
    "-A" = "hydrogens",
    "-U" = "nphs_lps_waters" # 删除非极性氢、配体和水分子
));