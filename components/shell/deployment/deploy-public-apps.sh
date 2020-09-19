#!/bin/bash

##########################################################################
##  Deploys shell solution
##
##  Parameters:
##
##  1- Name of resource group

rg=$1

echo "Resource group:  $rg"

if [[ $(az group exists -g $rg) = 'true' ]]
then
    echo "Resource Group already exists"
else
    echo "Resource Group doesn't exist"
    echo "Creating Resource group $rg"
    az group create -n $rg --location eastus
fi

echo
echo "Deploying ARM template"

az deployment group create -n "deploy-$(uuidgen)" -g $rg \
    --template-file public-apps.json