//  Deploy Kusto-X infra

@description('Name of environment')
param environment string
@description('Workbench container\'s full version')
param workbenchVersion string
@description('Deployment location')
param location string = resourceGroup().location

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
    suffix: suffix
  }
}

module frontDoorModule 'front-door.bicep' = {
  name: '${environment}-frontDoorDeploy'
  params: {
    environment: environment
    workbenchUrl: appModule.outputs.workbenchUrl
    suffix: suffix
  }
}
