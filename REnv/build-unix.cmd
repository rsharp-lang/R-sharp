@echo off

SET Rscript="D:\GCModeller\src\R-sharp\App\net5.0\Rscript.exe"

%Rscript% --build /save ./REnv.zip
R# --install.packages /module ./REnv.zip

pause