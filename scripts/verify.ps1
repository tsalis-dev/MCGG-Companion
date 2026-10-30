$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-DotNet {
    param([Parameter(Mandatory = $true)][string[]]$CommandArguments)

    & dotnet @CommandArguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($CommandArguments -join ' ') gagal dengan exit code $LASTEXITCODE."
    }
}

$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $projectRoot
try {
    Invoke-DotNet -CommandArguments @('restore', '.\MCGG-Companion.sln', '--locked-mode')
    Invoke-DotNet -CommandArguments @('build', '.\MCGG-Companion.sln', '--configuration', 'Release', '--no-restore')
    Invoke-DotNet -CommandArguments @('test', '.\MCGG-Companion.sln', '--configuration', 'Release', '--no-build', '--no-restore')
    Invoke-DotNet -CommandArguments @(
        'publish', '.\src\McggCompanion.App\McggCompanion.App.csproj',
        '--configuration', 'Release', '--runtime', 'win-x64', '--self-contained', 'true',
        '--no-restore', '--output', '.\artifacts\publish\win-x64'
    )
}
finally {
    Pop-Location
}
