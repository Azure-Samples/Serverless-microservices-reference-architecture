# NOTE: Install latest versions of Node and NPM: https://nodejs.org/en/download/
# NOTE: Install latest version of func CLI: https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local

$ErrorActionPreference = 'Stop'

try {

    # cd /dotnet
    Push-Location ( Join-Path $PSScriptRoot ../dotnet )

    Write-Host 'Install and start the storage emulator...'
    #   If this fails with error EADDRINUSE it is because Visual Studio has already started azurite (which is fine)
    Start-Process pwsh { -c md __azurite__ -Force && npm install -g azurite && azurite --silent -l __azurite__ }

    Write-Host 'Build and host SPA...'
    Start-Process pwsh { -c cd ../web/serverless-microservices-web && npm install && copy ../../test/settings.example.js ./public/js/settings.js && npm run serve -- --port 4280 }

    Write-Host 'Build and start Trip Archiver Nodejs Function...'
    Start-Process pwsh { -c cd ../nodejs/serverless-microservices-functionapp-triparchiver && npm install && npm run pack && func start --javascript -p 7075 --cors http://localhost:4280 }

    Write-Host 'Build and start Drivers Function...'
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Drivers && func start --csharp -p 7071 --cors http://localhost:4280 }
    
    Write-Host 'Build and start Trips Function...'
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Trips && func start --csharp -p 7072 --cors http://localhost:4280 }
    
    Write-Host 'Build and start Passengers Function...'
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Passengers && func start --csharp -p 7073 --cors http://localhost:4280 }
    
    Write-Host 'Build and start Orchestrators Function...'
    Start-Process pwsh { -c cd ServerlessMicroservices.FunctionApp.Orchestrators && func start --csharp -p 7074 --cors http://localhost:4280 }

    Write-Host 'When all builds have finished, open browser at http://localhost:4280/'

}
finally {
    Pop-Location
}
