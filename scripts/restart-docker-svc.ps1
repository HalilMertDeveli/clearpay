Restart-Service com.docker.service -Force
Get-Service com.docker.service | Format-List Name, Status
