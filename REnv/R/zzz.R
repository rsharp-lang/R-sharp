const github_url as string = ["https://github.com/rsharp-lang/R-sharp"];

const .onLoad = function() {
    grDevices::register.color_palette(palette = REnv::paletteer_colors);
}