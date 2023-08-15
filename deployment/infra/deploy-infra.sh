#!/bin/bash

##########################################################################
##  Deploys Integration Test Azure infrastructure solution

rg=$1
env=$2
workbenchVersion=$3
tenantId=$4
appId=$5
appSecret=$6

echo "Resource group:  $rg"
echo "Environment:  $env"
echo "Workbench Version:  $workbenchVersion"
echo "Tenant ID:  $tenantId"
echo "App ID:  $appId"
echo "App Secret:  $appSecret"
echo "Current directory:  $(pwd)"

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file infra.bicep \
    --parameters environment=$env \
    workbenchVersion=$workbenchVersion \
    tenantId=$tenantId appId=$appId appSecret=$appSecret
