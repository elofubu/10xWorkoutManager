# Script to run all tests with code coverage and generate HTML report

Write-Host "Running tests with code coverage..." -ForegroundColor Green

# Clean previous coverage results
if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force "TestResults"
}

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory:"TestResults"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Generating coverage report..." -ForegroundColor Green

# Generate HTML report
reportgenerator `
    -reports:"TestResults/**/coverage.cobertura.xml" `
    -targetdir:"TestResults/CoverageReport" `
    -reporttypes:"Html;Cobertura;lcov" `
    -verbosity:"Info"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to generate coverage report!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Coverage report generated successfully!" -ForegroundColor Green
Write-Host "Opening report in browser..." -ForegroundColor Cyan

# Open the report
$reportPath = Join-Path $PSScriptRoot "TestResults\CoverageReport\index.html"
Start-Process $reportPath

Write-Host "Done! Coverage report opened in browser." -ForegroundColor Green



