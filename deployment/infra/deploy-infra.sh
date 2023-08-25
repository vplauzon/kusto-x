#!/bin/bash

##########################################################################
##  Deploys Integration Test Azure infrastructure solution

rg=$1
env=$2
tenantId=$3
appId=$4
# workbenchVersion=$3
# appSecret=$6

echo "Resource group:  $rg"
echo "Environment:  $env"
echo "Tenant ID:  $tenantId"
echo "App ID:  $appId"
# echo "Workbench Version:  $workbenchVersion"
# echo "App Secret:  $appSecret"
echo "Current directory:  $(pwd)"

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file infra.bicep \
    --parameters environment=$env \
    tenantId=$tenantId testAppId=$appId