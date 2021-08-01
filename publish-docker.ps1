$ver = [regex]::Match((Get-Content NRXplorer\NRXplorer.csproj), '<Version>([^<]+)<').Groups[1].Value
git tag -a "v$ver" -m "$ver"
git push origin "v$ver"