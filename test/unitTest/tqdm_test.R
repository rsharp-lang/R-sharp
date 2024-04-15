let data = list(1,2,3,4,5,6,7,8,9,1,0,11,12,13,465,456,48,45,66,1,231,3,465,456,6464);

lapply(tqdm(data),x -> sleep(0.5));

setwd(@dir);

sink(file = "./tqdm_output.log");

lapply(tqdm(data),x -> sleep(0.5));

sink();