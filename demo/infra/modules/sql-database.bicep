@description('The name of the SQL Server')
param name string

@description('The Azure region')
param location string

@description('SQL admin login')
param adminLogin string

@description('SQL admin password')
param adminPassword string

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: name
  location: location
  properties: {
    administratorLogin: adminLogin
    administratorLoginPassword: adminPassword
    version: '12.0'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: 'TaskBoard'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

// Allow Azure services to connect (for demo simplicity)
resource firewallRule 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
