#!/bin/bash

PROJECT_NAME="MiriVoice"
OUTPUT_DIR="Mirivoice.Desktop/bin"
APP_DIR="Mirivoice.Desktop/osxbuild/MiriVoice.app"

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


mv "$UNIVERSAL_OUTPUT/MiriVoice" "$APP_DIR/Contents/MacOS/"
echo "Binary moved to $APP_DIR"
