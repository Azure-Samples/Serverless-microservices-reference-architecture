# NOTE: Install latest versions of Node and NPM: https://nodejs.org/en/download/
# NOTE: Install latest version of func CLI: https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local

$ErrorActionPreference = 'Stop'

try {    
    # cd /dotnet
    Push-Location ( Join-Path $PSScriptRoot ../dotnet ) -StackName scripts

    # Build and host SPA at http://127.0.0.1:8080/
    Start-Process pwsh { -c cd ../web/serverless-microservices-web && npm install && copy ../../test/settings.example.js web/js/settings.js && npm run serve }

    # Build and start Trip Archiver Nodejs Function
    Start-Process pwsh { -c cd ../nodejs/serverless-microservices-functionapp-triparchiver && npm install && npm run pack && func start --javascript -p 7075 }

    # Start each Function in a new console. Give each one a head start to avoid collisions building shared DLLs
    # Build and start Drivers Function
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Drivers && func start --csharp -p 7071 }
    
    Start-Sleep -Seconds 2
    # Build and start Trips Function
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Trips && func start --csharp -p 7072 }
    
    Start-Sleep -Seconds 2
    # Build and start Passengers Function
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Passengers && func start --csharp -p 7073 }
    
    Start-Sleep -Seconds 2
    # Build and start Orchestrators Function
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Orchestrators && func start --csharp -p 7074 }

    Start-Sleep -Seconds 90
    # Open the browser
    start 'http://localhost:4280/'

}
finally {
    Pop-Location -StackName scripts
}
