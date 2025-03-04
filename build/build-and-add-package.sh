#!/bin/bash

# Define paths
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LIB_PROJECT="$ROOT_DIR/src/DynamicQueryBuilder/DynamicQueryBuilder.csproj"
EXAMPLE_PROJECTS=(
    "$ROOT_DIR/examples/ClientWebAPI/ClientWebAPI/ClientWebAPI.csproj"
    "$ROOT_DIR/examples/ConsoleClient/ConsoleClient.csproj"
)
NUGET_OUTPUT_DIR="$ROOT_DIR/packages"
NUGET_SOURCE_NAME="LocalNuGet"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Helper function for logging
log() {
    echo -e "${BLUE}${1}${NC}"
}

success() {
    echo -e "${GREEN}${1}${NC}"
}

error() {
    echo -e "${RED}${1}${NC}"
}

# Clean up old packages
log "🗑 Cleaning up old NuGet packages..."
rm -rf "$NUGET_OUTPUT_DIR"
mkdir -p "$NUGET_OUTPUT_DIR"

# Pack the library
log "📦 Packing the project..."
dotnet pack "$LIB_PROJECT" --configuration Release --output "$NUGET_OUTPUT_DIR"

# Get the latest package version
PACKAGE_FILE=$(ls "$NUGET_OUTPUT_DIR"/*.nupkg | head -n 1)
PACKAGE_NAME=$(basename "$PACKAGE_FILE" .nupkg)

if [ -z "$PACKAGE_FILE" ]; then
    error "❌ No package was generated!"
    exit 1
fi

success "✅ Package generated: $PACKAGE_FILE"

# Reset the local NuGet source
log "🔄 Configuring NuGet source..."
dotnet nuget remove source "$NUGET_SOURCE_NAME" 2>/dev/null || true
dotnet nuget add source "$NUGET_OUTPUT_DIR" --name "$NUGET_SOURCE_NAME"

# Install the package in example projects
for PROJECT in "${EXAMPLE_PROJECTS[@]}"; do
    if [ ! -f "$PROJECT" ]; then
        error "❌ Project not found: $PROJECT"
        continue
    fi

    log "📥 Installing $PACKAGE_NAME in $PROJECT..."
    dotnet add "$PROJECT" package "$PACKAGE_NAME" --source "$NUGET_OUTPUT_DIR" --package-directory "$NUGET_OUTPUT_DIR"
done

# Restore dependencies
log "🔄 Restoring packages..."
dotnet restore --ignore-failed-sources

# Run the ConsoleClient project for testing
log "🚀 Running ConsoleClient..."
dotnet run --project "$ROOT_DIR/examples/ConsoleClient/ConsoleClient.csproj"

success "✅ Process completed successfully!"
