@echo off
echo Cleaning up previous Results and TestResults folders for Tests...
if exist "TestResults" rmdir /s /q "TestResults"

echo Running tests with coverage...
dotnet test --collect:"XPlat Code Coverage"

echo Generating coverage reports...
reportgenerator "-reports:TestResults/*/coverage.cobertura.xml" "-targetdir:TestResults/CoverageReport" -reporttypes:Html
reportgenerator "-reports:TestResults/*/coverage.cobertura.xml" "-targetdir:TestResults/CoverageReport" -reporttypes:TextSummary

echo.
echo Coverage report: TestResults/CoverageReport/index.html
type "TestResults\CoverageReport\Summary.txt"

echo.
echo Opening coverage report in browser...
start "" "TestResults\CoverageReport\index.html"