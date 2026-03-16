@description('The name of the App Service')
param name string

@description('The Azure region')
param location string

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: '${name}-plan'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: false
  }
}

resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: name
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      // MISSING: staging slot configuration
      // MISSING: health check path
      // MISSING: managed identity
    }
  }
}

// MISSING: Staging deployment slot
// MISSING: Managed Identity
// MISSING: Diagnostic settings

output url string = 'https://${appService.properties.defaultHostName}'
