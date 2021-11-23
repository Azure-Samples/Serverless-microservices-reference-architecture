$ErrorActionPreference = 'Stop'

try {
    # cd /dotnet/ServerlessMicroservices.Seeder
    Push-Location ( Join-Path $PSScriptRoot ../dotnet/ServerlessMicroservices.Seeder )
    dotnet run seed --seeddriversurl http://localhost:7071
    
    $json = Get-Content -Path ( Join-Path $PSScriptRoot ../test/testparams.json )
    Invoke-RestMethod -Method POST -Uri http://localhost:7072/api/triptestparameters -Body $json
    
    dotnet run testTrips --seeddriversurl http://localhost:7071 --testurl http://localhost:7072/api/triptestparameters

    start 'http://localhost:7072/api/activetrips'
    
}
finally {
    Pop-Location
}
