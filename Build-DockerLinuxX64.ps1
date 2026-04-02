$ErrorActionPreference = "Stop"

$Version = "2.0.2-lukas-experimental"

# 1. Build the frontend
npm install --prefix ./LANCommander.UI
npm run package --prefix ./LANCommander.UI
npm install --prefix ./LANCommander.Server
npm run package --prefix ./LANCommander.Server

# 2. Publish the auto-updater and server for linux-x64
dotnet publish ./LANCommander.AutoUpdater/LANCommander.AutoUpdater.csproj `
    -c Release --self-contained --runtime linux-x64 `
    /p:Version=$Version

dotnet publish ./LANCommander.Server/LANCommander.Server.csproj `
    -c Release --self-contained --runtime linux-x64 `
    /p:Version=$Version

# 3. Copy the auto-updater into the server publish output
Copy-Item -Force -Recurse `
    LANCommander.AutoUpdater/bin/Release/net9.0/linux-x64/publish/* `
    LANCommander.Server/bin/Release/net9.0/linux-x64/publish/

# 4. Stage for Docker
#    Placed at the repo root (outside LANCommander.Server/) so the Blazor
#    compiler never scans the pre-built .razor.js files and raises BLAZOR106.
$stagingDir = "docker-staging"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $stagingDir
New-Item -ItemType Directory -Path "$stagingDir/amd64" | Out-Null
Copy-Item -Force -Recurse `
    LANCommander.Server/bin/Release/net9.0/linux-x64/publish/* `
    "$stagingDir/amd64/"

# Copy shell scripts into the staging context so the Dockerfile can reach them
Copy-Item -Force LANCommander.Server/entrypoint.sh "$stagingDir/entrypoint.sh"
Copy-Item -Force LANCommander.Server/steamcmd.sh   "$stagingDir/steamcmd.sh"

# 5. Build the Docker image (context = docker-staging/)
docker build `
    --platform linux/amd64 `
    -t lancommander/lancommander:$Version `
    -f LANCommander.Server/Dockerfile `
    ./docker-staging

docker save --output "lancommander-$Version.tar" lancommander/lancommander:$Version
