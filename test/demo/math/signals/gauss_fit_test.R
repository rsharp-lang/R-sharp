imports "signalProcessing" from "signalKit";

# create test signal_data
let peaks = gaussian_peak( center = [3.6, 4.9, 3.3, 3.944, 6.5],
                                 height = [20000 23333 64444 52222 30000 11114] ,
                                width = [0.12 0.5 0.6 0.399 0.71112]) ;

setwd(@dir);

bitmap(file = "./view_data.png") {
    plot(peaks, x = 1:10 step 0.5, sin = FALSE);
}

let signal = as.signal(1:10 step 0.5, peaks);
let sig_df = as.data.frame(signal);

print(sig_df, max.print = 6);

print(sig_df$x);
print(sig_df$y);

let peaks_guess = gaussian_fit(signal,  max_peaks = 5, 
  gauss_clr = TRUE, sine_kernel = FALSE);

print(as.data.frame(peaks_guess));

bitmap(file = "./decompose_peaks.png") {
    plot(peaks_guess, x = 1:10 step 0.1, sin = FALSE);
}