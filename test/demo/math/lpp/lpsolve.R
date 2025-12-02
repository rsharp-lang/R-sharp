imports "lpSolve" from "Rlapack";

let obj = lpSolve::lp.max("z", 15*x1 + 19*x2 + 14*x3 + 3*x4 + 10*x5 + 18*x6 + 11*x7 + 5*x8 + 20*x9 + 14*x10);
let subj_to = lpSolve::subject_to(
    6*x1 + 3*x2 + 3*x3 + 10*x4 + 1*x5 + 7*x6 + 7*x7 + 6*x8 + 8*x9 + 9*x10 <= 100,
    9*x1 + 9*x2 + 8*x3 + 9*x4 + 7*x5 + 9*x6 + 5*x7 + 9*x8 + 2*x9 + 6*x10 <= 150,
	10*x1 + 9*x2 + 10*x3 + 3*x4 + 5*x5 + 9*x6 + 7*x7 + 2*x8 + 1*x9 + 5*x10 <= 200,
	5*x1 + 9*x2 + 7*x3 + 4*x4 + 10*x5 + 10*x6 + 5*x7 + 5*x8 + 9*x9 + 9*x10 <= 80
);
let lpp = lp(obj, subj_to);

str(lpp$solution);

print("objective value:");
print(lpp$objective);

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

