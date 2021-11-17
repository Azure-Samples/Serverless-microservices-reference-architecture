$ErrorActionPreference = 'Stop'

try {
    # cd /dotnet
    Push-Location ( Join-Path $PSScriptRoot ../dotnet ) -StackName scripts

    # Start each Function in a new console
    Push-Location ServerlessMicroservices.FunctionApp.Drivers
    Start-Process pwsh { -c func start --csharp }
    
}
finally {
    Pop-Location -StackName scripts
}
