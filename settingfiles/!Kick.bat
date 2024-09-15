@echo off
set yyyyMMdd=%date:/=%
echo.
echo ＹＧＷ対象ファイルチェック[%yyyyMMdd%]
echo.
RefreshYGW.exe >> .\%yyyyMMdd%_RefreshYGW.log
