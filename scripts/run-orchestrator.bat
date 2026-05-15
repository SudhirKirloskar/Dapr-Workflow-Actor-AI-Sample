@echo off

set LOGFILE=OrchestratorLog.txt

powershell -Command "dapr run --app-id orchestratorapp --app-port 5300 --resources-path ./components -- dotnet run --project .\OrchestratorApp\OrchestratorApp.csproj | Tee-Object -FilePath %LOGFILE%"

pause
