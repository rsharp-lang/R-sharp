require(utils);
require(JSON);

let data = readRData(system.file("data/paletteer/palettes_d.rda", package = "REnv"));

print(names(data$palettes_d));
print(names(data$palettes_d$ggthemes));

# stop();

# str(data);

print(paletteer_c("ggthemes::excel_Office_Theme", 30));

JSON::json_encode(data$palettes_d) |> writeLines(con = file.path(@dir, "palettes_d.json"));