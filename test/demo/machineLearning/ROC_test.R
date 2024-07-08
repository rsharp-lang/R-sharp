imports "validation" from "MLkit";

let pos = runif(800, 0.55, 1);
let neg = runif(200, 0, 0.7);

let data = data.frame(
    score = append(pos, neg), 
    label = append(
        rep(1, length(pos)), 
        rep(0, length(neg))
    )
);

print(data);

let roc = prediction(data$score, labels = data$label == 1, resolution= 1000);

print(AUC(roc));

bitmap(file = file.path(@dir, "roc.png")) {
    plot(roc);
}