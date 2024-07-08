imports "bitmap" from "MLkit";

let data = bitmap::open("F:/iri3-he.bmp");
let copy = corp_rectangle(data, [0,0], [900,900]);

bitmap(copy, file = file.path(@dir, "corp.bmp"));