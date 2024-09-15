@echo off
set logDate=%date:~0,4%-%date:~5,2%-%date:~8,2%
set yyyyMMdd=%date:/=%
echo.
echo ＹＧＷ対象ファイルチェック[%logDate%]
echo.
if not exist "%yyyyMMdd%_RefreshYGW.log" (
	echo ＹＧＷ対象ファイルチェック > %yyyyMMdd%_RefreshYGW.log
	echo. >> %yyyyMMdd%_RefreshYGW.log
)
echo 処理日時：[%logDate%%time:~0,8%] >> %yyyyMMdd%_RefreshYGW.log
RefreshYGW.exe >> %yyyyMMdd%_RefreshYGW.log
