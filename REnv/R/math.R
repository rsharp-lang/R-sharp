#' Scale values to 0-1 range relative to each other. 
#'
#' @param x vector or matrix
#' @return vector or matrix with all values adjusted 
#'    0-1 scale relative to each other.
#'
const scale0_1 = function(x) {
  (x - min(x, na.rm = T)) / diff(range(x, na.rm = T));
}
