/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location

var suffix = uniqueString(resourceGroup().id, 'kusto-x')
var environments = [
  'dev'
  'staging'
  'prod'
]

module storageModule 'storage.bicep' = [for environment in environments:{
  name: 'storageDeploy-${environment}'
  params: {
    location: location
    prefix: environment
    suffix: suffix
  }
}]
