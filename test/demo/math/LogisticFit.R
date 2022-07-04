require(REnv);

data = classify_cancer();

print(data, max.print = 6);

model = glm(labels ~ V1 +V2+V3+V4+ V5, family = binomial(link = "logit"), data = data );