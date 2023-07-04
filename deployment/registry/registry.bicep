/**************************************************/
//  Deploy registry

@description('Name of environment')
param environment string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string
@description('Deployment location')
param location string = resourceGroup().location

resource registry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: '${environment}-registry-${suffix}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
    anonymousPullEnabled: false
    dataEndpointEnabled: false
    policies: {
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
      retentionPolicy: {
        status: 'disabled'
      }
      softDeletePolicy: {
        status: 'disabled'
      }
    }
    publicNetworkAccess: 'enabled'
    zoneRedundancy: 'disabled'
  }
}

output registry string = registry.name
