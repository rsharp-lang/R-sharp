imports "clustering" from "R.math";

require(dataframe);

setwd(!script$dir);

"E:\smartnucl_integrative\biodeep_pipeline\Biodeep_Rpackage\etc\pathway_network\msms_Intensity.csv"
:> read.dataframe(mode = "numeric")
:> cmeans
:> as.data.frame
:> write.csv(file = "./cmeans.csv")
;