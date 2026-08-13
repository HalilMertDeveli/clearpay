@description('Q2 cache only. Do not output access keys. Add ConnectionStrings__Redis in the portal after deploy (host + primary key).')
param location string
param redisName string

resource redis 'Microsoft.Cache/redis@2023-08-01' = {
  name: redisName
  location: location
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    redisConfiguration: {
      'maxmemory-policy': 'volatile-lru'
    }
  }
}

output redisHost string = redis.properties.hostName
output redisSslPort int = 6380
output redisConnectionHint string = '${redis.properties.hostName}:6380,ssl=true,abortConnect=False,password=<PORTAL_PRIMARY_KEY>'
