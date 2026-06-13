using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace MrJB.AvailabilityChecks;

public static class Builder
{
    /******************************************/
    /*          azure app config              */
    /******************************************/

    public static TBuilder ConfigureAzureAppConfiguration<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // azure app config
        var connStr = builder.Configuration.GetValue<string>("AZ_APPCONFIG_CONNECTION_STRING");
        var labelFilter = builder.Configuration.GetValue<string>("AZ_APPCONFIG_LABEL_FILTER");

        // client id & secret
        var tenantId = builder.Configuration.GetValue<string>("AZ_TENANT_ID");
        var clientId = builder.Configuration.GetValue<string>("AAD_CLIENT_ID");
        var secret = builder.Configuration.GetValue<string>("AAD_CLIENT_SECRET");

        // validate configuration settings
        if (String.IsNullOrWhiteSpace(connStr) ||
            String.IsNullOrWhiteSpace(labelFilter) ||
            String.IsNullOrWhiteSpace(tenantId) ||
            String.IsNullOrWhiteSpace(clientId) ||
            String.IsNullOrWhiteSpace(secret)
            )
        {
            Console.WriteLine("Azure App Configuration & Key Vault settings not found.");
            return builder;
        }

        Console.WriteLine($"Setting up Azure App Config & Key Vault. App Config Label: {labelFilter}");

        // credentials
        var credentials = new ClientSecretCredential(tenantId, clientId, secret);

        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            // label
            options.Select(KeyFilter.Any, labelFilter);

            options.Connect(connStr).ConfigureKeyVault(kv =>
            {
                kv.SetCredential(credentials);
            });
        });
  
        return builder;
    }
}
