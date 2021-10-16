imports "geometry2D" from "graphics";

#' Evaluate geom density of 2d points 
#' 
#' @param qcut the quantile cutoff threshold for define 
#'    points as low density points(noise).
#' 
const density2DCut as function(data, k = 6, qcut = 0.1) {
    const [x, y]  = data;
    const density = density2D(x, y, k);
    const q       = quantile(density, probs = NULL); 
    const cut     = as.object(q)$Query(qcut);

    data.frame(
        x       = x,
        y       = y,
        density = density,
        noise   = density < cut
    );
}