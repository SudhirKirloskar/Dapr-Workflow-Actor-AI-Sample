@echo off

set LOGFILE=PaymentServiceLog.txt

powershell -Command  "dapr run --app-id paymentservice --app-port 5234 --dapr-http-port 3511 --resources-path ./components -- dotnet run --project .\PaymentServiceApp\PaymentServiceApp.csproj  | Tee-Object -FilePath %LOGFILE%"

pause

