properties {
    $base_dir = Resolve-Path ..
    $packages = "$base_dir\packages"
    $build_dir = "$base_dir\build"
    $scripts_dir = "$base_dir\scripts"
    $sln_file = "$base_dir\src\FileBiggy.sln"
    $src_dir = "$base_dir\src"
	$src_packages = "$src_dir\packages"
	
	$nuget_path = "$base_dir\.nuget\NuGet.exe"
	
	$nuspec_path = "$base_dir\FileBiggy.nuspec"
	
    $environment = ""   
	$revision = "2"    
	$version = "0.0." 
	
    $header = "$base_dir\header-agpl.txt"

	$config = "Release"
}

Framework "4.5.1x64"

task default -depends Clean, Compile

task release -depends Clean, Version, Restore-Packages, Compile, Create-Nuget-Package

Task Restore-Packages -Description "Restores all nuget packages" {
    $packageConfigs = Get-ChildItem $src_dir -Recurse | where{$_.Name -eq "packages.config"}
    foreach($packageConfig in $packageConfigs){
        Write-Host "Restoring" $packageConfig.FullName
        & $nuget_path i $packageConfig.FullName -o $src_packages
    }
}

Task Create-Nuget-Package -Description "Creates the nuget package" {
	& $nuget_path pack $nuspec_path -o $base_dir
}

Task Zip -Description "Package the output" {
    . "$scripts_dir\zip.ps1"

    ZipFiles "$build_dir.zip" $build_dir
}

Task Version -Description "Version the assemblies" {
    . "$scripts_dir\versioning.ps1"

	Update-AssemblyInfoFiles $src_dir $version$revision
}

task Clean -Description "Clean the build directory" {
	$dest = "$build_dir.zip"
    if (Test-Path($dest)) { rm $dest }
		
    @($build_dir) | Where-Object { Test-Path $_ } | ForEach-Object {
        Write-Host "Cleaning folder $_..."
        Remove-Item $_ -Recurse -Force -ErrorAction Stop
    }
}
 
task Compile -Description "Build the solution" {
    Write-Host $build_dir
    if (-not ($build_dir.EndsWith("\"))) { 
        $build_dir = $build_dir + '\'
    } 
  
    if ($build_dir.Contains(" ")) { 
        $build_dir = """$($build_dir)\""" #read comment from Johannes Rudolph here: http://www.markhneedham.com/blog/2008/08/14/msbuild-use-outputpath-instead-of-outdir/ 
    } 

    Write-Host "Compiling $sln_file in $BuildConfiguration mode to $build_dir"
    Exec { & msbuild """$sln_file"" /t:Clean /t:Build /p:Configuration=$config /m /nr:false /v:q /nologo /p:OutDir=$build_dir" }
}


