imports "signalProcessing" from "signalKit";

`${dirname(@script)}/GUCA.csv`
|> read.csv
|> (function(signal) as.signal(signal[, "Time"], signal[, "Intensity"]))
|> findpeaks(
	baseline = 0.7,
	cutoff   = 8
)
|> as.data.frame
|> t
|> print
;      
