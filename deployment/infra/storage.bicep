/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location
@description('Suffix to resource, typically to make the resource name unique')
param suffix string
@description('Test SP Object Id')
param testObjectId string

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'kustoxsto${suffix}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    isHnsEnabled: true
  }

  resource blobServices 'blobServices' = {
    name: 'default'

    resource testContainer 'containers' = {
      name: 'test'
      properties: {
        publicAccess: 'None'
      }
    }
  }
}

//  Storage Blob Data Contributor
//  cf https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
var dataContributor = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

//  We need to authorize test app to read / write
resource testStorageAuthorization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, testObjectId, storage.name, dataContributor, 'data-plane')
  //  See https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/scope-extension-resources
  //  for scope for extension
  scope: storage::blobServices::testContainer

  properties: {
    description: 'Give "Storage Blob Data Owner" to the SP'
    principalId: testObjectId
    //  Required in case principal not ready when deploying the assignment
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', dataContributor)
  }
}
