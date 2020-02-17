# Kinetics of influenza A virus infection in humans
# DOI: 10.3390/v7102875

require(base.math);

setwd(!script$dir);

# config kinetics parameters
let p     <- 3e-2;
let cc    <- 2;
let beta  <- 8.8e-6;
let delta <- 2.6;

# config of the initial status
let y0 = list(V=1.4e-2, T=4e8, I=0);
# create kinetics system
let Kinetics_of_influenza_A_virus_infection_in_humans = [
	T -> -beta * T * V,
	I ->  beta * T * V - delta * I,
	V ->     p * I     - cc * V
];

# do run kinetics system simulation
Kinetics_of_influenza_A_virus_infection_in_humans
:> deSolve(y0, a = 0, b = 7)
:> as.data.frame 
:> write.csv(file = "./Kinetics_of_influenza_A_virus_infection_in_humans.csv")
;
