@echo off

SET R_HELP=../App/help/R#.exe
SET R_ENV=../App/net6.0-windows/R#.exe

start http://127.0.0.1/

if exist %R_HELP% (
    "%R_HELP%"  ./httpHelp.R
) else (
    "%R_ENV%"   ./httpHelp.R
)

PAUSE