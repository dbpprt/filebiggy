#http://codecampserver.codeplex.com/SourceControl/changeset/view/4755c1386bff#deployment/Tools/export-db.ps1

#function out-zip($path, $files)
#{
#    if (-not $path.EndsWith('.zip')) {$path += '.zip'} 
#
#    if (Test-Path $path) {
#        rm $path
#    }
#
#    if (-not (test-path $path)) { 
#        set-content $path ("PK" + [char]5 + [char]6 + ("$([char]0)" * 18)) 
#    } 
#
#    $zip=resolve-path($path)
#    $ZipFile = (new-object -com shell.application).NameSpace( "$zip" ) 
#    $files | foreach {$ZipFile.CopyHere($_.fullname)} 
#}

function ZipFiles( $zipfilename, $sourcedir )
{
   Add-Type -Assembly System.IO.Compression.FileSystem
   $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir,
        $zipfilename, $compressionLevel, $false)
}