#!/bin/bash

# Script to update version in csproj file
# Usage: ./update-version.sh <version>

VERSION=$1

if [ -z "$VERSION" ]; then
  echo "Error: Version parameter is required"
  echo "Usage: ./update-version.sh <version>"
  exit 1
fi

CSPROJ_FILE="src/Clerk.AspNet.Authorization/Clerk.AspNet.Authorization.csproj"

if [ ! -f "$CSPROJ_FILE" ]; then
  echo "Error: File not found: $CSPROJ_FILE"
  exit 1
fi

echo "Updating version to $VERSION in $CSPROJ_FILE..."

# Update Version property
sed -i.bak "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" "$CSPROJ_FILE"

# Update AssemblyVersion property (append .0 for 4-part version)
sed -i.bak "s|<AssemblyVersion>.*</AssemblyVersion>|<AssemblyVersion>$VERSION.0</AssemblyVersion>|g" "$CSPROJ_FILE"

# Update FileVersion property (append .0 for 4-part version)
sed -i.bak "s|<FileVersion>.*</FileVersion>|<FileVersion>$VERSION.0</FileVersion>|g" "$CSPROJ_FILE"

# Remove backup file
rm -f "$CSPROJ_FILE.bak"

echo "Version updated successfully!"