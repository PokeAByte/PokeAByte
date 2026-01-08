#/bin/bash
set -e
cd src

declare -A targets=(
	["win-x64"]="Windows-x64.zip"
	["linux-x64"]="Linux-x64.zip"
	["osx-arm64"]="macOS-arm64.zip"
	["osx-x64"]="macOS-x64.zip"
)

build () {
	dotnet publish PokeAByte.Web -r $1 -o artifacts/github/PokeAByte -c Release
	pushd artifacts/github
	zip -r ${targets[$1]} PokeAByte/
	rm -rf PokeAByte/
	popd
}

for entry in "${!targets[@]}"; do
    build $entry
done

# Build the bizhawk DLLs:
dotnet publish PokeAByte.Protocol.BizHawk -o artifacts/github/ -c Release
dotnet publish PokeAByte.Integrations.BizHawk -o artifacts/github/ -c Release

# If "--publish" flag is set, use the github cli to create a release:
if [[ $1 == *--publish* ]] then
	cd artifacts/github
	# Grab the release notes from the CHANGELOG.md via awk:
	# Basically it gets the first line starting with "# " until the next line starting with "# ".
	awk '/^# /{if (version) exit; version=1} version {print}' ../../../CHANGELOG.md > release_notes.md
	# Create the github release. Argument 2 is the release tag given in the workflow input.
	# The release is marked as a draft so that a human maintainer can make sure the script worked properly.
	gh release create $2 *.{zip,dll} -t "Version $2" -F release_notes.md -d
fi