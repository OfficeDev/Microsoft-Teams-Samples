#!/bin/bash

# Azure CLI login
az login --use-device-code

az group create -l westus -n FaqBot

# Deploy ARM template using Azure CLI
az deployment group create \
  --resource-group FaqBot \
  --template-file ./infra/azure.ai.resources.json