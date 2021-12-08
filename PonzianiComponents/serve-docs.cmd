@echo off
dotnet-serve -d "../docs" -h "Cache-Control: no-cache" -o -c --mime .png=image/png