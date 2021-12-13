# NOTE: Install latest versions of Node and NPM: https://nodejs.org/en/download/
# NOTE: Install latest version of func CLI: https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local
# NOTE: Start Azure Storage Emulator https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator

$ErrorActionPreference = 'Stop'

try {    
    # cd /dotnet
    Push-Location ( Join-Path $PSScriptRoot ../dotnet ) -StackName scripts

    # Build and host SPA at http://127.0.0.1:8080/
    Start-Process pwsh { -c cd ../web/serverless-microservices-web && npm install && copy ../../test/settings.example.js ./public/js/settings.js && npm run serve -- --port 4280 }

    # Build and start Trip Archiver Nodejs Function
    Start-Process pwsh { -c cd ../nodejs/serverless-microservices-functionapp-triparchiver && npm install && npm run pack && func start --javascript -p 7075 --cors http://localhost:4280 }

    # Start each Function in a new console. Give each one a head start to avoid collisions building shared DLLs
    # Build and start Drivers Function
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Drivers && func start --csharp -p 7071 --cors http://localhost:4280 }
    
    Start-Sleep -Seconds 2
    # Build and start Trips Function
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Trips && func start --csharp -p 7072 --cors http://localhost:4280 }
    
    Start-Sleep -Seconds 2
    # Build and start Passengers Function
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Passengers && func start --csharp -p 7073 --cors http://localhost:4280 }
    
    Start-Sleep -Seconds 2
    # Build and start Orchestrators Function
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Orchestrators && func start --csharp -p 7074 --cors http://localhost:4280 }

    Start-Sleep -Seconds 90
    # Open the browser
    start 'http://localhost:4280/'

}
finally {
    Pop-Location -StackName scripts
}
