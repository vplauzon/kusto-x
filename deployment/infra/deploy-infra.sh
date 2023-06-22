#!/bin/bash

##########################################################################
##  Deploys Integration Test Azure infrastructure solution
##
##  Parameters:
##
##  1- Resource group
##  1- Environment

rg=$1
env=$2

echo "Resource group:  $rg"
echo "Environment:  $env"
echo "Current directory:  $(pwd)"

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file infra.bicep \
    --parameters environment=$env
