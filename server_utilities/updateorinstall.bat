@echo off
setlocal

rem Read environment variables from the "env" file
for /f "usebackq tokens=1* delims==" %%a in ("env.txt") do (
    if "%%a"=="STEAM_USERNAME" set "steamusername=%%b"
    if "%%a"=="STEAM_PASSWORD" set "steampassword=%%b"
    if "%%a"=="BETA_NAME" set "betaname=%%b"
    if "%%a"=="ENABLE_BETA" set "enablebeta=%%b"
)

set "steamusername=%steamusername:\r=%"
set "steampassword=%steampassword:\r=%"
set "betaname=%betaname:\r=%"
set "enablebeta=%enablebeta:\r=%"

rem Log in to SteamCMD using the provided credentials
if "%enablebeta%"=="true" (
    "C:\Users\Administrator\Documents\steamcmd\steamcmd.exe" +force_install_dir "C:\Users\Administrator\Documents\battlebit" +login %steamusername% +app_update 671860 -beta %betaname% validate +quit
) else (
    "C:\Users\Administrator\Documents\steamcmd\steamcmd.exe" +force_install_dir "C:\Users\Administrator\Documents\battlebit" +login %steamusername% +app_update 671860 validate +quit
)

endlocal