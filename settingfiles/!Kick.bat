@echo off
set logDate=%date:~0,4%-%date:~5,2%-%date:~8,2%
set yyyyMMdd=%date:/=%
echo.
echo �x�f�v�Ώۃt�@�C���`�F�b�N[%logDate%]
echo.
if not exist "%yyyyMMdd%_RefreshYGW.log" (
	echo �x�f�v�Ώۃt�@�C���`�F�b�N > %yyyyMMdd%_RefreshYGW.log
	echo. >> %yyyyMMdd%_RefreshYGW.log
)
echo ���������F[%logDate%%time:~0,8%] >> %yyyyMMdd%_RefreshYGW.log
RefreshYGW.exe >> %yyyyMMdd%_RefreshYGW.log
