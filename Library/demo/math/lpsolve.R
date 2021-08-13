imports "lpSolve" from "Rlapack";

const objective  = ~x1 + 9 * x2 + x3;
const subject_to = ~[
        x1 + 2 * x2 + 3 * x3 = 9,
    3 * x1 + 2 * x2 + 2 * x3 = 15
];
const lpp = lp(objective, subject_to, "max");

str(lpp$solution);

print("objective value:");
print(lpp$objective);

print("solve problem 2:");

# [1] 0.0 4.5 0.0
# the objective function is 40.5
str(lp(
	
	  ~     x1 + 9 * x2 +     x3,
	
	[
	  ~     x1 + 2 * x2 + 3 * x3 <= 9,
      ~ 3 * x1 + 2 * x2 + 2 * x3 <= 15
	],
	
	"max"

));

