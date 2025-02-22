imports "randomForest" from "MLkit";
imports "dataset" from "MLkit";

data(bezdekIris);

print(bezdekIris);

bezdekIris <- as.MLdataset(bezdekIris,
                                    labels = "class",
                                    in_memory = TRUE);

print(bezdekIris);