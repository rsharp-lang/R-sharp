imports "bitmap" from "MLkit";

let data = readImage(file.path(@dir, "lena.jpg" ));
let regions = bitmap::slic(data);

