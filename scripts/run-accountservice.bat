@echo off

set LOGFILE=AccountServiceLog.txt

powershell -Command "dapr run --app-id accountservice --app-port 5236 --dapr-http-port 3512 --resources-path ./components -- dotnet run --project .\AccountServiceApp\AccountServiceApp.csproj  | Tee-Object -FilePath %LOGFILE%"

pause

