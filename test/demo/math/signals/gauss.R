imports "signalProcessing" from "signalKit";

setwd(@dir);

let signals_data = read.csv("./linear_sampler.csv", 
    row.names = NULL, check.names = FALSE);
let peaks = gaussian_fit(signals_data,  max_peaks = 6);

print(signals_data, max.print = 13);
print(peaks);