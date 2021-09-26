$version = $args[0]
$srcPath = "$PSScriptRoot\..\src"
$slnPath = "$srcPath\AutoGame.sln"
$installerProjectPath = "$srcPath\Installer\Installer.vdproj"
$installerReleaseDir = "$srcPath\Installer\Release"

# Make sure version was passed in
if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Error "Version was not specified"
    exit 2
}

# Make sure the working directory is clean
$gitStatus = git status --porcelain
if (![string]::IsNullOrWhiteSpace($gitStatus)) {
    Write-Error "The git working directory is not clean"
    exit 1
}

try {
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

    Write-Host "Build complete"
    Write-Host "$installerReleaseDir\AutoGame_Setup_$version.msi"

    # Reset version changes
    git checkout -- .
}
catch [System.Exception] {
    Write-Error $_.ToString()
}