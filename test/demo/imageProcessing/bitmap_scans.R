imports "bitmap" from "MLkit";

let data = bitmap::open("F:/iri3-he.bmp");
let scans = bitmap::scan_peaks(data, [10000,8000], [300, 300], threshold = 45);

print(scans);

bitmap(file = file.path(@dir, "draw_scans.png")) {
    plot(scans$x, scans$y, reverse = TRUE);
}