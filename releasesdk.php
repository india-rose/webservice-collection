<?php

	// template Variable must have id, author & version fields 
	function UpdateTemplate($templateFile, $outputFile, $templateVariables)
	{
		$content = file_get_contents($templateFile);
		
		// upgrade template variables
		foreach($templateVariables as $key => $value)
		{
			$content = str_replace('{'. $key . '}', $value, $content);
		}
		
		file_put_contents($outputFile, $content);
	}

	function checkDirectoryOrCreate($directory)
	{
		if(!file_exists($directory))
		{
			if(!mkdir(trim($directory, '/'), 0777, true))
			{
				log_error('Cannot create directory ' . $directory);
			}
		}
	}

	function print_log($message)
	{
		echo $message . PHP_EOL;
	}
	
	function log_error($message)
	{
		echo 'ERROR : ' . $message . PHP_EOL;
		die();
	}

	function exec_system($command)
	{
		echo $command . PHP_EOL;
		
		$result = 0;
		$output = array();
		
		//exec($command, $output, $result);
		$output = system($command, $result);
		
		if($result !== 0)
		{
			echo $output;
			log_error('execution of command ' . $command . ' failed');
		}
	}
	
	function exec_copy($source, $dest)
	{
		if(!copy($source, $dest))
		{
			log_error('Cannot copy file ' . $source . ' to ' . $dest);
		}
	}
	
	function move_chdir($path)
	{
		echo "cd " . $path . PHP_EOL;
		chdir($path);
	}

	global $argc, $argv;
	
	$projectName = "IndiaRose.WebAPI.Sdk";
	$buildConfiguration = "Release";
	$buildPlatform = "AnyCPU";
	$nugetBuildDirectory = "nuget_build";
	$nugetTemplateDirectory = "nuget_template";
	$nugetPackageDirectory = "nuget_packages";
	$releaseBranchName = "sdk_v"; // will be completed with version number
	$dependencies = "WebAPI.Common";
	$libProfile = "portable-net45+win+wpa81+wp80+MonoAndroid10+xamarinios10+MonoTouch10";
	
	
	
	if($argc < 2)
	{
		echo "Usage : releasesdk.sh <version>";
		die;
	}
	
	$versionNumber = $argv[$argc - 1];
	
	$path = getenv("PATH");
	$vspath = getenv("VSPATH");
	if(!putenv("PATH=" . $path . ';' . $vspath))
	{
		die('Cannot set path');
	}
	
	//exec_system('export PATH="$VSPATH;$PATH"');
	
	print_log("# Ok let's go !");
	
	// get develop branch
	print_log("# Checking out develop branch");
	exec_system('git checkout develop');
	
	// create release branch
	print_log("# Creating release branch from develop");
	$branchName = 'release/' . $releaseBranchName . $versionNumber;
	exec_system('git checkout -b ' . $branchName . ' develop');
	
	// build project with msbuild to check for error
	print_log("# Building your project " . $projectName);
	exec_system('msbuild /t:Clean;Build /p:Configuration=' . $buildConfiguration . ';Platform=' . $buildPlatform . ' ' . $projectName . '/' . $projectName . '.csproj');
		
	// create directories
	checkDirectoryOrCreate($nugetBuildDirectory);
	checkDirectoryOrCreate($nugetPackageDirectory);

		
	print_log("# Updating nuspec file for " . $projectName);	
	$buildDirectory = $nugetBuildDirectory . '/' . $projectName . '_v' . $versionNumber . '/';
	$libDirectory = $buildDirectory . 'lib/' . $libProfile . '/';
	$nuspecFile = $buildDirectory . $projectName . '.nuspec';
	$templateFile = $nugetTemplateDirectory . '/' . $projectName . '.nuspec';
		
	checkDirectoryOrCreate($buildDirectory);
	checkDirectoryOrCreate($libDirectory);
	exec_copy($projectName . '/bin/' . $buildConfiguration . '/' . $projectName . '.dll', $libDirectory . $projectName . '.dll');
	
	foreach(explode(';', $dependencies) as $dependency)
	{
		exec_copy($projectName . '/bin/' . $buildConfiguration . '/' . $dependencies . '.dll', $libDirectory . $dependencies . '.dll');
	}
	
		
	UpdateTemplate($templateFile, $nuspecFile, array('id' => $projectName, 'author' => 'India Rose Autism', 'version' => $versionNumber));
			
	move_chdir($buildDirectory);
		
	print_log("# Generating nuget package for " . $projectName . " version " . $versionNumber);
	exec_system("nuget pack -Verbosity quiet -NonInteractive");
		
	print_log("# Uploading nuget package " . $projectName . " version " . $versionNumber . " to nuget server");
	exec_system("nuget push -Verbosity quiet -NonInteractive *.nupkg");
	exec_system("mv *.nupkg ../../" . $nugetPackageDirectory);
		
	move_chdir('../..');
	
	print_log("# Add new packages to git");
	exec_system("git add -f nuget_packages/*.nupkg");
	$commitMessage = "Build file for release " . $projectName . " in version " . $versionNumber;
	$commitMessage = str_replace('"', '\\"', $commitMessage);
	
	print_log("# Commit packages to git");
	
	$tag = $projectName . '_v' . $versionNumber;
	exec_system('git tag -a ' . $tag . ' -m "Release ' . $projectName . ' in version ' . $versionNumber . '"');
	exec_system('git commit -m "' . $commitMessage . '"');
	
	print_log("# Merging to branch master and develop");;
	exec_system('git checkout master && git merge --no-ff ' . $branchName . ' && git push');
	exec_system('git checkout develop && git merge --no-ff ' . $branchName . ' && git push');
	exec_system('git branch -d ' . $branchName);
	
	// Push all tags
	print_log("# Pushing tags to remote");
	exec_system('git push origin ' . $tag);
	
	print_log("# Finished Release ! :)");
	
	