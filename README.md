# .NET: Application Insights Availability Checks
Application Insights runs availability checks from the public internet and cannot access private networks. This tool is a containerized .NET application that runs in Kubernetes, that is configuration driven, and will monitor internal web applications.   

## Getting Started
This is a guide to setting this up.

### Azure App Config + Key Vault
You can also use this with Azure App Config & Key Vault to store configuration. This may work better in a distributed environment as it uses a centralized configuration store. Changes can be made easily and once the service is restarted it picks up the changes. This makes adding/removing and managing a breeze.

### appsettings.Production.json
This is a samples settings file. Set your values in here and this will kick off the service.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=...;IngestionEndpoint=...",
  "OTEL_SERVICE_NAME": "availabilitychecks",
  "Application": {
    "LogKey": "3484991B-8304-4B3E-8784-BBDABF6DE346"
  },
  "AvailabilityChecks": [
    {
      "Name": "Auth Internal Health",
      "Url": "https://auth.internal.example.com",
      "Environment": "prod",
      "Location": "k8s-internal"
    },
    {
      "Name": "Admin Internal Health",
      "Url": "https://admin.internal.example.com",
      "Environment": "prod",
      "Location": "k8s-internal"
    }
  ]
}
```

## Endpoints

### Up
There is an `Up` endpoint that is an availability endpoint. Kind of ironic, right?   

### Log Test
I always include a key in here, it's not entirely necessary but it does add an extra gate in the process. This can reduce exploitation.   
While this is an internal service, and not likely to be exploited, we also don't want other processes, users, to pound this with traffic.   
If this were to happen it would blow up the logs with data.  
If there were ever a sitatuation and we need to reduce traffic, the service is accidentally exposed publicly, rolling the log key (guid) can instantly reduce and drop traffic.  

```
https://localhost:7127/up/log?logKey=3484991B-8304-4B3E-8784-BBDABF6DE346
```