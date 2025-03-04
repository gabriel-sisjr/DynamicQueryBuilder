# Define paths
$rootDir = Resolve-Path "$PSScriptRoot/.."
$libProject = "$rootDir/src/DynamicQueryBuilder/DynamicQueryBuilder.csproj"
$exampleProjects = @(
    "$rootDir/examples/ClientWebAPI/ClientWebAPI.csproj",
    "$rootDir/examples/ConsoleClient/ConsoleClient.csproj"
)
$nugetOutputDir = "$rootDir/packages"

# Ensure a fresh start by removing old packages
Write-Host "🗑 Removing old NuGet packages..."
if (Test-Path $nugetOutputDir) {
    Remove-Item -Recurse -Force $nugetOutputDir
}
New-Item -ItemType Directory -Path $nugetOutputDir | Out-Null

# Pack the library
Write-Host "📦 Packing the project..."
dotnet pack $libProject --configuration Release --output $nugetOutputDir

# Get the latest package version
$packageFile = Get-ChildItem -Path $nugetOutputDir -Filter "*.nupkg" | Select-Object -ExpandProperty Name
$packageName = $packageFile -replace "\.nupkg$", ""

Write-Host "✅ Package generated: $packageFile"

# Reset the local NuGet source
Write-Host "🔄 Resetting NuGet source..."
dotnet nuget remove source LocalNuGet | Out-Null
dotnet nuget add source $nugetOutputDir --name LocalNuGet

# Install the package in example projects
foreach ($project in $exampleProjects) {
    Write-Host "📥 Installing $packageName in $project..."
    dotnet add $project package $packageName --source $nugetOutputDir --package-directory $nugetOutputDir
}

# Restore packages
Write-Host "🔄 Restoring packages..."
dotnet restore --ignore-failed-sources

# Run the ClientWebAPI project for testing
Write-Host "🚀 Running ClientWebAPI..."
dotnet run --project "$rootDir/examples/ClientWebAPI/ClientWebAPI.csproj"

Write-Host "✅ Process completed successfully!"
