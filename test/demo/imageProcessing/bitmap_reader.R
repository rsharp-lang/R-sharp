imports "bitmap" from "MLkit";

let data = bitmap::open("F:/iri3-he.bmp");
let copy = corp_rectangle(data, [10000,8000], [300,300]);

bitmap(copy, file = file.path(@dir, "corp.bmp"));