require(utils);

let data = readRData(system.file("data/paletteer/palettes_d.rda", package = "REnv"));

print(names(data$palettes_d));
print(names(data$palettes_d$ggthemes));

# stop();

# str(data);

print(paletteer_c("ggthemes::excel_Office_Theme", 30));