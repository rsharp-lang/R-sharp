require(REnv);

data(law);
print(cor(law$LSAT, law$GPA));

n<-15
theta<-function(law){cor(law[,1],law[,2])}
result<-bootstrap(1:n,200,theta,law)$thetastar
hist(result,prob=T)
sd(result)