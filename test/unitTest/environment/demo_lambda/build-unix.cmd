@echo off

SET R_HOME=../../../../App\net6.0
SET Rscript="%R_HOME%\Rscript.exe"
SET REnv="%R_HOME%/R#.exe"

%Rscript% --build /save ./REnv.zip
%REnv% --install.packages /module ./REnv.zip

pause