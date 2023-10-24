require(REnv);

data(law);
print(cor(law$LSAT, law$GPA));

#设置bootstrap循环
let B <- 200; #重复抽样的次数
let n <- nrow(law); #样本大小（15个）
let R <- numeric(B); #储存每次循环后计算得到的相关系数

#对R的标准差进行bootstrap评估（通过计算每次抽样的标准差）
for (b in 1:B) {
  #randomly select the indices
  let i <- sample(1:n, size = n, replace = TRUE);
  # 从1:n的范围内有放回地抽取n个样本
  let LSAT <- law$LSAT[i] ;
  let GPA <- law$GPA[i];

  R[b] <- cor(LSAT, GPA);
}

se.R <- sd(R);
print(R);

#output
print(se.R);  #多次抽样的标准差即为标准误

bitmap(file = `${@dir}/law_bootstrap.png`) {
    plot(hist(R, prob = TRUE));
}
