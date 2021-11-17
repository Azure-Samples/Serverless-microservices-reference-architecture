$ErrorActionPreference = 'Stop'

try {
    # cd /dotnet
    Push-Location ( Join-Path $PSScriptRoot ../dotnet ) -StackName scripts

    # Start each Function in a new console. Give each one a head start to avoid collisions building shared DLLs
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Drivers && func start --csharp -p 7071 }
    Start-Sleep -Seconds 2
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Trips && func start --csharp -p 7072 }
    Start-Sleep -Seconds 2
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Passengers && func start --csharp -p 7073 }
    Start-Sleep -Seconds 2
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Orchestrators && func start --csharp -p 7074 }
    
}
finally {
    Pop-Location -StackName scripts
}
