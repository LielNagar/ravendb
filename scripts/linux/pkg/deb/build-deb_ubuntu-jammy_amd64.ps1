$env:OUTPUT_DIR = "$PSScriptRoot/dist"

.\set-ubuntu-jammy.ps1
.\set-raven-platform-amd64.ps1
.\set-raven-version-env.ps1

.\build-deb.ps1
