//  Deploy Kusto-X infra

@description('Name of environment')
param environment string
@description('Workbench container\'s full version')
param workbenchVersion string
@description('API container\'s full version')
param apiVersion string
@description('Deployment location')
param location string = resourceGroup().location
@description('AAD Tenant Id')
param tenantId string
@description('Workbench AAD App Id')
param workbenchAppId string
@description('Workbench AAD App Secret')
@secure()
param workbenchAppSecret string

module suffixModule '../suffix.bicep' = {
  name: '${environment}-suffix'
}

var suffix = suffixModule.outputs.suffix

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
    environment: environment
    workbenchVersion: workbenchVersion
    apiVersion: apiVersion
    suffix: suffix
    tenantId: tenantId
    appId: workbenchAppId
    appSecret: workbenchAppSecret
  }
}

// module frontDoorModule 'front-door.bicep' = {
//   name: '${environment}-frontDoorDeploy'
//   params: {
//     environment: environment
//     workbenchUrl: appModule.outputs.workbenchUrl
//     suffix: suffix
//   }
// }
