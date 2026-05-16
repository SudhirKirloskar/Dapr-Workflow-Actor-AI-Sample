@echo off

set URL=http://localhost:5300/start-transfer

echo Invoking workflow...

powershell -NoProfile -Command "$body = @{ from='A'; to='B'; amount=1700 } | ConvertTo-Json; $response = Invoke-RestMethod -Uri '%URL%' -Method POST -Body $body -ContentType 'application/json'; Write-Output ('Workflow scheduled with ID = ' + $response)"