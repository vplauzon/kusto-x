#!/bin/bash

##########################################################################
##  Deploys shell solution
##
##  Parameters:
##
##  1- Name of resource group

rg=$1

echo "Resource group:  $rg"
echo "Current directory:  $(pwd)"

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file public-apps.json
