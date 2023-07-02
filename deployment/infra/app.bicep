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
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        allowInsecure: false
        clientCertificateMode: 'ignore'
        exposedPort: 80
        external: true
        targetPort: 80
        transport: 'auto'
      }
      // registries: [
      //   {
      //     identity: 'system'
      //     server: 'string'
      //   }
      // ]
    }
    environmentId: appEnvironment.id
    template: {
      containers: [
        {
          args: [
            'string'
          ]
          command: [
            'string'
          ]
          env: [
            {
              name: 'string'
              secretRef: 'string'
              value: 'string'
            }
          ]
          image: 'string'
          name: 'string'
          probes: [
            {
              failureThreshold: int
              httpGet: {
                host: 'string'
                httpHeaders: [
                  {
                    name: 'string'
                    value: 'string'
                  }
                ]
                path: 'string'
                port: int
                scheme: 'string'
              }
              initialDelaySeconds: int
              periodSeconds: int
              successThreshold: int
              tcpSocket: {
                host: 'string'
                port: int
              }
              terminationGracePeriodSeconds: int
              timeoutSeconds: int
              type: 'string'
            }
          ]
          resources: {
            cpu: json('decimal-as-string')
            memory: 'string'
          }
          volumeMounts: [
            {
              mountPath: 'string'
              volumeName: 'string'
            }
          ]
        }
      ]
      initContainers: [
        {
          args: [
            'string'
          ]
          command: [
            'string'
          ]
          env: [
            {
              name: 'string'
              secretRef: 'string'
              value: 'string'
            }
          ]
          image: 'string'
          name: 'string'
          resources: {
            cpu: json('decimal-as-string')
            memory: 'string'
          }
          volumeMounts: [
            {
              mountPath: 'string'
              volumeName: 'string'
            }
          ]
        }
      ]
      revisionSuffix: 'string'
      scale: {
        maxReplicas: int
        minReplicas: int
        rules: [
          {
            azureQueue: {
              auth: [
                {
                  secretRef: 'string'
                  triggerParameter: 'string'
                }
              ]
              queueLength: int
              queueName: 'string'
            }
            custom: {
              auth: [
                {
                  secretRef: 'string'
                  triggerParameter: 'string'
                }
              ]
              metadata: {}
              type: 'string'
            }
            http: {
              auth: [
                {
                  secretRef: 'string'
                  triggerParameter: 'string'
                }
              ]
              metadata: {}
            }
            name: 'string'
            tcp: {
              auth: [
                {
                  secretRef: 'string'
                  triggerParameter: 'string'
                }
              ]
              metadata: {}
            }
          }
        ]
      }
      volumes: [
        {
          name: 'string'
          storageName: 'string'
          storageType: 'string'
        }
      ]
    }
  }
}
