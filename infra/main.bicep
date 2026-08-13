@description('Globally unique App Service name. Try clearpay, then clearpay-wallet, then hm-clearpay.')
param webAppName string = 'hm-clearpay'

@description('Azure region. CANLI: West Europe.')
param location string = resourceGroup().location

param sqlAdminLogin string = 'clearpayadmin'

@secure()
param sqlAdminPassword string

@description('Deploy Azure Cache for Redis (Q2). CloudAMQP is not Azure — set ConnectionStrings__RabbitMq in the portal after signup.')
param deployQ2 bool = false

param databaseName string = 'ClearPay'

var sqlServerName = 'sql-${webAppName}-${uniqueString(resourceGroup().id)}'
var planName = 'plan-${webAppName}'

resource plan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: planName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
}

resource allowAzureSql 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAllAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

var sqlConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${databaseName};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'Hangfire__WorkerEnabled'
          value: 'true'
        }
      ]
      connectionStrings: [
        {
          name: 'ClearPay'
          connectionString: sqlConnectionString
          type: 'SQLAzure'
        }
      ]
    }
  }
  dependsOn: [
    sqlDb
    allowAzureSql
  ]
}

module q2 'q2.bicep' = if (deployQ2) {
  name: 'q2-redis'
  params: {
    location: location
    redisName: 'redis-${webAppName}-${uniqueString(resourceGroup().id)}'
  }
}

output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output sqlFqdn string = sqlServer.properties.fullyQualifiedDomainName
output resourceGroupName string = resourceGroup().name
