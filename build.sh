#!/bin/sh

dotnet clean --nologo -v q
dotnet build "$@" --nologo -c 'Release' RenamePawns.sln
