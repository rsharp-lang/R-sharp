REM @echo off

SET RENV=../App/net6.0-windows/R#.exe
SET MAN="../docs/documents"

"%RENV%" --man.1 --debug --out %MAN%

pause