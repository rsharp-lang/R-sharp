let data = list(1,2,3);

lapply(tqdm(data),x -> sleep(1));

setwd(@dir);

sink(file = "./tqdm_output.log");

lapply(tqdm(data),x -> sleep(1));

sink();