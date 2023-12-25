imports "signalProcessing" from "signalKit";

require(clustering);

setwd(@dir);

let raw_signal = read.csv("./GUCA.csv", 
    row.names = NULL, 
    check.names = FALSE);
let signal = as.signal(raw_signal$Time, raw_signal$Intensity);
let gauss = gmm(gaussian_bin(signal, max = 30), components = 9);

print(gmm.predict_proba(gauss));