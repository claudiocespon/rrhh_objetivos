Add-Type -AssemblyName System.IO.Compression.FileSystem
$source = 'c:\Development\Antigravity\RRHH_Objetivos\Seguimiento\Roles - Plataforma de objetivos.docx'
$temp = Join-Path $env:TEMP 'Roles_Copy.docx'
Copy-Item $source $temp -Force
$zip = [System.IO.Compression.ZipFile]::OpenRead($temp)
$doc = $zip.GetEntry('word/document.xml')
$stream = $doc.Open()
$reader = New-Object System.IO.StreamReader($stream)
$xml = $reader.ReadToEnd()
$reader.Close()
$zip.Dispose()
Remove-Item $temp
$text = $xml -replace '<[^>]+>', ' ' -replace '\s+', ' '
$text | Out-File "c:\Development\Antigravity\RRHH_Objetivos\Seguimiento\Roles.txt" -Encoding utf8
