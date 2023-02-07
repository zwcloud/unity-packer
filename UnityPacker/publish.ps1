dotnet publish -r linux-x64 --self-contained -c Release
if ($LASTEXITCODE -ne 0) {
    exit;
}
dotnet publish -r win-x64 --self-contained -c Release
