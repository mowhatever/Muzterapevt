param( $Target )

$root = [System.IO.Path]::GetFullPath( [System.IO.Path]::Combine( (Split-Path $MyInvocation.MyCommand.Path), ".." ) )
cd $root
& "$root\build.ps1" $Target -Bootstrap