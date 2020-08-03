imports "xlsx" from "R.base";

let file = "X:\PlantMAT_v1.0.xlsm"
:> open.xlsx
;

file
:> sheetNames
:> print
;

print(file :> read.xlsx(sheetIndex = "SMILES"));
print(file :> read.xlsx(sheetIndex = "Library"));