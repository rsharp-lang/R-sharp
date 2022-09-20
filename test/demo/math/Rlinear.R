imports "package_utils" from "devkit";

setwd(@dir);

pkg = "D:\biodeep\biodeep_pipeline\biodeepMSMS_v5\data\HQ_linears.rda"
|> parseRData.raw()
|> unpackRData()
|> lapply(package_utils::deserialize)
;

print(pkg);

