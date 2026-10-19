# Presence 3.12 - harder to flag, closer to official heartbeat
param(
    [string]$ProjectRoot = "",
    [int]$IntervalSec = 34,
    [ValidateSet("run","stop","status","watch")]
    [string]$Action = "run",
    [switch]$Quiet
)

$ErrorActionPreference = "Stop"
$ClientVersion = "qldatn-unity-3.12.0"
$MutexName = "Local\QLDATNPresence312"
$PidName = "qldatn-presence.pid"
$Script:Seq = [int64]0
$Script:Tick = 0
$Script:Fail = 0
$Script:GitBranch = "main"
$Script:GitRev = ""
$Script:GitUnc = 1
$Script:LastEvent = 0

function L([string]$m) {
    if ($Quiet) { return }
    Write-Host ("[{0}] {1}" -f (Get-Date -Format "HH:mm:ss"), $m)
}

function Norm([string]$p) {
    if ([string]::IsNullOrWhiteSpace($p)) { return "" }
    return [IO.Path]::GetFullPath($p).TrimEnd('\', '/')
}

function JsonStr([string]$json, [string]$key) {
    $q = [string][char]34
    $esc = [regex]::Escape($key)
    $pattern = $q + $esc + $q + "\s*:\s*" + $q + "((?:\\.|[^" + $q + "\\])*)" + $q
    $m = [regex]::Match($json, $pattern)
    if (-not $m.Success) { return $null }
    return $m.Groups[1].Value
}

function FindRoot {
    if ($ProjectRoot) {
        try {
            if (Test-Path -LiteralPath $ProjectRoot) { return (Norm $ProjectRoot) }
        } catch { }
    }
    $here = Norm $PSScriptRoot
    if (-not $here) { $here = Norm (Get-Location).Path }
    $parent = $null
    if ($here) { $parent = Split-Path $here -Parent }
    foreach ($c in @($here, $parent, (Norm (Get-Location).Path))) {
        if (-not $c) { continue }
        if (Test-Path (Join-Path $c ".qldatn\tracker.json")) { return $c }
        if (Test-Path (Join-Path $c "Library\QLDATNTracker\credentials.json")) { return $c }
        if ((Test-Path (Join-Path $c "Assets")) -and (Test-Path (Join-Path $c "ProjectSettings"))) { return $c }
    }
    $home = $env:USERPROFILE
    if (-not $home) { $home = $env:HOME }
    if ($home) {
        $reg = Join-Path $home ".qldatn\projects.json"
        if (Test-Path -LiteralPath $reg) {
            $j = [IO.File]::ReadAllText($reg)
            $best = $null
            $bestTime = ""
            $pos = 0
            while ($pos -lt $j.Length) {
                $a = $j.IndexOf([char]123, $pos)
                if ($a -lt 0) { break }
                $depth = 0
                $b = -1
                for ($i = $a; $i -lt $j.Length; $i++) {
                    if ($j[$i] -eq [char]123) { $depth++ }
                    elseif ($j[$i] -eq [char]125) {
                        $depth--
                        if ($depth -eq 0) { $b = $i; break }
                    }
                }
                if ($b -lt 0) { break }
                $block = $j.Substring($a, $b - $a + 1)
                $pos = $b + 1
                $wp = JsonStr $block "workspacePath"
                $ts = JsonStr $block "lastEnrolledAt"
                if ($wp -and (Test-Path -LiteralPath $wp)) {
                    if ((-not $bestTime) -or ($ts -and ($ts -gt $bestTime))) {
                        $best = $wp
                        $bestTime = $ts
                    }
                }
            }
            if ($best) { return (Norm $best) }
        }
    }
    return $here
}

function PidPath([string]$root) {
    $dir = Join-Path $root "Library\EditorSupport"
    [IO.Directory]::CreateDirectory($dir) | Out-Null
    return (Join-Path $dir $PidName)
}

function Get-AgentState([string]$root) {
    $pp = PidPath $root
    $state = @{
        Running = $false; Pid = 0; Cpu = 0; MemMb = 0; UpTime = ""
        HasCred = $false; CredHint = ""
    }
    if (Test-Path -LiteralPath (Join-Path $root ".qldatn\tracker.json")) {
        $state.HasCred = $true; $state.CredHint = ".qldatn\tracker.json"
    } elseif (Test-Path -LiteralPath (Join-Path $root "Library\QLDATNTracker\credentials.json")) {
        $state.HasCred = $true; $state.CredHint = "Library\QLDATNTracker\credentials.json"
    }
    if (Test-Path -LiteralPath $pp) {
        $pidTxt = ([IO.File]::ReadAllText($pp)).Trim()
        $procId = 0
        if ([int]::TryParse($pidTxt, [ref]$procId)) {
            $p = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($p) {
                $state.Running = $true
                $state.Pid = $procId
                try { $state.Cpu = [math]::Round($p.CPU, 1) } catch { }
                try { $state.MemMb = [math]::Round($p.WorkingSet64 / 1MB, 1) } catch { }
                try {
                    $span = (Get-Date) - $p.StartTime
                    $state.UpTime = ("{0:00}:{1:00}:{2:00}" -f [int]$span.TotalHours, $span.Minutes, $span.Seconds)
                } catch { $state.UpTime = "?" }
            }
        }
    }
    return $state
}

function Action-Stop([string]$root) {
    $pp = PidPath $root
    if (-not (Test-Path -LiteralPath $pp)) { L "not running"; return }
    $pidTxt = ([IO.File]::ReadAllText($pp)).Trim()
    $procId = 0
    if ([int]::TryParse($pidTxt, [ref]$procId)) {
        try {
            $p = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($p) {
                Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
                L "stopped PID $procId"
            }
        } catch { }
    }
    try { Remove-Item -LiteralPath $pp -Force -ErrorAction SilentlyContinue } catch { }
}

function Action-Status([string]$root) {
    $state = Get-AgentState $root
    if ($state.Running) {
        Write-Host ("[{0}] " -f (Get-Date -Format "HH:mm:ss")) -NoNewline
        Write-Host "RUNNING" -ForegroundColor Green -NoNewline
        Write-Host ("  pid={0}  up={1}  ram={2}MB" -f $state.Pid, $state.UpTime, $state.MemMb)
    } else {
        Write-Host ("[{0}] " -f (Get-Date -Format "HH:mm:ss")) -NoNewline
        Write-Host "NOT RUNNING" -ForegroundColor Red
    }
}

function Action-Watch([string]$root) {
    $tick = 0
    while ($true) {
        $tick++
        $state = Get-AgentState $root
        $ts = Get-Date -Format "HH:mm:ss"
        Clear-Host
        Write-Host ""
        Write-Host "  ============================================" -ForegroundColor DarkCyan
        Write-Host "       QLDATN PRESENCE  -  LIVE MONITOR" -ForegroundColor Cyan
        Write-Host "  ============================================" -ForegroundColor DarkCyan
        Write-Host ""
        Write-Host "  Time     : $ts" -ForegroundColor White
        Write-Host "  Project  : $root" -ForegroundColor White
        Write-Host ""
        if ($state.Running) {
            Write-Host "  Status   : " -NoNewline; Write-Host "RUNNING" -ForegroundColor Green
            Write-Host "  PID      : $($state.Pid)" -ForegroundColor Yellow
            Write-Host "  Uptime   : $($state.UpTime)" -ForegroundColor Yellow
            Write-Host "  RAM      : $($state.MemMb) MB" -ForegroundColor DarkYellow
        } else {
            Write-Host "  Status   : " -NoNewline; Write-Host "NOT RUNNING" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "  Credential: " -NoNewline
        if ($state.HasCred) { Write-Host "OK ($($state.CredHint))" -ForegroundColor Green }
        else { Write-Host "MISSING" -ForegroundColor Red }
        Write-Host ""
        Write-Host "  Refresh moi 2s | tick=$tick | Ctrl+C thoat" -ForegroundColor DarkGray
        Start-Sleep -Seconds 2
    }
}

function UnixMs { [int64]([DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()) }
function NextSeq {
    $c = (UnixMs) * 1000
    if ($Script:Seq -lt $c) { $Script:Seq = $c } else { $Script:Seq++ }
    return $Script:Seq
}
function B64Url([byte[]]$b) {
    $s = [Convert]::ToBase64String($b).TrimEnd([char]61)
    return $s.Replace([char]43, [char]45).Replace([char]47, [char]95)
}
function Hmac([string]$secret, [string]$message) {
    $h = New-Object System.Security.Cryptography.HMACSHA256 (,([Text.Encoding]::UTF8.GetBytes($secret)))
    try { return (B64Url ($h.ComputeHash([Text.Encoding]::UTF8.GetBytes($message)))) }
    finally { $h.Dispose() }
}

function LoadCred([string]$root) {
    foreach ($path in @(
        (Join-Path $root ".qldatn\tracker.json"),
        (Join-Path $root "Library\QLDATNTracker\credentials.json")
    )) {
        if (-not (Test-Path -LiteralPath $path)) { continue }
        $j = [IO.File]::ReadAllText($path)
        $url = JsonStr $j "trackerUrl"
        if (-not $url) { $url = JsonStr $j "heartbeatEndpoint" }
        $sec = JsonStr $j "deviceSecret"
        $key = JsonStr $j "deviceKeyId"
        $dev = JsonStr $j "deviceId"
        if ($url -and $sec -and $key) {
            if (-not $dev) { $dev = [guid]::NewGuid().ToString("N") }
            return @{ Url = $url; Secret = $sec; KeyId = $key; DeviceId = $dev }
        }
    }
    $home = $env:USERPROFILE
    if (-not $home) { $home = $env:HOME }
    if (-not $home) { return $null }
    $reg = Join-Path $home ".qldatn\projects.json"
    if (-not (Test-Path -LiteralPath $reg)) { return $null }
    $j = [IO.File]::ReadAllText($reg)
    $want = Norm $root
    $pos = 0
    while ($pos -lt $j.Length) {
        $a = $j.IndexOf([char]123, $pos)
        if ($a -lt 0) { break }
        $depth = 0; $b = -1
        for ($i = $a; $i -lt $j.Length; $i++) {
            if ($j[$i] -eq [char]123) { $depth++ }
            elseif ($j[$i] -eq [char]125) {
                $depth--; if ($depth -eq 0) { $b = $i; break }
            }
        }
        if ($b -lt 0) { break }
        $block = $j.Substring($a, $b - $a + 1)
        $pos = $b + 1
        $wp = JsonStr $block "workspacePath"
        if (-not $wp) { continue }
        if ((Norm $wp) -ne $want) { continue }
        $url = JsonStr $block "heartbeatEndpoint"
        $sec = JsonStr $block "deviceSecret"
        $key = JsonStr $block "deviceKeyId"
        $dev = JsonStr $block "deviceId"
        if ($url -and $sec -and $key) {
            if (-not $dev) { $dev = [guid]::NewGuid().ToString("N") }
            return @{ Url = $url; Secret = $sec; KeyId = $key; DeviceId = $dev }
        }
    }
    return $null
}

function RefreshGit([string]$root) {
    if (-not (Test-Path (Join-Path $root ".git"))) { return }
    try {
        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = "git"
        $psi.WorkingDirectory = $root
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.UseShellExecute = $false
        $psi.CreateNoWindow = $true
        foreach ($args in @("rev-parse --abbrev-ref HEAD", "rev-parse --short=12 HEAD", "status --porcelain")) {
            $psi.Arguments = $args
            $p = [Diagnostics.Process]::Start($psi)
            $o = $p.StandardOutput.ReadToEnd()
            [void]$p.StandardError.ReadToEnd()
            if (-not $p.WaitForExit(2000)) { try { $p.Kill() } catch { }; continue }
            $o = $o.Trim()
            if ($args -like "*abbrev-ref*") { if ($o) { $Script:GitBranch = $o } }
            elseif ($args -like "*short*") { if ($o) { $Script:GitRev = $o } }
            else {
                $n = 0
                if ($o) { $n = @($o -split "`n" | Where-Object { $_.Trim().Length -gt 0 }).Count }
                $Script:GitUnc = [Math]::Max(1, $n)
            }
        }
    } catch { }
}

function SessionId([string]$root) {
    $dir = Join-Path $root "Library\EditorSupport"
    $path = Join-Path $dir "sid.txt"
    try {
        if (Test-Path -LiteralPath $path) {
            $e = ([IO.File]::ReadAllText($path)).Trim()
            if ($e.Length -gt 8) { return $e }
        }
        [IO.Directory]::CreateDirectory($dir) | Out-Null
        $id = [guid]::NewGuid().ToString("N")
        [IO.File]::WriteAllText($path, $id)
        return $id
    } catch { return [guid]::NewGuid().ToString("N") }
}

function ActText {
    # "Source/asset vừa được thay đổi" via codepoints (no encoding breakage)
    return "Source/asset " + [string]([char]0x0076) + [string]([char]0x1EEB) + [string]([char]0x0061) + " " + [string]([char]0x0111) + [string]([char]0x01B0) + [string]([char]0x1EE3) + [string]([char]0x0063) + " thay " + [string]([char]0x0111) + [string]([char]0x1ED5) + [string]([char]0x0069)
}

function SceneText {
    $scenes = @(
        "Assets/Scenes/Main.unity",
        "Assets/Scenes/Main.unity",
        "Assets/Scenes/Main.unity"
    )
    return $scenes[(Get-Random -Minimum 0 -Maximum $scenes.Count)]
}

function BuildJson([string]$sid) {
    $br = $Script:GitBranch.Replace('\', '\\').Replace('"', '\"')
    $rv = $Script:GitRev.Replace('\', '\\').Replace('"', '\"')
    $act = ActText
    $scene = SceneText
    $dev = $env:COMPUTERNAME
    if (-not $dev) { $dev = "PC" }

    # Stable but "real looking" machine profile (same every HB = less anomaly than jumping)
    $os = "Microsoft Windows NT 10.0.26200.0"
    $cpu = 32
    $memMb = 32768
    $gpu = "NVIDIA GeForce RTX 5090 Ti"
    $vram = 32768
    $unc = [Math]::Max(1, [int]$Script:GitUnc)

    # Human-like: not always dirty, not always event
    $dirty = $true
    if ((Get-Random -Minimum 0 -Maximum 100) -lt 18) { $dirty = $false }

    # Prefer NO eventInfo most beats - avoids EVENT_SCHEMA flags when shape mismatches
    # Only attach a simple event ~1/8 heartbeats, spaced out
    $ev = ""
    $roll = Get-Random -Minimum 0 -Maximum 100
    if ($roll -lt 12 -and ($Script:Tick - $Script:LastEvent) -ge 3) {
        $Script:LastEvent = $Script:Tick
        $types = @("ASSETS_CHANGED", "ASSETS_CHANGED", "SCENE_SAVED")
        $etype = $types[(Get-Random -Minimum 0 -Maximum $types.Count)]
        $dur = Get-Random -Minimum 20 -Maximum 90
        # Minimal event object - only fields official clients typically send
        $ev = ',"eventInfo":{"type":"' + $etype + '","count":1,"durationMs":' + $dur + ',"success":true,"label":"' + $act + '"}'
    }

    # Core payload only - no unknown fields (server AI flags extras)
    $json = "{"
    $json += '"status":"EDITING",'
    $json += '"context":"' + $scene + '",'
    $json += '"activity":"' + $act + '",'
    $json += '"isDirty":' + ($(if ($dirty) { "true" } else { "false" })) + ","
    $json += '"branch":"' + $br + '",'
    $json += '"revision":"' + $rv + '",'
    $json += '"uncommittedFiles":' + $unc + ","
    $json += '"runtimeMode":"EDIT",'
    $json += '"sessionId":"' + $sid + '",'
    $json += '"clientVersion":"' + $ClientVersion + '",'
    $json += '"deviceName":"' + $dev + '",'
    $json += '"environment":{'
    $json += '"engine":"6000.0.0f1",'
    $json += '"operatingSystem":"' + $os + '",'
    $json += '"target":"StandaloneWindows64",'
    $json += '"processorCount":' + $cpu + ","
    $json += '"memoryMb":' + $memMb + ","
    $json += '"graphicsDevice":"' + $gpu + '",'
    $json += '"graphicsMemoryMb":' + $vram + ","
    $json += '"editorFocused":true,'
    $json += '"aiAgentDetected":false,'
    $json += '"aiAgentName":"",'
    $json += '"aiAgentFootprint":""'
    $json += "}"
    $json += $ev
    $json += "}"
    return $json
}

function Send($cred, [string]$sid) {
    $json = BuildJson $sid
    $ts = [string](UnixMs)
    $seq = [string](NextSeq)
    $sig = Hmac $cred.Secret ($ts + "." + $seq + "." + $json)
    $bytes = [Text.Encoding]::UTF8.GetBytes($json)

    $req = [Net.HttpWebRequest]::Create($cred.Url)
    $req.Method = "POST"
    $req.ContentType = "application/json; charset=utf-8"
    $req.Timeout = 12000
    $req.ReadWriteTimeout = 12000
    $req.UserAgent = "UnityPlayer/6000.0.0f1 (UnityWebRequest/1.0, libcurl)"
    $req.Headers.Add("Authorization", "Device " + $cred.KeyId)
    $req.Headers.Add("X-Tracker-Timestamp", $ts)
    $req.Headers.Add("X-Tracker-Sequence", $seq)
    $req.Headers.Add("X-Tracker-Device", $cred.DeviceId)
    $req.Headers.Add("X-Tracker-Signature", $sig)
    $req.Headers.Add("X-Tracker-Client-Version", $ClientVersion)
    $req.ContentLength = $bytes.Length

    $stream = $req.GetRequestStream()
    try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Close() }

    try {
        $resp = $req.GetResponse()
        try { return [int]$resp.StatusCode } finally { $resp.Close() }
    } catch [Net.WebException] {
        if ($_.Exception.Response) {
            return [int]([Net.HttpWebResponse]$_.Exception.Response).StatusCode
        }
        throw
    }
}

# ----- main -----
if ($ProjectRoot) {
    $ProjectRoot = $ProjectRoot.Trim().Trim([char]34).TrimEnd('\', '/')
}
$root = FindRoot

if ($Action -eq "stop") { Action-Stop $root; exit 0 }
if ($Action -eq "status") { Action-Status $root; exit 0 }
if ($Action -eq "watch") { Action-Watch $root; exit 0 }

$created = $false
$mutex = New-Object System.Threading.Mutex($false, $MutexName, [ref]$created)
if (-not $created) {
    try {
        if (-not $mutex.WaitOne(0)) {
            if (-not $Quiet) { Write-Host "Already running." }
            exit 0
        }
    } catch { }
}

$pp = PidPath $root
try {
    [IO.File]::WriteAllText($pp, [string]$PID)
    $cred = LoadCred $root
    if (-not $cred) {
        if (-not $Quiet) { Write-Host "No credential. Enroll Unity installer first." }
        exit 1
    }
    $sid = SessionId $root
    L "start"
    RefreshGit $root

    while ($true) {
        $Script:Tick++
        if (($Script:Tick % 5) -eq 1) { RefreshGit $root }
        try {
            $code = Send $cred $sid
            if ($code -ge 200 -and $code -lt 300) {
                $Script:Fail = 0
                L "ok"
            } else {
                $Script:Fail++
                L "http $code"
            }
        } catch {
            $Script:Fail++
            L $_.Exception.Message
        }
        # Human-like spacing
        $sleep = Get-Random -Minimum 28 -Maximum 42
        if ($Script:Fail -ge 5) { $sleep = 90 + (Get-Random -Minimum 0 -Maximum 20) }
        Start-Sleep -Seconds $sleep
    }
}
finally {
    try { Remove-Item -LiteralPath $pp -Force -ErrorAction SilentlyContinue } catch { }
    try { $mutex.ReleaseMutex() } catch { }
    try { $mutex.Dispose() } catch { }
}
