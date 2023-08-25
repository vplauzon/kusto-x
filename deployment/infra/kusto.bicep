@description('Deployment location')
param location string = resourceGroup().location
@description('Name of environment')
param environment string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string

resource kusto 'Microsoft.Kusto/clusters@2023-05-02' = {
  name: 'kustox${suffix}'
  location: location
  sku: {
    capacity: 1
    name: 'Dev(No SLA)_Standard_E2a_v4'
    tier: 'Standard'
  }
  properties: {
    enableAutoStop: true
    enablePurge: false
    enableStreamingIngest: true
  }

  resource tests 'databases@2023-05-02' = {
    name: 'tests'
    location: location
    kind: 'ReadWrite'
    properties: {
      hotCachePeriod: '1d'
      softDeletePeriod: '1d'
    }
  }

  resource env 'databases@2023-05-02' = {
    name: environment
    location: location
    kind: 'ReadWrite'
    properties: {
      hotCachePeriod: '30d'
      softDeletePeriod: '30d'
    }
  }
}
