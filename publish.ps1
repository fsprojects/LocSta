
. ./helper.ps1

if (-not (Test-Path env:nuget_push)) {
    write-host "Please set the 'nuget_push' env var first."
    exit
}

$nugetServer = "https://api.nuget.org/v3/index.json"
$nugetKey = $Env:nuget_push

write-host "Nuget server is: " $nugetServer
write-host "Nuget key is: " $nugetKey

dotnet test ./src/FsLocalState.sln 
success

$packPath = "./.pack"

dotnet pack ./src/FsLocalState.Core/FsLocalState.fsproj -o $packPath -c Release
success

$packageName = Get-ChildItem "$packPath/*.nupkg" | Sort-Object desc | select-object -first 1

dotnet nuget push "$packageName" -k $nugetKey -s $nugetServer
success
