@echo off

SET R_HOME=../App
SET Rscript="%R_HOME%\Rscript.exe"
SET REnv="%R_HOME%/R#.exe"

%Rscript% --build /save ./REnv.zip
%REnv% --install.packages /module ./REnv.zip

pause