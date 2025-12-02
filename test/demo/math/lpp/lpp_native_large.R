# ===================================================================
# R 脚本：使用 lpSolve 求解一个包含1000个变量的线性规划问题
#
# 功能描述:
# 1. 随机生成一个包含1000个决策变量和3个约束条件的线性规划问题。
# 2. 将目标函数和约束条件以可读的字符串格式打印出来。
# 3. 调用 lpSolve 进行求解。
# 4. 打印求解结果的摘要信息。
#
# 日期: 2025年12月02日
# ===================================================================

# 第1步: 安装并加载必要的包
# 如果尚未安装 lpSolve，请取消下面一行的注释并运行
# install.packages("lpSolve")

library(lpSolve)

# 第2步: 设置问题参数
# 为了结果可复现，我们设置一个随机数种子
set.seed(123)

# 定义问题的规模
num_vars <- 1000          # 决策变量的数量
num_constraints <- 100      # 约束条件的数量

# 第3步: 随机生成线性规划问题的数据

# 3.1 目标函数系数 (c vector)
# 我们的目标是最大化 Z = c1*x1 + c2*x2 + ... + c1000*x1000
# 生成1000个在 [1, 20] 范围内的随机系数
objective_coeffs <- runif(num_vars, min = 1, max = 20)

# 3.2 约束条件矩阵 (A matrix)
# 约束形式为: A * x <= b
# 生成一个 3x1000 的矩阵，系数在 [0.1, 5] 范围内
constraint_matrix <- matrix(runif(num_constraints * num_vars, min = 0.1, max = 5), 
                             nrow = num_constraints, 
                             ncol = num_vars)

# 3.3 约束方向
# 所有约束都设置为 "小于等于"
constraint_dir <- rep("<=", num_constraints)

# 3.4 约束右侧值
# 生成3个右侧值，确保它们足够大，使得问题有可行解
# 我们将每行系数的和乘以一个系数来生成RHS，确保可行性
row_sums <- rowSums(constraint_matrix)
constraint_rhs <- row_sums * runif(num_constraints, min = 0.4, max = 0.6)


# 第4步: 构建并打印问题的字符串表示

cat("--- 线性规划问题定义 ---\n\n")

# 4.1 打印目标函数
# 为了可读性，只打印前3项和最后一项
obj_terms <- paste0(sprintf("%.2f", objective_coeffs[1:3]), "*x", 1:3, collapse = " + ")
obj_last_term <- paste0(sprintf("%.2f", objective_coeffs[num_vars]), "*x", num_vars)
objective_string <- paste0("最大化 Z = ", obj_terms, " + ... + ", obj_last_term)

std_out = paste0(sprintf("%.2f", objective_coeffs), "*x", 1:length(objective_coeffs), collapse = " + ");
std_out = c(std_out,"\n");

cat("目标函数:\n")
cat(objective_string, "\n\n")

# 4.2 打印约束条件
cat("约束条件:\n")
constraint_strings <- c()
for (i in 1:num_constraints) {
  coeffs_i <- constraint_matrix[i, ]
  # 同样，只打印前3项和最后一项
  terms_i <- paste0(sprintf("%.2f", coeffs_i[1:3]), "*x", 1:3, collapse = " + ")
  last_term_i <- paste0(sprintf("%.2f", coeffs_i[num_vars]), "*x", num_vars)
  
  std_out = c(std_out, sprintf("%s <= %s,",  paste0(sprintf("%.2f", coeffs_i), "*x", 1:length(coeffs_i), collapse = " + "),constraint_rhs[i]  ) );
  
  constraint_string_i <- paste0("约束", i, ": ", terms_i, " + ... + ", last_term_i, " ", constraint_dir[i], " ", sprintf("%.2f", constraint_rhs[i]))
  constraint_strings <- c(constraint_strings, constraint_string_i)
}
cat(paste(constraint_strings, collapse = "\n"))
cat("\n\n")

# 第5步: 调用 lpSolve 求解问题
cat("--- 开始求解 ---\n\n")

# lp(direction, objective.in, const.mat, const.dir, const.rhs, ...)
lp_result <- lp(direction = "max",
                objective.in = objective_coeffs,
                const.mat = constraint_matrix,
                const.dir = constraint_dir,
                const.rhs = constraint_rhs)

# 第6步: 打印求解结果
cat("--- 求解结果 ---\n\n")

# 检查求解状态
# 0: 成功找到最优解
# 2: 问题无解
# 其他值表示其他状态
if (lp_result$status == 0) {
  cat("求解状态: 成功找到最优解\n\n")
  
  # 打印目标函数的最优值
  cat("目标函数最优值 (Z): ", sprintf("%.4f", lp_result$objval), "\n\n")
  
  # 打印决策变量值的摘要
  solution_vector <- lp_result$solution
  cat("决策变量值摘要:\n")
  print(summary(solution_vector))
  
  # 计算并打印非零变量的数量（使用一个很小的阈值，如 1e-9）
  non_zero_vars <- sum(solution_vector > 1e-9)
  cat("\n非零变量个数: ", non_zero_vars, "\n")
  
  # (可选) 打印前10个非零变量的值和索引
  if (non_zero_vars > 0) {
    non_zero_indices <- which(solution_vector > 1e-9)
    cat("\n前10个非零变量的值:\n")
    top_indices <- non_zero_indices; # head(non_zero_indices, 10)
    for (idx in top_indices) {
      cat(sprintf("  x%-4d = %.4f\n", idx, solution_vector[idx]))
    }
  }
  
} else {
  cat("求解状态: 未找到最优解 (状态码: ", lp_result$status, ")\n")
  cat("这可能意味着问题是不可行的或无界的。\n")
}

writeLines(std_out, con = "large_lpp.txt");

cat("\n--- 脚本结束 ---\n")
