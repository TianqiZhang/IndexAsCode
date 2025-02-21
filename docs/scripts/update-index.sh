#!/bin/bash

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --index-file|-f)
      INDEX_FILE="$2"
      shift 2
      ;;
    --endpoint|-e)
      ENDPOINT="$2"
      shift 2
      ;;
    --index-name|-n)
      INDEX_NAME="$2"
      shift 2
      ;;
    *)
      echo "Unknown parameter: $1"
      exit 1
      ;;
  esac
done

# Validate required parameters
if [ -z "$INDEX_FILE" ] || [ -z "$ENDPOINT" ]; then
    echo "Required parameters missing. Usage:"
    echo "update-index.sh --index-file <file> --endpoint <url> [--index-name <name>]"
    exit 1
fi

# Build parameters array
PARAMS=("--index-file" "$INDEX_FILE" "--endpoint" "$ENDPOINT")
if [ ! -z "$INDEX_NAME" ]; then
    PARAMS+=("--index-name" "$INDEX_NAME")
fi

# Run diff command
echo "Checking for index differences..."
if ! diff_output=$(IndexAsCode.Tools diff "${PARAMS[@]}"); then
    echo "Error running diff command: $diff_output"
    exit 1
fi

# Check if update is needed
if echo "$diff_output" | grep -q "Index does not exist\|Found differences"; then
    echo "Changes detected, updating index..."
    if ! IndexAsCode.Tools update "${PARAMS[@]}"; then
        echo "Error updating index"
        exit 1
    fi
    echo "Index updated successfully"
else
    echo "No changes needed"
fi