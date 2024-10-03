#!/bin/bash

PROJECT_NAME="MiriVoice"
OUTPUT_DIR="Mirivoice.Desktop/bin/osx"

appversion=$1

# Function to build for a specific architecture
build_for_arch() {
    local arch=$1
    echo "Building for $arch..."
    dotnet publish Mirivoice.Desktop/Mirivoice.Desktop.csproj -r osx-$arch -c Release -p:AssemblyVersion=$appversion -o "$OUTPUT_DIR/osx-$arch"
}

# Build for both architectures
build_for_arch "x64"
build_for_arch "arm64"

# Create the final output directory
mkdir -p "$FINAL_OUTPUT_DIR"

# Create universal binary
echo "Creating universal binary..."
lipo -create \
    "/Users/appveyor/projects/mirivoice/Mirivoice.Desktop/bin/osx/osx-x64" \
    "/Users/appveyor/projects/mirivoice/Mirivoice.Desktop/bin/osx/osx-arm64" \
    -output "Mirivoice.Desktop/bin/Universal/MiriVoice"

echo "Universal binary created successfully"

cp -a  /Users/appveyor/projects/mirivoice/Mirivoice.Desktop/bin/Universal/MiriVoice /Users/appveyor/projects/mirivoice/Mirivoice.Desktop/osxbuild/MiriVoice.app/Contents/MacOS