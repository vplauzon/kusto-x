#!/bin/bash

##########################################################################
##  Deploys Integration Test Azure infrastructure solution

rg=$1
env=$2
workbenchVersion=$3
apiVersion=$4
tenantId=$5
appId=$6
appSecret=$7

echo "Resource group:  $rg"
echo "Environment:  $env"
echo "Workbench Version:  $workbenchVersion"
echo "Workbench Version:  $apiVersion"
echo "Workbench Version:  $tenantId"
echo "Workbench Version:  $appId"
echo "Workbench Version:  $appSecret"
echo "Current directory:  $(pwd)"

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file infra.bicep \
    --parameters environment=$env \
    workbenchVersion=$workbenchVersion apiVersion=$apiVersion \
    tenantId=$tenantId appId=$appId appSecret=$appSecret
