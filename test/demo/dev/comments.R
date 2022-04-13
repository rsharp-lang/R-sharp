
#' print hello content
#'
#' @description print content on the console with hello prefix
#'
#' @param x a character content vector for print on the console 
#'
#' @return content string vector that print on the console.
#' @keywords print, hello
#'
#' @remarks no additional remarks
#'
let hello.world as function(x = "world") {
	print(`hello ${x}!`);
}