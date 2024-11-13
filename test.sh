#!/usr/bin/env bash
set -e
dotnet test -c Release -f "$1" --no-build --no-restore
