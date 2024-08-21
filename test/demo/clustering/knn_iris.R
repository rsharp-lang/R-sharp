imports "dataset" from "MLkit";
imports "clustering" from "MLkit";

data(bezdekIris);
print(bezdekIris);

# create training/test for demo
set.seed(123);

let [training, test] = split_training_test(bezdekIris, ratio = 0.7);
let predictions = knn(train = training[, -"class"], 
    test = test[, -"class"], cl = training$class, k = 3); 

print(predictions);