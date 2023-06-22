/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location
@description('Resource prefix, typically the name of the environment')
param prefix string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string
@description('Retention of blobs in days')
param retentionInDays int

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

  resource symbolicname 'managementPolicies' = {
    name: 'default'
    properties: {
      policy: {
        rules: [
          {
            definition: {
              actions: {
                baseBlob: {
                  delete: {
                    daysAfterModificationGreaterThan: retentionInDays
                  }
                }
              }
              filters: {
                blobTypes: [
                  'blockBlob'
                  'appendBlob'
                ]
                prefixMatch: [
                  'data/'
                ]
              }
            }
            enabled: true
            name: 'retention'
            type: 'Lifecycle'
          }
        ]
      }
    }
  }
}
