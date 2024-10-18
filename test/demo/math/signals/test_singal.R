imports "signalProcessing" from "signalKit";

let t0 = 0;
let t1 = 1800;

let x = t0:t1 step 0.1;

let s1(x) = signalProcessing::gaussian(x, 300,300,5);
let s2(x) = signalProcessing::gaussian(x, 900,990,5);
let s3(x) = signalProcessing::gaussian(x, 630,1690,11);
let s4(x) = signalProcessing::gaussian(x, 130,1090,13);
let s5(x) = signalProcessing::gaussian(x, 1130,1029,6);


let y = s1(x) + s2(x)+s3(x)+s4(x)+s5(x) ;


pdf(file = file.path(@dir, "gauss_signal_peaks.pdf")) {
	plot(x,y , fill = "white", point.size = 5);
};

let sig = as.signal(x,y);

sig = findpeaks(
	sig,
	baseline = 0.7,
	cutoff   = 8
)
|> as.data.frame()
|> t()
|> print()
;      
