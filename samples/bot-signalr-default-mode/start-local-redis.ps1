# start-local-redis.ps1
# Starts a local Redis server used by APX's Default-mode socket connection directory
# (RedisConnectionDirectory: HSET botconn:{botKey} field=connectionId, with a TTL; the invoker pod
# HGETALLs and picks the most-recently-seen connection to target with client results). Default mode
# has NO reply backplane — Redis only holds the botKey -> connectionId directory.
#
# Idempotent: if redis-server is already running it just PINGs it; on first run it downloads a portable
# native Windows Redis (no admin / no Docker / no WSL needed) and starts it detached.
#
# Usage:   .\start-local-redis.ps1            # default localhost:6379
#          .\start-local-redis.ps1 -Port 6380
# Stop:    Get-Process redis-server | Stop-Process

[CmdletBinding()]
param(
  [int]$Port = 6379,
  [string]$InstallDir = "C:\src\local-redis"
)

$ErrorActionPreference = "Stop"
$server = Join-Path $InstallDir "redis-server.exe"
$cli    = Join-Path $InstallDir "redis-cli.exe"

# Already running? Just verify and exit.
$existing = Get-Process redis-server -ErrorAction SilentlyContinue
if ($existing) {
  Write-Host "redis-server already running (PID $($existing.Id -join ',')). PING ->" -ForegroundColor Yellow -NoNewline
  Write-Host (" " + (& $cli -p $Port ping))
  return
}

# First-run install of the portable Windows Redis build.
if (-not (Test-Path $server)) {
  Write-Host "Redis not found at $InstallDir - downloading portable Windows build..." -ForegroundColor Cyan
  New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
  $zip = Join-Path $InstallDir "redis.zip"
  $url = "https://github.com/tporadowski/redis/releases/download/v5.0.14.1/Redis-x64-5.0.14.1.zip"
  [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
  Invoke-WebRequest -Uri $url -OutFile $zip -UseBasicParsing
  Expand-Archive -Path $zip -DestinationPath $InstallDir -Force
  Remove-Item $zip -Force
}

# Start detached so it survives this shell.
Start-Process -FilePath $server -ArgumentList "--port", $Port -WindowStyle Hidden
Write-Host "Started redis-server on port $Port." -ForegroundColor Green

# Verify PONG (server binds in well under a second).
$pong = ""
foreach ($i in 1..12) {
  $pong = (& $cli -p $Port ping) 2>&1
  if ($pong -match 'PONG') { break }
  Start-Sleep -Milliseconds 300
}
if ($pong -match 'PONG') {
  Write-Host "Redis is up on localhost:$Port (PING -> PONG)." -ForegroundColor Green
  Write-Host "Point APX at it with APX_LOCAL_REDIS_CONNECTIONSTRING=localhost:$Port (SignalRConfiguration.LocalRedisConnectionString)."
  Write-Host "Stop later with: Get-Process redis-server | Stop-Process"
} else {
  Write-Warning "Redis did not answer PING on port $Port - check $InstallDir."
}
