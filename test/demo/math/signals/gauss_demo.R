imports "signalProcessing" from "signalKit";

let v = [0, 0.1, 0.2, 0.5, 0.9, 1.3, 1.25, 0.99, 0.7, 0.35, 0.4, 0.5, 0.6, 0.65, 0.45, 0.4, 0.35, 0.2, 0.1, 0];
let peaks = gaussian_fit(v,  max_peaks = 8, gauss_clr = TRUE);

print(as.data.frame(peaks));
# print(peaks);

setwd(@dir);

bitmap(file = "./demo_vector.png", padding = [100, 200, 150, 200], fill = "white") {
    plot((1:length(v))/length(v), v,    line = TRUE);
}