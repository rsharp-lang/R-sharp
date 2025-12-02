# 安装和加载必要的包
if (!require(lpSolve)) {
  install.packages("lpSolve")
  library(lpSolve)
}

# 设置随机种子以确保结果可重现
set.seed(123)

# 1. 定义目标函数：最大化利润
# 10个变量，代表10种产品的产量
objective_coef <- sample(1:20, 10, replace = TRUE)
cat("目标函数系数（单位利润）:\n")
print(objective_coef)

# 2. 定义约束条件
# 4个资源约束
constraint_matrix <- matrix(
  data = sample(1:10, 40, replace = TRUE),
  nrow = 4,
  ncol = 10
)
cat("\n约束矩阵:\n")
print(constraint_matrix)

constraint_rhs <- c(100, 150, 200, 80)  # 资源上限
constraint_dir <- rep("<=", 4)  # 约束方向

# 3. 打印问题方程式
cat("\n=== 线性规划问题方程式 ===\n")

# 打印目标函数方程式
obj_terms <- paste0(objective_coef, "*x", 1:10)
obj_equation <- paste("最大化 z =", paste(obj_terms, collapse = " + "))
cat("目标函数: ", obj_equation, "\n\n")

# 打印约束条件方程式
cat("约束条件:\n")
for(i in 1:nrow(constraint_matrix)) {
  # 构建每个约束的左边部分
  constraint_terms <- paste0(constraint_matrix[i,], "*x", 1:10)
  constraint_lhs <- paste(constraint_terms, collapse = " + ")
  
  # 完整约束方程式
  constraint_eq <- paste(constraint_lhs, constraint_dir[i], constraint_rhs[i])
  cat(paste0("约束", i, ": ", constraint_eq, "\n"))
}
cat("\n")

# 4. 求解线性规划问题
solution <- lp(
  direction = "max",
  objective.in = objective_coef,
  const.mat = constraint_matrix,
  const.dir = constraint_dir,
  const.rhs = constraint_rhs
)

# 5. 输出结果
cat("=== 线性规划求解结果 ===\n")
if (solution$status == 0) {
  cat("求解状态: 成功\n")
  cat("最优解（各产品产量）:\n")
  names(solution$solution) <- paste0("x", 1:10)
  print(solution$solution)
  cat("最优目标函数值（总利润）:", solution$objval, "\n")
} else {
  cat("求解状态: 无可行解（状态码:", solution$status, "）\n")
}

# 6. 整数规划示例
cat("\n=== 整数线性规划求解 ===\n")
solution_int <- lp(
  direction = "max",
  objective.in = objective_coef,
  const.mat = constraint_matrix,
  const.dir = constraint_dir,
  const.rhs = constraint_rhs,
  all.int = TRUE  # 所有变量为整数
)

if (solution_int$status == 0) {
  cat("整数规划最优解:\n")
  names(solution_int$solution) <- paste0("x", 1:10)
  print(solution_int$solution)
  cat("整数规划最优利润:", solution_int$objval, "\n")
} else {
  cat("整数规划无可行解\n")
}

# 目标函数系数（单位利润）:
#  [1] 15 19 14  3 10 18 11  5 20 14

# 约束矩阵:
#      [,1] [,2] [,3] [,4] [,5] [,6] [,7] [,8] [,9] [,10]
# [1,]    6    3    3   10    1    7    7    6    8     9
# [2,]    9    9    8    9    7    9    5    9    2     6
# [3,]   10    9   10    3    5    9    7    2    1     5
# [4,]    5    9    7    4   10   10    5    5    9     9

# === 线性规划问题方程式 ===
# 目标函数:  最大化 z = 15*x1 + 19*x2 + 14*x3 + 3*x4 + 10*x5 + 18*x6 + 11*x7 + 5*x8 + 20*x9 + 14*x10 

# 约束条件:
# 约束1: 6*x1 + 3*x2 + 3*x3 + 10*x4 + 1*x5 + 7*x6 + 7*x7 + 6*x8 + 8*x9 + 9*x10 <= 100
# 约束2: 9*x1 + 9*x2 + 8*x3 + 9*x4 + 7*x5 + 9*x6 + 5*x7 + 9*x8 + 2*x9 + 6*x10 <= 150
# 约束3: 10*x1 + 9*x2 + 10*x3 + 3*x4 + 5*x5 + 9*x6 + 7*x7 + 2*x8 + 1*x9 + 5*x10 <= 200
# 约束4: 5*x1 + 9*x2 + 7*x3 + 4*x4 + 10*x5 + 10*x6 + 5*x7 + 5*x8 + 9*x9 + 9*x10 <= 80

# === 线性规划求解结果 ===
# 求解状态: 成功
# 最优解（各产品产量）:
#  x1  x2  x3  x4  x5  x6  x7  x8  x9 x10 
#  16   0   0   0   0   0   0   0   0   0 
# 最优目标函数值（总利润）: 240 

# === 整数线性规划求解 ===
# 整数规划最优解:
#  x1  x2  x3  x4  x5  x6  x7  x8  x9 x10 
#  16   0   0   0   0   0   0   0   0   0 
# 整数规划最优利润: 240 