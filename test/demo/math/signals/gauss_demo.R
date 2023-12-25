imports "signalProcessing" from "signalKit";

let peaks = gaussian_fit([0, 0.1, 0.2, 0.5, 0.9, 0.4, 0.35, 0.2, 0.1, 0],  max_peaks = 2);

print(peaks);