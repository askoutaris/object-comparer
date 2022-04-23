@echo off

setlocal EnableDelayedExpansion

dotnet pack -c Release --include-symbols

:end
