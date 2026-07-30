#!/bin/bash

# Azure CLI login
az login --use-device-code

az group create -l westus -n ChatWithYourData

# Deploy ARM template using Azure CLI
az deployment group create \
  --resource-group ChatWithYourData \
  --template-file ./infra/azure.ai.resources.json
