require(REnv);

data(law);
print(cor(law$LSAT, law$GPA));

const theta<-function(law){
    cor(law[,1],law[,2])
};
const result<-bootstrap(law,2000,theta)$thetastar;

bitmap(file = `${@dir}/law_bootstrap_func.png`) {
    plot(hist(result, prob = TRUE, n = 20));
}

# hist(result,prob=T)

print(sd(result));