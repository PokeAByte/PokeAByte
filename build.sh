#/bin/sh
set -e
cd src

# Build and zip PokeAByte.Web for linux:
dotnet publish PokeAByte.Web -r linux-x64 -o artifacts/github/PokeAByte -c Release
cd artifacts/github/
zip Linux-x64.zip PokeAByte/**
rm -rf PokeAByte
cd ../..

# And for windows:
dotnet publish PokeAByte.Web -r win-x64 -o artifacts/github/PokeAByte -c Release
cd artifacts/github/
zip Windows-x64.zip PokeAByte/**
rm -rf PokeAByte
cd ../..

# macOS (ARM):
dotnet publish PokeAByte.Web -r osx-arm64 -o artifacts/github/PokeAByte -c Release
cd artifacts/github/
zip macOS-arm64.zip PokeAByte/**
rm -rf PokeAByte
cd ../..

# macOS (x86):
dotnet publish PokeAByte.Web -r osx-x64 -o artifacts/github/PokeAByte -c Release
cd artifacts/github/
zip macOS-x64.zip PokeAByte/**
rm -rf PokeAByte
cd ../..


# Build the bizhawk DLLs:
dotnet publish PokeAByte.Protocol.BizHawk -o artifacts/github/ -c Release
dotnet publish PokeAByte.Integrations.BizHawk -o artifacts/github/ -c Release
