@echo off
setlocal

REM Full offline comparison pipeline for the R# glmnet package wrapper.
REM Run from anywhere; all paths are absolute on purpose.

set "RSCRIPT=C:\Program Files\R\R-4.5.0\bin\Rscript.exe"
set "RSHARP=G:\GCModeller\src\R-sharp\App\net10.0\R#.exe"
set "ROOT=G:\GCModeller\src\R-sharp\test\glmnet"
set "SLN=G:\GCModeller\src\R-sharp\R_system.NET5.slnx"

if not exist "%ROOT%\data" mkdir "%ROOT%\data"
if not exist "%ROOT%\result" mkdir "%ROOT%\result"
if not exist "%ROOT%\report" mkdir "%ROOT%\report"
del /q "%ROOT%\result\*.csv" 2>nul

echo [1/7] Verify GNU R 4.5.0 / glmnet 5.1 toolchain
"%RSCRIPT%" "%ROOT%\R\check_version.R" || goto :fail

echo [2/7] Export GNU R golden data
"%RSCRIPT%" "%ROOT%\R\export_golden.R" "%ROOT%\data" || goto :fail

echo [3/7] Build solution Rsharp_app_release x64
dotnet build "%SLN%" -c Rsharp_app_release -p:Platform=x64 -nologo -clp:ErrorsOnly || goto :fail

echo [4/7] R# demo_fit.R
"%RSHARP%" "%ROOT%\demo_fit.R"

echo [5/7] R# demo_cv.R
"%RSHARP%" "%ROOT%\demo_cv.R"

echo [6/7] R# demo_predict.R
"%RSHARP%" "%ROOT%\demo_predict.R"

echo [7/7] GNU R compare.R (final verdict)
"%RSCRIPT%" "%ROOT%\R\compare.R" "%ROOT%\data" "%ROOT%\result" "%ROOT%\report" || goto :fail

echo.
echo OK: glmnet comparison test passed.
exit /b 0

:fail
echo.
echo FAILED: glmnet comparison test did not complete successfully.
exit /b 1
