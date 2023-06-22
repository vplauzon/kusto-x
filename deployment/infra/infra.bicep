/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location

var suffix = uniqueString(resourceGroup().id, 'kusto-x')

module storageModule 'storage.bicep' = {
  name: 'storageDeploy'
  params: {
    location: location
    prefix: 'dev'
    suffix: suffix
  }
}
