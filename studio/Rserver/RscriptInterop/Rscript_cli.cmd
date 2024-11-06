@echo off

SET R_HOME=../../App/net6.0
SET Rscript="%R_HOME%/Rscript"
SET Rserver="%R_HOME%/Rserve"

%Rscript% /cli.dev ---echo /namespace RscriptCommandLine > ./Rscript.vb
%Rserver% /cli.dev ---echo /namespace RscriptCommandLine > ./Rserver.vb