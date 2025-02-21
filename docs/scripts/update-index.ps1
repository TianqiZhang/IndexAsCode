# Parameters for the script
param(
    [Parameter(Mandatory=$true)]
    [string]$IndexFile,
    
    [Parameter(Mandatory=$true)]
    [string]$Endpoint,
    
    [Parameter(Mandatory=$false)]
    [string]$IndexName
)

# Build the common parameters
$params = @("--index-file", $IndexFile, "--endpoint", $Endpoint)
if ($IndexName) {
    $params += "--index-name"
    $params += $IndexName
}

# First, run the diff command to check for changes
Write-Host "Checking for index differences..."
$diffOutput = & IndexAsCode.Tools diff $params
$exitCode = $LASTEXITCODE

# Check if there was an error running the diff command
if ($exitCode -ne 0) {
    Write-Error "Error running diff command: $diffOutput"
    exit $exitCode
}

# If the index doesn't exist or has differences, proceed with update
if ($diffOutput -match "Index does not exist" -or $diffOutput -match "Found differences") {
    Write-Host "Changes detected, updating index..."
    & IndexAsCode.Tools update $params
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -ne 0) {
        Write-Error "Error updating index"
        exit $exitCode
    }
    Write-Host "Index updated successfully"
} else {
    Write-Host "No changes needed"
}