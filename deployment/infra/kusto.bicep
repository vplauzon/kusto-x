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
    capacity: 2
    name: 'Standard_E8s_v4+2TB_PS'
    tier: 'Basic'
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
      hotCachePeriod: 'P1D'
      softDeletePeriod: 'P1D'
    }

    resource testsScript 'scripts' = {
      name: 'kustox-tracking'
      properties:{
        scriptContent:  loadTextContent('kustox-tracking.kql')
      }
    }
  }

  resource testsSandbox 'databases@2023-05-02' = {
    name: 'tests-sandbox'
    location: location
    kind: 'ReadWrite'
    properties: {
      hotCachePeriod: 'P1D'
      softDeletePeriod: 'P1D'
    }
  }

  resource env 'databases@2023-05-02' = {
    name: environment
    location: location
    kind: 'ReadWrite'
    properties: {
      hotCachePeriod: 'P30D'
      softDeletePeriod: 'P30D'
    }

    resource envScript 'scripts' = {
      name: 'kustox-tracking-${environment}'
      dependsOn: [
        tests::testsScript
      ]
      properties:{
        scriptContent:  loadTextContent('kustox-tracking.kql')
      }
    }
  }

  resource envSandbox 'databases@2023-05-02' = {
    name: '${environment}-sandbox'
    location: location
    kind: 'ReadWrite'
    properties: {
      hotCachePeriod: 'P30D'
      softDeletePeriod: 'P30D'
    }
  }
}
