imports "signalProcessing" from "signalKit";

# create test signal_data
let peaks = gaussian_peak( center = [1.6, 4.9, 2.3, 1.944, 5.5],
                                 height = [1 2 6 0.5 0.3 1.1114] ,
                                width = [0.12 0.5 1.6 0.999 2.1112]) ;

setwd(@dir);

bitmap(file = "./view_data.png") {
    plot(peaks, x = 1:10 step 0.5);
}

let signal = as.signal(1:10 step 0.25, peaks);

print(as.data.frame(signal));

let peaks_guess = gaussian_fit(signal,  max_peaks = 8, gauss_clr = TRUE);

bitmap(file = "./decompose_peaks.png") {
    plot(peaks_guess, x = 1:10 step 0.5);
}