# CI/CD Pipeline Examples

This directory contains example configurations for common CI/CD platforms showing how to integrate Index as Code into your deployment pipeline.

## Azure Pipelines
```yaml
# azure-pipelines.yml
trigger:
  - main

variables:
  - group: search-service-config # Contains searchEndpoint variables per environment

stages:
  - stage: ValidateIndexChanges
    jobs:
      - job: DiffCheck
        steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: 'build'
              projects: '**/*.csproj'
          
          - script: |
              IndexAsCode.Tools diff -f search.index.json -e $(searchEndpoint) -n search-dev
            displayName: 'Check Index Changes'
            failOnStderr: true

  - stage: DeployToDev
    jobs:
      - deployment: UpdateDevIndex
        environment: 'development'
        strategy:
          runOnce:
            deploy:
              steps:
                - script: |
                    ./docs/scripts/update-index.sh -f search.index.json -e $(searchEndpoint) -n search-dev
                  displayName: 'Update Dev Search Index'

  - stage: DeployToStaging
    jobs:
      - deployment: UpdateStagingIndex
        environment: 'staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - script: |
                    ./docs/scripts/update-index.sh -f search.index.json -e $(searchEndpoint) -n search-staging
                  displayName: 'Update Staging Search Index'

  - stage: DeployToProd
    jobs:
      - deployment: UpdateProdIndex
        environment: 'production'
        strategy:
          runOnce:
            deploy:
              steps:
                - script: |
                    ./docs/scripts/update-index.sh -f search.index.json -e $(searchEndpoint) -n search
                  displayName: 'Update Production Search Index'
```

## GitHub Actions
```yaml
# .github/workflows/deploy-search-index.yml
name: Deploy Search Index

on:
  push:
    branches: [ main ]
    paths:
      - '**.index.json'
  pull_request:
    paths:
      - '**.index.json'

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '9.0.x'
      
      - name: Build Solution
        run: dotnet build
      
      - name: Check Index Changes
        run: IndexAsCode.Tools diff -f search.index.json -e ${{ secrets.SEARCH_ENDPOINT }} -n search-dev

  deploy-dev:
    needs: validate
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    environment: development
    steps:
      - uses: actions/checkout@v2
      - name: Update Dev Index
        run: ./docs/scripts/update-index.sh -f search.index.json -e ${{ secrets.SEARCH_ENDPOINT }} -n search-dev

  deploy-prod:
    needs: deploy-dev
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    environment: production
    steps:
      - uses: actions/checkout@v2
      - name: Update Production Index
        run: ./docs/scripts/update-index.sh -f search.index.json -e ${{ secrets.SEARCH_ENDPOINT }} -n search

## GitLab CI
```yaml
# .gitlab-ci.yml
stages:
  - validate
  - deploy-dev
  - deploy-staging
  - deploy-prod

variables:
  DOTNET_VERSION: '9.0'

validate:
  stage: validate
  script:
    - dotnet build
    - IndexAsCode.Tools diff -f search.index.json -e $SEARCH_ENDPOINT -n search-dev
  rules:
    - changes:
        - "**.index.json"

deploy-dev:
  stage: deploy-dev
  script:
    - ./docs/scripts/update-index.sh -f search.index.json -e $SEARCH_ENDPOINT -n search-dev
  environment: development
  rules:
    - if: $CI_COMMIT_BRANCH == "main"
      changes:
        - "**.index.json"

deploy-staging:
  stage: deploy-staging
  script:
    - ./docs/scripts/update-index.sh -f search.index.json -e $SEARCH_ENDPOINT -n search-staging
  environment: staging
  rules:
    - if: $CI_COMMIT_BRANCH == "main"
      changes:
        - "**.index.json"

deploy-prod:
  stage: deploy-prod
  script:
    - ./docs/scripts/update-index.sh -f search.index.json -e $SEARCH_ENDPOINT -n search
  environment: production
  when: manual
  rules:
    - if: $CI_COMMIT_BRANCH == "main"
      changes:
        - "**.index.json"
```