#!/bin/bash

PROJECT_NAME="MiriVoice"
OUTPUT_DIR="Mirivoice.Desktop/bin/osx"
FINAL_OUTPUT_DIR="Mirivoice.Desktop/bin/Universal"
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
    "$OUTPUT_DIR/osx-x64" \
    "$OUTPUT_DIR/osx-arm64" \
    -output "$FINAL_OUTPUT_DIR/$PROJECT_NAME"

echo "Universal binary created successfully"

cp -a  $FINAL_OUTPUT_DIR/$PROJECT_NAME Mirivoice.Desktop/osxbuild/MiriVoice.app/Contents/MacOS