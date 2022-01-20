> cortest_psy <- corr.test(attitude[1:3], attitude[4:6], method = "pearson")
> cortest_psy_sdj <- corr.test(attitude[1:3], attitude[4:6], method = "pearson", adjust = "fdr")
> cortest_psy
Call:corr.test(x = attitude[1:3], y = attitude[4:6], method = "pearson")
Correlation matrix 
           learning raises critical
rating         0.62   0.59     0.16
complaints     0.60   0.67     0.19
privileges     0.49   0.45     0.15
Sample Size 
[1] 30
These are the unadjusted probability values.
  The probability values  adjusted for multiple tests are in the p.adj object. 
           learning raises critical
rating         0.00   0.00     0.41
complaints     0.00   0.00     0.32
privileges     0.01   0.01     0.44

 To see confidence intervals of the correlations, print with the short=FALSE option
> cortest_psy_sdj
Call:corr.test(x = attitude[1:3], y = attitude[4:6], method = "pearson", 
    adjust = "fdr")
Correlation matrix 
           learning raises critical
rating         0.62   0.59     0.16
complaints     0.60   0.67     0.19
privileges     0.49   0.45     0.15
Sample Size 
[1] 30
These are the unadjusted probability values.
  The probability values  adjusted for multiple tests are in the p.adj object. 
           learning raises critical
rating         0.00   0.00     0.41
complaints     0.00   0.00     0.32
privileges     0.01   0.01     0.44

 To see confidence intervals of the correlations, print with the short=FALSE option