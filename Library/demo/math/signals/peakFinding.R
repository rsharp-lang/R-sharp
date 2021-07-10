imports "signalProcessing" from "signalKit";

`${dirname(@script)}/GUCA.csv`
|> read.csv
|> (function(signal) as.signal(signal[, "Time"], signal[, "Intensity"]))
|> print
;      
