//  Deploy Kusto-X Apps

@description('Deployment location')
param location string = resourceGroup().location
@description('Resource prefix, typically the name of the environment')
param environment string
@description('Workbench container\'s full version')
param workbenchVersion string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string

resource registry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: '${environment}registry${suffix}'
}

resource containerFetchingIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${environment}-id-container-${suffix}'
  location: location
}

//  We also need to authorize the user identity to pull container images from the registry
resource userIdentityRbacAuthorization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerFetchingIdentity.id, registry.id, 'rbac')
  scope: registry

  properties: {
    description: 'Giving AcrPull RBAC to identity'
    principalId: containerFetchingIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
}

resource appEnvironment 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: '${environment}-env-${suffix}'
  location: location
  sku: {
    name: 'Consumption'
  }
  properties: {
    zoneRedundant: false
  }
}

resource workbench 'Microsoft.App/containerApps@2022-10-01' = {
  name: '${environment}-app-workbench-${suffix}'
  location: location
  dependsOn: [
    userIdentityRbacAuthorization
  ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerFetchingIdentity.id}': {}
    }
  }
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        allowInsecure: false
        exposedPort: 0
        external: true
        targetPort: 80
        transport: 'auto'
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          identity: containerFetchingIdentity.id
          server: registry.properties.loginServer
        }
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

output test object = workbench
output workbenchUrl string = workbench.properties.latestRevisionFqdn
