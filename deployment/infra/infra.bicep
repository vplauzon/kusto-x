/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location

var suffix = uniqueString(resourceGroup().id, 'kusto-x')
var environments = [
  'dev'
  'stg'
  'prd'
]

module storageModule 'storage.bicep' = [for environment in environments:{
  name: '${environment}-storageDeploy'
  params: {
    location: location
    prefix: environment
    suffix: suffix
  }
}]

module appModule 'app.bicep' = [for environment in environments:{
  name: '${environment}-appDeploy'
  params: {
    location: location
    prefix: environment
    suffix: suffix
  }
}]
