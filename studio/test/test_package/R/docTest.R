
#' test function title
#' 
#' @description test description with
#'    multiple lines
#' 
#' @param a a string parameter
#' @param b integer vector with default 
#' value \code{[1,2,3]}
#' 
#' @return nothing?
#' 
#' @author xieguigang
#' @details this is the remark text about this
#' function
#' 
#' @keywords keyword1,keyword2,keyword3
#' 
let func as function(a as string, b = [1,2,3], c = ["12345", FALSE], d = list(a=1,b=333)) {

}


imports "roxygen" from "roxygenNet";


print(parse(readText(@script)));
