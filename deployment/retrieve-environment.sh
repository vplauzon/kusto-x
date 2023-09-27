#!/bin/bash

##########################################################################
##  Retrieve environment given the current branch
##
##  Parameters:
##
##  1- Branch

branch=$1

if [ $branch == "main" ]
then
    echo "dev"
elif [ $branch == "staging" ]
then
    echo "stg"
elif [ $branch == "prod" ]
then
    echo "prd"
else
    echo "dev"
fi
