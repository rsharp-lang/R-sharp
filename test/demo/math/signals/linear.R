imports "signalProcessing" from "signalKit";

setwd(@dir);

let raw_signal = read.csv("./GUCA.csv", 
    row.names = NULL, 
    check.names = FALSE);

raw_signal = raw_signal[order(raw_signal$Time), ];

print(raw_signal, max.print = 6);

let x_axis = range(raw_signal$Time);
let x_diff = diff(raw_signal$Time);

print(x_axis);
print(x_diff);

x_axis = min(x_axis):max(x_axis) step mean(x_diff);

print("x axis for do signal data re-sampling:");
print(x_axis);

let signal = as.signal(raw_signal$Time, raw_signal$Intensity);
let linear = resampler(signal, max.dx = mean(x_diff) * 10);

let interpolate = data.frame(time = x_axis, intensity = linear(x_axis));

print(interpolate, max.print = 6);

write.csv(interpolate, file = "./linear_sampler.csv", row.names = FALSE);
