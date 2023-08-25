/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location
@description('Name of environment')
param environment string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string
@description('Retention of blobs in days')
param retentionInDays int
@description('AAD Tenant Id')
param tenantId string
@description('Test SP App Id')
param testAppId string
@description('Test SP App Secret')
@secure()
param testAppSecret string

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'storage${suffix}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'

  resource blobServices 'blobServices' = {
    name: 'default'

    resource environmentContainer 'containers' = {
      name: environment
      properties: {
        publicAccess: 'None'
      }
    }

    resource testContainer 'containers' = {
      name: 'test'
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
                  '${environment}/'
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

//  We need to authorize test app to read / write
resource clusterEventHubAuthorization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(testAppId, storage.name, 'data-plane')
  //  See https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/scope-extension-resources
  //  for scope for extension
  scope: storage::blobServices::testContainer
  
  properties: {
    description: 'Give "Storage Blob Data Owner" to the SP'
    principalId: testAppId
    //  Required in case principal not ready when deploying the assignment
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
  }
}
