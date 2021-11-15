# Restore, Build and Test (don't publish)

Push-Location (Join-Path $PSScriptRoot ../dotnet)

try {
    dotnet restore
    dotnet build -c Release
    dotnet test -c Release
}
finally {
    Pop-Location
}
