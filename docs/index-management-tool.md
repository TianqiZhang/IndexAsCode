# Index Management Tool Guide

The Index Management Tool is a key component of the Index as Code pattern, enabling safe and automated synchronization between your source-controlled index definitions and Azure AI Search service.

## Core Concepts

### Diff-Based Updates
The tool follows a "diff first" approach:
1. Compare local definition with remote index
2. Show detailed differences
3. Only update when changes are detected

### Environment Management
Support different environments through index name overrides:
- `-n search-dev` for development
- `-n search-staging` for staging
- `-n search` for production

## Commands

### Comparing Index Definitions (diff)
```bash
IndexAsCode.Tools diff --index-file hotels.index.json --endpoint https://your-search-service.search.windows.net [--index-name optional-name]
```
Or using short aliases:
```bash
IndexAsCode.Tools diff -f hotels.index.json -e https://your-search-service.search.windows.net [-n optional-name]
```

The diff command shows:
- Added/removed fields
- Changed field attributes
- Modified analyzers or suggesters
- Index-level setting changes

### Updating Index Definitions (update)
```bash
IndexAsCode.Tools update --index-file hotels.index.json --endpoint https://your-search-service.search.windows.net [--index-name optional-name]
```

The update command:
- Creates new index if it doesn't exist
- Updates existing index if changes detected
- Preserves data while updating schema
- Reports success/failure status

## Command Options
- `--index-file` or `-f`: The path to the index definition JSON file (required)
- `--endpoint` or `-e`: The Azure Search service endpoint URL (required)
- `--index-name` or `-n`: Optional name to override the index name from the JSON file

## Common Workflows

### Local Development
```bash
# Check for unexpected changes
IndexAsCode.Tools diff -f search.index.json -e $ENDPOINT -n search-dev

# Apply changes to dev environment
IndexAsCode.Tools update -f search.index.json -e $ENDPOINT -n search-dev
```

### Staging Deployment
```bash
# Create/update staging index
IndexAsCode.Tools update -f search.index.json -e $ENDPOINT -n search-staging

# Verify changes in staging
IndexAsCode.Tools diff -f search.index.json -e $ENDPOINT -n search-staging
```

### Production Deployment
```bash
# Safety check - preview changes
IndexAsCode.Tools diff -f search.index.json -e $ENDPOINT -n search

# Apply if changes are expected
IndexAsCode.Tools update -f search.index.json -e $ENDPOINT -n search
```

## Automated Deployment Scripts

The repository includes scripts that implement a safe deployment pattern:
1. Run diff to detect changes
2. Analyze diff output
3. Update only if changes are expected

### PowerShell Script
See [update-index.ps1](./scripts/update-index.ps1) for an example PowerShell script that combines diff and update commands.

Basic usage:
```powershell
.\update-index.ps1 -IndexFile "hotels.index.json" -Endpoint "https://your-search-service.search.windows.net"
```

### Bash Script
See [update-index.sh](./scripts/update-index.sh) for an equivalent bash script.

Basic usage:
```bash
./update-index.sh --index-file hotels.index.json --endpoint https://your-search-service.search.windows.net
```

## Best Practices

### Version Control
- Keep index definitions with your application code
- Review index changes in pull requests
- Use meaningful commit messages

### Deployment Safety
- Always run diff before update
- Use staging environments for testing
- Consider blue-green deployment for major changes

### CI/CD Integration
- Include diff command in PR checks
- Use scripts for automated deployments
- Set different index names per environment

## Error Handling

The tool provides detailed error messages for common scenarios:
- Invalid JSON syntax
- Missing required fields
- Azure Search service connectivity issues
- Permission problems

Exit codes indicate success (0) or failure (non-zero) for CI/CD integration.