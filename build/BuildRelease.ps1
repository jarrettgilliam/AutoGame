try
{
    $version = $args[0]
    $github_token = $args[1]
    $srcPath = "$PSScriptRoot\..\src"
    $slnPath = "$srcPath\AutoGame.sln"
    $installerProjectPath = "$srcPath\Installer\Installer.vdproj"
    $installerReleaseDir = "$srcPath\Installer\Release"
    $startPath = $PWD

    Set-Location -Path $srcPath

    # Make sure version was passed in
    if ([string]::IsNullOrWhiteSpace($version))
    {
        Write-Error "Version was not specified"
        exit 1
    }
    
    $version = $version.TrimStart('v');

    # Make sure the working directory is clean
    $gitStatus = git status --porcelain

    if (!$?)
    {
        Write-Error "git status failed"
        exit 2
    }

    if (![string]::IsNullOrWhiteSpace($gitStatus))
    {
        Write-Error "The git working directory is not clean"
        exit 3
    }

    dotnet test $slnPath

    if (!$?)
    {
        Write-Error "One or more tests failed"
        exit 4
    }

    # Set version on assemblies
    Get-ChildItem -Path $srcPath -Filter *.csproj -Recurse -File |
            Foreach-Object {
                $path = $_.FullName
                (Get-Content $path) | ForEach-Object {
                    $_ -replace '(?<=<Version>)\d(\.\d)+(?=</Version>)', "$version"
                } | Set-Content $path
            }

    # Set version and generate GUIDs on the installer project
    $productGuid = [GUID]::NewGuid().ToString().ToUpper()
    $packageGuid = [GUID]::NewGuid().ToString().ToUpper()

    (Get-Content $installerProjectPath) | ForEach-Object {
        $line = $_
        $line = $line -replace '(?<="ProductVersion" = "8:)\d(\.\d)+(?=")', $version
        $line = $line -replace '(?<="ProductCode" = "8:\{)[^}]+(?=\}")', $productGuid
        $line = $line -replace '(?<="PackageCode" = "8:\{)[^}]+(?=\}")', $packageGuid
        $line
    } | Set-Content $installerProjectPath

    # Build the solution
    devenv "$slnPath" /rebuild Release /project Installer

    # Rename the output *.msi file
    Rename-Item "$installerReleaseDir\AutoGame_Setup.msi" "AutoGame_Setup_$version.msi"
    
    # Create a GitHub release
    if (![string]::IsNullOrWhiteSpace($github_token))
    {
        $env:GITHUB_TOKEN = $github_token;
        hub release create --draft --message "AutoGame v${version}" --attach "$installerReleaseDir\AutoGame_Setup_$version.msi" "v${version}"
        Remove-Item Env:\GITHUB_TOKEN
    }

    Write-Host "Build complete"
    Write-Host "$installerReleaseDir\AutoGame_Setup_$version.msi"

    # Reset version changes
    git reset --hard HEAD
}
catch [System.Exception]
{
    Write-Error $_.ToString()
}
finally
{
    Set-Location -Path $startPath
}