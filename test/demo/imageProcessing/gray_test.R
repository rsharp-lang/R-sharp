require(filter);

setwd(@dir);

bitmap(file = "./lena-RTCP_gray.png") {
    "lena.jpg"
    |> readImage()
    |> RTCP_gray()
    ;
}
