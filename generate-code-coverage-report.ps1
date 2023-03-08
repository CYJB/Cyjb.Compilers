dotnet test --no-build
dotnet reportgenerator -reports:.\TestCompilers\coverage.cobertura.xml -targetdir:.\CodeCoverage
