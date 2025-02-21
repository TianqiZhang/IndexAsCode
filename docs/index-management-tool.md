# Index Management Tool Guide

The solution includes a command-line tool that helps maintain synchronization between your local index definitions and Azure Search.

## Commands

### Comparing Index Definitions (diff)
```bash
IndexAsCode.Tools diff --index-file hotels.index.json --endpoint https://your-search-service.search.windows.net [--index-name optional-name]
```
Or using short aliases:
```bash
IndexAsCode.Tools diff -f hotels.index.json -e https://your-search-service.search.windows.net [-n optional-name]
```

### Updating Index Definitions (update)
```bash
IndexAsCode.Tools update --index-file hotels.index.json --endpoint https://your-search-service.search.windows.net [--index-name optional-name]
```

## Command Options
- `--index-file` or `-f`: The path to the index definition JSON file (required)
- `--endpoint` or `-e`: The Azure Search service endpoint URL (required)
- `--index-name` or `-n`: Optional name to override the index name from the JSON file

## Automated Deployment Scripts

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