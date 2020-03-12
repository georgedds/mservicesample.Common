#!/bin/bash

cd src/mservicesample.Common
dotnet pack /p:PackageVersion=1.0.$TRAVIS_BUILD_NUMBER --no-restore -o .

case "$GIT_BRANCH" in
  "dev")    
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;    
esac