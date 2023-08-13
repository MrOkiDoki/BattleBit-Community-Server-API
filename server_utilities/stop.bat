@echo off
setlocal

rem Find and terminate the BattleBit game server process
for /f "tokens=2 delims=," %%a in ('tasklist /nh /fi "imagename eq BattleBit.exe" /fo csv') do (
    taskkill /pid %%a /f
)

echo BattleBit game server has been stopped.

endlocal