require(ggplot);

let center = as.data.frame( hist( rnorm(1000,0,1), n=10));
let right =as.data.frame( hist( rexp(1000, 1), n=10));
let left = as.data.frame( hist( -rexp(1000, 1), n=10));

print(center);
print(left);
print(right);

stop();

bitmap(file = file.path(@dir,"moments.png")) {
    ggplot(dataset , aes(x ="x"))
    + geom_point(aes(y = "center"))
    + geom_point(aes(y = "left"))
    + geom_point(aes(y = "right"))

}