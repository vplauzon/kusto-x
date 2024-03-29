//  Deploy Kusto-X Apps

@description('Deployment location')
param location string = resourceGroup().location
@description('Resource prefix, typically the name of the environment')
param environment string
@description('Suffix used for resources')
param suffix string
@description('Workbench container\'s full version')
param workbenchVersion string
@description('AAD Tenant Id')
param tenantId string
@description('AAD App Id')
param appId string
@description('AAD App Secret')
@secure()
param appSecret string

var appSecretName = 'app-secret'

resource registry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: '${environment}registry${suffix}'
}

resource kusto 'Microsoft.Kusto/clusters@2023-05-02' existing = {
  name: 'kustox${suffix}'
}

//  Identity pulling the containers from the registry
resource containerFetchingIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${environment}-id-container-${suffix}'
  location: location
}

//  Identity orchestrating, i.e. accessing Kusto + Storage
resource orchestratorIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${environment}-id-orchestrator-${suffix}'
  location: location
}

//  We also need to authorize the user identity to pull container images from the registry
resource containerFetchingIdentityRbacAuthorization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerFetchingIdentity.id, registry.id, 'rbac')
  scope: registry

  properties: {
    description: 'Giving AcrPull RBAC to identity'
    principalId: containerFetchingIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
}

//  Give the orchestrator identity admin rights on the cluster's data plane
resource symbolicname 'Microsoft.Kusto/clusters/principalAssignments@2023-05-02' = {
  name: 'orchestrator-assignment'
  parent: kusto
  properties: {
    principalId: orchestratorIdentity.properties.principalId
    principalType: 'App'
    role: 'AllDatabasesAdmin'
    tenantId: tenantId
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
    containerFetchingIdentityRbacAuthorization
  ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerFetchingIdentity.id}': {}
      '${orchestratorIdentity.id}': {}
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
      secrets: [
        {
          name: appSecretName
          value: appSecret
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
          env: [
            {
              name: 'kustoCluster'
              value: kusto.properties.uri
            }
            {
              name: 'kustoDb-sandbox'
              value: '${environment}-sandbox'
            }
            {
              name: 'kustoDb-state'
              value: environment
            }
            {
              name: 'userIdentityResourceId'
              value: orchestratorIdentity.id
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }

  resource symbolicname 'authConfigs' = {
    name: 'current'
    properties: {
      globalValidation: {
        redirectToProvider: 'azureactivedirectory'
        unauthenticatedClientAction: 'RedirectToLoginPage'
      }
      identityProviders: {
        azureActiveDirectory: {
          isAutoProvisioned: false
          registration: {
            clientId: appId
            clientSecretSettingName: appSecretName
            openIdIssuer: 'https://sts.windows.net/${tenantId}/v2.0'
          }
          validation: {
            allowedAudiences: []
            defaultAuthorizationPolicy: {
              allowedApplications: []
            }
          }
        }
      }
      login: {
        preserveUrlFragmentsForLogins: false
      }
      platform: {
        enabled: true
      }
    }
  }
}

output workbenchUrl string = workbench.properties.latestRevisionFqdn
