var Ct = [1,2,3,4,5]
var At = Ct ^ 2.5
var linear = lm(Ct ~ At, data = data.frame(Ct, At), weights = 1 / (At ^ 2))

print(linear);

// Call:
// lm(formula = Ct ~ At, data = data.frame(Ct, At), weights = 1/(At^2))
//
// Weighted Residuals:
//        1        2        3        4        5 
// -0.01219  0.07374  0.01283 -0.02535 -0.04902 
//
// Coefficients:
//             Estimate Std. Error t value Pr(>|t|)    
// (Intercept)  0.88964    0.06454   13.78 0.000826 ***
// At           0.12255    0.02939    4.17 0.025105 *  
// ---
// Signif. codes:  0 ‘***’ 0.001 ‘**’ 0.01 ‘*’ 0.05 ‘.’ 0.1 ‘ ’ 1
//
// Residual standard error: 0.05415 on 3 degrees of freedom
// Multiple R-squared:  0.8529,	Adjusted R-squared:  0.8038 
// F-statistic: 17.39 on 1 and 3 DF,  p-value: 0.0251
