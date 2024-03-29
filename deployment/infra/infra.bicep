//  Deploy Kusto-X infra

@description('Name of environment')
param environment string
@description('Deployment location')
param location string = resourceGroup().location
@description('AAD Tenant Id')
param tenantId string
@description('Test SP Object Id')
param testObjectId string
// @description('Workbench container\'s full version')
// param workbenchVersion string
// @description('API container\'s full version')
// param apiVersion string

module suffixModule '../suffix.bicep' = {
  name: '${environment}-suffix'
}

var suffix = suffixModule.outputs.suffix

module storageModule 'storage.bicep' = {
  name: '${environment}-storageDeploy'
  params: {
    location: location
    suffix: suffix
    testObjectId: testObjectId
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
