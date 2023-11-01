@echo off

SET R_HOME=../App/net6.0
SET REnv="%R_HOME%/R#.exe"
SET updater=../studio/code_banner.R

%REnv% %updater% --banner-xml ./gpl3.xml --proj-folder ../

git add -A
git commit -m "update source file banner headers!"

pause