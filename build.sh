#!/usr/bin/env bash
set -e
dotnet restore -p:TargetFramework="$1"
dotnet build -c Release -p:TargetFramework="$1" --no-restore
