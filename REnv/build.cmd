@echo off

Rscript --build /save ./REnv.zip
R# --install.packages /module ./REnv.zip

REM pause