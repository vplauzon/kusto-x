/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location
@description('Resource prefix, typically the name of the environment')
param prefix string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string

resource environment 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: '${prefix}env${suffix}'
  location: location
  sku: {
    name: 'Consumption'
  }
  properties: {
    zoneRedundant: false
  }
}
