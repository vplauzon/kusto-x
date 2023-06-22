/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location
@description('Resource prefix, typically the name of the environment')
param prefix string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${prefix}storage${suffix}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'

  resource blobServices 'blobServices' = {
    name: 'default'

    resource mycontainer 'containers' = {
      name: 'data'
      properties: {
        publicAccess: 'None'
      }
    }
  }
}
