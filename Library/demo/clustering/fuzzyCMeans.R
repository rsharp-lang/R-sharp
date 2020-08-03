imports "clustering" from "R.math";

require(dataframe);

setwd(!script$dir);

let result = "E:\smartnucl_integrative\biodeep_pipeline\Biodeep_Rpackage\etc\pathway_network\msms_Intensity.csv"
:> read.dataframe(mode = "numeric")
:> cmeans(centers = 16, fuzzification = 1.25, threshold = 0.005)
:> as.data.frame
;

print(result);

result
:> write.csv(file = "./cmeans.csv")
;