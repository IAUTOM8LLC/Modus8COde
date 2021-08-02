@echo off
cls
setlocal ENABLEEXTENSIONS
set KEY_NAME="HKLM\SOFTWARE\JavaSoft\Java Development Kit"
set VALUE_NAME=CurrentVersion
::
:: get the current version
::
FOR /F "usebackq skip=2 tokens=3" %%A IN (`REG QUERY %KEY_NAME% /v %VALUE_NAME% 2^>nul`) DO (
    set ValueValue=%%A
)
IF %ValueValue%==1.8 (
	ECHO "Correct version"
) ELSE (
	ECHO "Need to install JAVA SDK"
)
pause