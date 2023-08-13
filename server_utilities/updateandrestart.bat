@echo off
setlocal

rem Stop Server
call "stop.bat"

rem Update Server
call "updateorinstall.bat"

rem Start Server
call "start.bat"

endlocal