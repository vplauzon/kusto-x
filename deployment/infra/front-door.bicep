//  Deploy Kusto-X Front door

@description('Resource prefix, typically the name of the environment')
param environment string
@description('Workbench Url')
param workbenchUrl string
@description('Suffix to resource, typically to make the resource name unique')
param suffix string

var FrontDoorName = 'front-door-${suffix}'

resource Front_Door 'Microsoft.Network/frontDoors@2021-06-01' = {
  name: FrontDoorName
  location: 'global'
  properties: {
    healthProbeSettings: [
      {
        name: 'healthProbeSettings'
        properties: {
          path: '/'
          protocol: 'Https'
          intervalInSeconds: 120
        }
      }
    ]
    loadBalancingSettings: [
      {
        name: 'loadBalancingSettings'
        properties: {
          sampleSize: 4
          successfulSamplesRequired: 2
        }
      }
    ]
    backendPools: [
      {
        name: '${environment}-workbench'
        properties: {
          backends: [
            {
              address: workbenchUrl
              backendHostHeader: workbenchUrl
              httpsPort: 443
              httpPort: 80
              weight: 100
              priority: 1
            }
          ]
          loadBalancingSettings: {
            id: resourceId('Microsoft.Network/frontDoors/loadBalancingSettings', FrontDoorName, 'loadBalancingSettings')
          }
          healthProbeSettings: {
            id: resourceId('Microsoft.Network/frontDoors/healthProbeSettings', FrontDoorName, 'healthProbeSettings')
          }
        }
      }
    ]
    frontendEndpoints: [
      {
        name: 'defaultFrontendEndpoint'
        properties: {
          hostName: '${FrontDoorName}.azurefd.net'
          sessionAffinityEnabledState: 'Disabled'
          sessionAffinityTtlSeconds: 0
        }
      }
    ]
    routingRules: [
      {
        name: 'prodRoutingRule'
        properties: {
          frontendEndpoints: [
            {
              id: resourceId('Microsoft.Network/frontDoors/frontendEndpoints', FrontDoorName, 'defaultFrontendEndpoint')
            }
          ]
          acceptedProtocols: [
            'Https'
          ]
          patternsToMatch: [
            '/*'
          ]
          routeConfiguration: {
            '@odata.type': '#Microsoft.Azure.FrontDoor.Models.FrontdoorForwardingConfiguration'
            forwardingProtocol: 'HttpsOnly'
            backendPool: {
              id: resourceId('Microsoft.Network/frontDoors/backendPools', FrontDoorName, '${environment}-workbench')
            }
          }
          enabledState: 'Enabled'
        }
      }
    ]
    enabledState: 'Enabled'
  }
}
