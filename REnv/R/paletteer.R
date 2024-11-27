#' Get continuous palette by package and name
#'
#' Available package/palette combinations are available in the data.frame
#' \code{\link[paletteer]{palettes_c_names}}.
#'
#' @param palette Name of palette as a string. Must be on the form
#' packagename::palettename.
#' @param n Number of colors desired. Must be supplied.
#' @param direction Either `1` or `-1`. If `-1` the palette will be reversed.
#' @return A vector of colors.
#' @examplesIf rlang::is_installed("scico")
#' paletteer_c("scico::berlin", 100)
#' @export
const paletteer_c = function(palette, n, direction = 1) {
  let palette_internal = ".paletteer$in-memory";

  if (!exists(palette_internal, globalenv())) {
    # load in-memory cache data to global environment
    let res_file = system.file("data/paletteer/palettes_d.rda", package = "REnv");
    let cache = readRData(res_file);
    
    set(globalenv(), palette_internal, cache$palettes_d);
  }

  palette_internal <- get(palette_internal, globalenv());

  let c_palette <- unlist(strsplit(palette, "::"));
  let pkg_src = c_palette[1];
  let c_name = c_palette[2];

  if (!(pkg_src in palette_internal)) {
    stop([
        'Palette not found. Make sure both package and palette name are spelled correct in the format "package::palette".',
        `palette input: ${palette}`,
        `parsed name tokens: ${c_palette}`
    ]);
  } else {
    palette <- palette_internal[[pkg_src]];
  }

  if (!(c_name in palette)) {
    stop([
       'Palette not found. Make sure both package and palette name are spelled correct in the format "package::palette".',
       `palette name is unavailable: ${c_name}`
    ]);
  }  

  if (direction < 0) {
    # make reverse
    rev(
      grDevices::colors(palette[[c_name]], n, character = TRUE)
    );
  } else {
    grDevices::colors(palette[[c_name]], n, character = TRUE);
  }
}