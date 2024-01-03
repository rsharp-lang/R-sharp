imports "signalProcessing" from "signalKit";
imports "symbolic" from "Rlapack";

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

let formulas = gp_fit(sig_df$x, sig_df$y);

print(as.data.frame(formulas));