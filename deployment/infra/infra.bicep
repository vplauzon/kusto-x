/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location
@description('Name of environment')
param environment string

var suffix = uniqueString(resourceGroup().id, 'kusto-x')

module storageModule 'storage.bicep' = {
  name: '${environment}-storageDeploy'
  params: {
    location: location
    prefix: environment
    suffix: suffix
    retentionInDays: environment == 'tst' ? 1 : 30
  }
}

module appModule 'app.bicep' = {
  name: '${environment}-appDeploy'
  params: {
    location: location
    prefix: environment
    suffix: suffix
  }
}

