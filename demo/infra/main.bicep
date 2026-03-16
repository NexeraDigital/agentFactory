// VIOLATION FILE
//
// VIOLATES:
//   SEC-006 — Contains hardcoded secrets (SQL admin password).
//             Secrets must come from Azure Key Vault or deployment parameters
//             marked @secure(), never hardcoded in templates.
//
// ALSO MISSING:
//   - Azure Key Vault for secret management
//   - Managed Identity for service-to-service auth
//   - Staging slot on App Service for safe deployments

@description('The environment name (dev, staging, prod)')
param environmentName string = 'dev'

@description('The Azure region for all resources')
param location string = resourceGroup().location

@description('The name prefix for all resources')
param namePrefix string = 'taskboard'

// VIOLATION SEC-006: Hardcoded SQL admin password in plain text
// Should be: @secure() param or Key Vault reference
var sqlAdminPassword = 'P@ssw0rd123!'  // VIOLATION: secret in source code
var sqlAdminUser = 'sqladmin'

var uniqueSuffix = uniqueString(resourceGroup().id)
var appName = '${namePrefix}-${environmentName}-${uniqueSuffix}'

module appService 'modules/app-service.bicep' = {
  name: 'appService'
  params: {
    name: '${appName}-app'
    location: location
    // MISSING: no staging slot, no managed identity
  }
}

module sqlDatabase 'modules/sql-database.bicep' = {
  name: 'sqlDatabase'
  params: {
    name: '${appName}-sql'
    location: location
    adminLogin: sqlAdminUser
    adminPassword: sqlAdminPassword  // VIOLATION: passing hardcoded secret
  }
}

module storageAccount 'modules/storage-account.bicep' = {
  name: 'storageAccount'
  params: {
    name: replace('${namePrefix}${environmentName}${uniqueSuffix}', '-', '')
    location: location
  }
}

// MISSING: Key Vault module for secret management
// MISSING: Managed Identity assignment
// MISSING: Private endpoints for SQL and Storage

output appServiceUrl string = appService.outputs.url
output sqlServerFqdn string = sqlDatabase.outputs.serverFqdn
