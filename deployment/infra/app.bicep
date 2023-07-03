/**************************************************/
//  Deploy Kusto-X infra

@description('Deployment location')
param location string = resourceGroup().location
@description('Resource prefix, typically the name of the environment')
param environment string
@description('Workbench container\'s full version')
param workbenchVersion string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string

var containerFetchingIdentityName = 'container-fetching'

resource registry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: '${environment}registry${suffix}'
}

resource containerFetchingIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${environment}-container-fetching-${suffix}'
  location: location
}

resource appEnvironment 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: '${environment}env${suffix}'
  location: location
  sku: {
    name: 'Consumption'
  }
  properties: {
    zoneRedundant: false
  }
}

resource workbench 'Microsoft.App/containerApps@2022-10-01' = {
  name: 'workbench'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      containerFetchingIdentityName: containerFetchingIdentity
    }
  }
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        allowInsecure: false
        exposedPort: 0
        external: true
        targetPort: 443
        transport: 'auto'
      }
      registries: [
        // {
        //   identity: containerFetchingIdentity.id
        //   server: '${registry.name}.azurecr.io'
        // }
      ]
    }
    environmentId: appEnvironment.id
    template: {
      containers: [
        {
          image: '${registry.name}.azurecr.io/kustox/workbench:${workbenchVersion}'
          name: 'main-workbench'
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}
