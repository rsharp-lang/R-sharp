imports "bitmap" from "MLkit";

let data = bitmap::open("F:/iri3-he.bmp");
let copy = corp_rectangle(data, [0,0], [300,300])