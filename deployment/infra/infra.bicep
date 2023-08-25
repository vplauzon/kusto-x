//  Deploy Kusto-X infra

@description('Name of environment')
param environment string
@description('Deployment location')
param location string = resourceGroup().location
// @description('Workbench container\'s full version')
// param workbenchVersion string
// @description('API container\'s full version')
// param apiVersion string
// @description('AAD Tenant Id')
// param tenantId string
// @description('Workbench AAD App Id')
// param appId string
// @description('Workbench AAD App Secret')
// @secure()
// param appSecret string

module suffixModule '../suffix.bicep' = {
  name: '${environment}-suffix'
}

var suffix = suffixModule.outputs.suffix

module storageModule 'storage.bicep' = {
  name: '${environment}-storageDeploy'
  params: {
    location: location
    environment: environment
    suffix: suffix
    retentionInDays: environment == 'tst' ? 1 : 30
  }
}

module kustoModule 'kusto.bicep' = {
  name: '${environment}-kustoDeploy'
  params: {
    location: location
    environment: environment
    suffix: suffix
  }
}

module registryModule 'registry.bicep' = {
  name: '${environment}-registryDeploy'
  params: {
    environment: environment
    suffix: suffix
    location: location
  }
}

// module appModule 'app.bicep' = {
//   name: '${environment}-appDeploy'
//   params: {
//     location: location
//     environment: environment
//     workbenchVersion: workbenchVersion
//     apiVersion: apiVersion
//     suffix: suffix
//     tenantId: tenantId
//     appId: appId
//     appSecret: appSecret
//   }
// }

// module frontDoorModule 'front-door.bicep' = {
//   name: '${environment}-frontDoorDeploy'
//   params: {
//     environment: environment
//     workbenchUrl: appModule.outputs.workbenchUrl
//     suffix: suffix
//   }
// }
