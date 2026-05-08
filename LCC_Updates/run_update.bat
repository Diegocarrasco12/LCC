@echo off

set LOCAL_PATH=C:\ProgramData\LCC
set INSTALLER=%LOCAL_PATH%\setup.exe

mkdir %LOCAL_PATH% 2>nul

copy "\\192.168.1.71\LCC_Updates\LogisticControlCenter_Setup_v2.1.5.exe" %INSTALLER% /Y

start "" %INSTALLER%