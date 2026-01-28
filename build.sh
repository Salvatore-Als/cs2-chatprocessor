#!/bin/bash

CONFIG="Debug"
if [[ "$1" =~ ^[Rr]elease$ ]]; then
    CONFIG="Release"
elif [[ "$1" =~ ^[Dd]ebug$ ]] || [[ -z "$1" ]]; then
    CONFIG="Debug"
else
    echo "Usage: $0 [debug|release]"
    exit 1
fi

echo "Building ChatProcessor in $CONFIG mode..."

BUILD_SUCCESS=0
BUILD_FAIL=0

echo "Deleting build/ folder..."
rm -rf build/

copy_files() {
    SRC_DIR="$1"
    DEST_DIR="$2"
    shift 2
    PATTERNS=("$@")

    mkdir -p "$DEST_DIR"

    if [ -d "$SRC_DIR" ]; then
        for pattern in "${PATTERNS[@]}"; do
            cp "$SRC_DIR"/$pattern "$DEST_DIR/" 2>/dev/null || true
        done
        echo "  - Copied files (${PATTERNS[*]}) from $SRC_DIR to $DEST_DIR"
    else
        echo "  - Warning: source folder $SRC_DIR not found"
    fi
}

echo "Building ChatProcessor.Api..."
if dotnet build src/ChatProcessor.Api/ChatProcessor.Api.csproj -c $CONFIG --no-incremental; then
    copy_files "src/ChatProcessor.Api/bin/$CONFIG/net8.0" "build/shared/ChatProcessorApi" "*.dll" "*.pdb" "*.deps.json"
    ((BUILD_SUCCESS++))
else
    echo "  - Failed to build ChatProcessor.Api"
    ((BUILD_FAIL++))
fi

echo "Building ChatProcessor.Core..."
if dotnet build src/ChatProcessor.Core/ChatProcessor.Core.csproj -c $CONFIG --no-incremental; then
    copy_files "src/ChatProcessor.Core/bin/$CONFIG/net8.0" "build/plugins/ChatProcessorCore" "ChatProcessorCore.dll" "ChatProcessorCore.pdb" "ChatProcessorCore.deps.json"
    
    if [ -d "src/ChatProcessor.Core/lang" ]; then
        cp -r "src/ChatProcessor.Core/lang" "build/plugins/ChatProcessorCore/" 2>/dev/null || true
        echo "  - Copied lang folder into build/plugins/ChatProcessorCore/"
    fi
    
    if [ -f "src/ChatProcessor.Core/config.json" ]; then
        cp "src/ChatProcessor.Core/config.json" "build/plugins/ChatProcessorCore/" 2>/dev/null || true
        echo "  - Copied config.json into build/plugins/ChatProcessorCore/"
    fi
    
    ((BUILD_SUCCESS++))
else
    echo "  - Failed to build ChatProcessor.Core"
    ((BUILD_FAIL++))
fi

echo "Building ChatProcessor.Example..."
if dotnet build src/ChatProcessor.Example/ChatProcessor.Example.csproj -c $CONFIG --no-incremental; then
    copy_files "src/ChatProcessor.Example/bin/$CONFIG/net8.0" "build/plugins/ChatProcessorExample" "ChatProcessorExample.dll" "ChatProcessorExample.pdb" "ChatProcessorExample.deps.json"
    
    if [ -f "src/ChatProcessor.Example/config.json" ]; then
        cp "src/ChatProcessor.Example/config.json" "build/plugins/ChatProcessorExample/" 2>/dev/null || true
        echo "  - Copied config.json into build/plugins/ChatProcessorExample/"
    fi
    
    echo "  - Plugin ChatProcessorExample built in build/plugins/ChatProcessorExample"
    ((BUILD_SUCCESS++))
else
    echo "  - Failed to build ChatProcessor.Example"
    ((BUILD_FAIL++))
fi

echo ""
echo "Build completed! Structure is in build/"
echo "  - Shared library: build/shared/ChatProcessorApi/"
echo "  - Core plugin: build/plugins/ChatProcessorCore/"
echo "  - Example plugin: build/plugins/ChatProcessorExample/"
echo ""
echo "Build Summary:"
echo "  - Success: $BUILD_SUCCESS"
echo "  - Failed: $BUILD_FAIL"

if [ $BUILD_FAIL -gt 0 ]; then
    exit 1
fi
