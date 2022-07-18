require(REnv);

imports "validation" from "MLkit";

data = classify_cancer();

print(data, max.print = 6);

model = glm(labels ~ V1 +V2+V3+V4+ V5, family = binomial(link = "logit", rate= 0.0001, iteration = 1000), data = data );

print(model);

y = predict(model, x = data);

print(y);

print(abs(y - data$labels));

print(mean(abs(y - data$labels)));

roc = prediction(y, data$labels);

print(`AUC: ${AUC(roc)}`);

bitmap(file = `${@dir}/logit_roc.png`) {
	plot(roc);
}