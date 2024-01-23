require(clustering);

setwd(@dir);

let rawdata = read.csv("./feature_regions.csv", row.names = 1, check.names = FALSE);
rawdata[, "Cluster"] = NULL;
rawdata[, "x"] = as.numeric(rawdata$x);
rawdata[, "y"] = as.numeric(rawdata$y);

print(rawdata, max.print = 13);

let t0 = now();
let k0 = canopy(rawdata,seed =FALSE);
let t1 = now() - t0;
let kdf = as.data.frame(k0);
let canopy_seeds = canopy(rawdata, seed = TRUE);
let k_size = length(k0);
t0=now();
let k = kmeans(rawdata, centers = canopy_seeds);
let t2 = now()-t0;
t0=now();
let k_native = kmeans(rawdata, centers = k_size);
let t3 = now()-t0;

print(t1);
print(t2);
print(t2+t1);
print(t3);

print(kdf, max.print = 6);

write.csv(kdf, file = "./canopy.csv");

bitmap(file = `./canopy.png`) {
    plot(kdf$V1, kdf$V2, 
        class     = kdf$clusters, 
        grid.fill = "white",
        padding   = "padding: 125px 300px 200px 200px;",
        colorSet  = "paper"
    );
}


k = as.data.frame(k);

print(k, max.print = 6);

write.csv(k, file = "./canopy-kmeans.csv");

bitmap(file = `./canopy-kmeans.png`) {
    plot(as.numeric(k$x), as.numeric(k$y), 
        class     = k$Cluster, 
        grid.fill = "white",
        padding   = "padding: 125px 300px 200px 200px;",
        colorSet  = "paper"
    );
}


k_native = as.data.frame(k_native);

print(k_native, max.print = 6);

write.csv(k_native, file = "./canopy-native-kmeans.csv");

bitmap(file = `./canopy-kmeans.png`) {
    plot(as.numeric(k_native$x), as.numeric(k_native$y), 
        class     = k_native$Cluster, 
        grid.fill = "white",
        padding   = "padding: 125px 300px 200px 200px;",
        colorSet  = "paper"
    );
}