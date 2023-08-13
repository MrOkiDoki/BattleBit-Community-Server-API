@echo off
setlocal

rem Read settings from config.txt
for /f "delims=" %%a in (config.txt) do (
    set "%%a"
)

set "battlebit_args=-batchmode -nographics -startserver -Name=%Name% -Password=%Password% -AntiCheat=%AntiCheat% -Hz=%Hz% -Port=%Port% -MaxPing=%MaxPing% -LocalIP=%LocalIP% -VoxelMode=%VoxelMode% -ConfigPath=%ConfigPath% -ApiEndpoint=%ApiEndpoint% -FixedSize=%FixedSize% -FirstSize=%FirstSize% -MaxSize=%MaxSize% -FirstGamemode=%FirstGamemode% -FirstMap=%FirstMap%"

echo /-----------------------------/
echo Server Settings:
echo Name: %Name%
echo Password: %Password%
echo AntiCheat: %AntiCheat%
echo Hz: %Hz%
echo Port: %Port%
echo MaxPing: %MaxPing%
echo LocalIP: %LocalIP%
echo VoxelMode: %VoxelMode%
echo ConfigPath: %ConfigPath%
echo ApiEndpoint: %ApiEndpoint%
echo FixedSize: %FixedSize%
echo FirstSize: %FirstSize%
echo MaxSize: %MaxSize%
echo FirstGamemode: %FirstGamemode%
echo FirstMap: %FirstMap%
echo /-----------------------------/
echo Launching the BattleBit game server...

rem Start the BattleBit game server
Start "" "C:\Users\Administrator\Documents\battlebit\BattleBit.exe" %battlebit_args%

endlocal