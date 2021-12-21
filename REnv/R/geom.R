imports "geometry2D" from "graphics";

#' Evaluate geom density of 2d points
#'
#' @param qcut the quantile cutoff threshold for define
#'    points as low density points(noise).
#'
#' @return a new dataframe object with data fields:
#'    x, y, density, and noise boolean flags
#'
density2DCut = function(data, k = 6, qcut = 0.1, f = 10) {
    const [x, y]  = data;
    const density = density2D(as.integer(x * f), as.integer(y * f), k);
    const q       = quantile(density, probs = NULL);
    const cut     = as.object(q)$Query(qcut);

    print("quantile cut of the density value is:");
    print(`${qcut} -> ${cut}`);
    print("previews of the density value:");
    print(toString(q));
    str(density);

    data.frame(
        x       = x,
        y       = y,
        density = density,
        noise   = density <= cut
    );
}
