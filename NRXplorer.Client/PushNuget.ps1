rm "bin\release\" -Recurse -Force
dotnet pack --configuration Release --include-symbols -p:SymbolPackageFormat=snupkg
$package=(ls .\bin\Release\*.nupkg).FullName
dotnet nuget push $package --source "https://api.nuget.org/v3/index.json"
$ver = ((ls .\bin\release\*.nupkg)[0].Name -replace 'NRXplorer\.Client\.(\d+(\.\d+){1,3}).*', '$1')
git tag -a "Client/v$ver" -m "Client/$ver"
git push origin "Client/v$ver"
