#!/bin/bash

##########################################################################
##  Deploys Integration Test Azure infrastructure solution
##
##  Parameters:
##
##  1- Resource group
##  2- Environment
##  3- Workbench full version

rg=$1
env=$2
workbenchVersion=$3

echo "Resource group:  $rg"
echo "Environment:  $env"
echo "Workbench Version:  $workbenchVersion"
echo "Current directory:  $(pwd)"

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file infra.bicep \
    --parameters environment=$env workbenchVersion=$workbenchVersion
