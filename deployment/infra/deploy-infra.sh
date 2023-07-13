#!/bin/bash

##########################################################################
##  Deploys Integration Test Azure infrastructure solution

rg=$1
env=$2
workbenchVersion=$3
tenantId=$4
appWorkbenchId=$5
appWorkbenchSecret=$6

echo "Resource group:  $rg"
echo "Environment:  $env"
echo "Workbench Version:  $workbenchVersion"
echo "Current directory:  $(pwd)"

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file infra.bicep \
    --parameters environment=$env workbenchVersion=$workbenchVersion \
    tenantId=$tenantId workbenchAppId=$appWorkbenchId workbenchAppSecret=$appWorkbenchSecret
