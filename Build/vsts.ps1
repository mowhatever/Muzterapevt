param( $Target )

$root = [System.IO.Path]::Combine( (Split-Path $MyInvocation.MyCommand.Path), ".." )
cd $root
Write-Host $root
& build.ps1 $Target -Bootstrap