#!/usr/bin/env bash
set -e
dotnet test -c Release -p:TargetFramework="$1"
