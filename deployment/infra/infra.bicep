/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location

var suffix = uniqueString(resourceGroup().id, 'kusto-x')

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'storage${suffix}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}
