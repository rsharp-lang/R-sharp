imports "bitmap" from "MLkit";

let data = bitmap::open("F:/iri3-he.bmp");
let scans = bitmap::scan_peaks(data, [0,0], [300, 300], threshold = 60);

print(scans);

bitmap(file = file.path(@dir, "draw_scans.png")) {
    plot(scans$x, scans$y, reverse = TRUE);
}