/**************************************************/
//  Deploy registry

@description('Name of environment')
param environment string
@description('Deployment location')
param location string = resourceGroup().location

module suffixModule '../suffix.bicep' = {
  name: '${environment}-suffix'
}

var suffix = suffixModule.outputs.suffix

module storageModule 'registry.bicep' = {
  name: '${environment}-storageDeploy'
  params: {
    environment: environment
    suffix: suffix
    location: location
  }
}
