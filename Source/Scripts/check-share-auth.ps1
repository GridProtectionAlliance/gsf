# Checks in with a GSF Time Series Windows Service to see if any
# errors occurred when authenticating to the file share at startup.
#
# Reads the StatusLog.txt file once per minute to identify whether
# an error has occurred. After 15 minutes, the script stops checking.
# If the last relevant message indicates that authentication errors
# are still occurring, this script sends an email notification to
# admin@gridprotectionalliance.org.

$startTime = Get-Date
$serverName = $env:ComputerName
$serviceName = "GSFTimeSeriesService"
$installPath = "C:\Program Files\$serviceName"
$statusLogPath = Join-Path $installPath "StatusLog.txt"

enum MessageType {
    Startup
    Success
    Error
    Other
}

function Get-MessageType {
    param ([string]$line)
    if ($line -match "Node \{[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}\} Initializing") {
        return [MessageType]::Startup
    }
    if ($line -match "\[HealthExporter\] Network share authentication to \\\\[^\\]+\\[^\\]+ succeeded.") {
        return [MessageType]::Success
    }
    if ($line -match "\[HealthExporter\] Export attempt aborted") {
        return [MessageType]::Error
    }
    return [MessageType]::Other
}

while ($true) {
    $elapsedTime = $(Get-Date) - $startTime

    $lastMessage = Get-Content $statusLogPath |
        ForEach-Object { Get-MessageType $_ } |
        Where-Object { $_ -ne [MessageType]::Other } |
        Select-Object -Last 1

    if ($lastMessage -eq [MessageType]::Error) {
        $consolePath = Join-Path $installPath "${serviceName}Console.exe"
        Write-Output "Authenticate" | & $consolePath | Out-Null
    }

    if ($elapsedTime.TotalMinutes -ge 15) {
        if ($lastMessage -eq [MessageType]::Error) {
            $subject = "[$serverName] $serviceName authentication failure"
            $body = "$serviceName service failed to authenticate to the network share"

            Send-MailMessage `
                -SmtpServer 127.0.0.1 `
                -To admin@gridprotectionalliance.org `
                -From tsservice@gridprotectionalliance.org `
                -Subject $subject `
                -Body $body `
                -ErrorAction Stop
        }

        break
    }

    Start-Sleep 60
}