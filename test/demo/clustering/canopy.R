require(clustering);

setwd(@dir);

let rawdata = read.csv("./feature_regions.csv", row.names = 1, check.names = FALSE);
rawdata[, "Cluster"] = NULL;
rawdata[, "x"] = as.numeric(rawdata$x);
rawdata[, "y"] = as.numeric(rawdata$y);

print(rawdata, max.print = 13);

let k0 = canopy(rawdata);
let k = kmeans(rawdata, centers = length(k0),
                         bisecting = TRUE);
let kdf = as.data.frame(k0);

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