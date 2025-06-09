require(Matrix);

let problem = read.csv(relative_work("./problem.csv"), row.names = NULL, check.names = FALSE);
problem[,"cost"] = 10 / (as.numeric(problem$score)+0.1);
problem[,"cost"] = problem$cost / max(problem$cost);

let result = Matrix::hungarian_assignments(problem, "a","b", "cost");

print(result);