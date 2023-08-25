@description('Deployment location')
param location string = resourceGroup().location
@description('Name of environment')
param environment string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string
@description('AAD Tenant Id')
param tenantId string
@description('Test SP App Id')
param testAppId string

resource kusto 'Microsoft.Kusto/clusters@2023-05-02' = {
  name: 'kustox${suffix}'
  location: location
  sku: {
    capacity: 1
    name: 'Dev(No SLA)_Standard_E2a_v4'
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

  //  We need to authorize test app to admin the DB
    resource testAssignment 'principalAssignments' = {
      name: 'test-assignment'
      properties: {
        principalId: testAppId
        principalType: 'App'
        role: 'Admin'
        tenantId: tenantId
      }
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
  }
}
