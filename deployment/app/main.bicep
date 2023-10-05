//  Deploy Kusto-X Apps

@description('Deployment location')
param location string = resourceGroup().location
@description('Resource prefix, typically the name of the environment')
param environment string
@description('Workbench container\'s full version')
param workbenchVersion string
@description('AAD Tenant Id')
param tenantId string
@description('AAD App Id')
param appId string
@description('AAD App Secret')
@secure()
param appSecret string

module suffixModule '../suffix.bicep' = {
  name: 'Suffix-${environment}'
}

//  We use indirection because of suffix which need to be "known at deployment time" for AAD-bound resources
module appModule 'app.bicep' = {
  name: 'App-${environment}'
  params: {
    location: location
    suffix: suffixModule.outputs.suffix
    environment: environment
    workbenchVersion: workbenchVersion
    tenantId: tenantId
    appId: appId
    appSecret: appSecret
  }
}
