@echo off

SET R_HOME=D:\GCModeller\src\R-sharp\App\net5.0
SET Rscript="%R_HOME%\Rscript.exe"

%Rscript% --build /save ./REnv.zip
"%R_HOME%/R#.exe" --install.packages /module ./REnv.zip

pause