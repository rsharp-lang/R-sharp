require(Matrix);

let problem = ~[
     2*x + y -   z =  8,
    -3*x - y + 2*z = -11,
    -2*x + y + 2*z = -3
];

let x = gauss_solve(problem);

print(x);

let problem_mat = data.frame(
    x = [ 2, -3, -2],
    y = [ 1, -1,  1],
    z = [-1,  2,  2]
);

print(problem_mat);

let x1 = gauss_solve(problem_mat, [8, -11, -3]);

print(x1);