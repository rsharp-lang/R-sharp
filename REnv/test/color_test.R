require(utils);

let data = readRData(system.file("data/paletteer/palettes_d.rda", package = "REnv"));

print(names(data$palettes_d));

# stop();

str(data);

print(paletteer_c("ggthemes::Blue-Green Sequential", 30));