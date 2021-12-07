imports "package_utils" from "devkit";

[data, flags, list_data, printContent] = "D:\GCModeller\src\R-sharp\studio\test\data\multiple_object.rda"
|> package_utils::parseRData.raw() 
|> package_utils::unpackRData
;