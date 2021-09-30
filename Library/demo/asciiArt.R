require(graphics2D);

setwd(@dir);

"1537192287563.jpg"
|> readImage
|> resizeImage(factor = 1)
|> asciiArt
|> writeLines(con = "bilibili.txt")
;