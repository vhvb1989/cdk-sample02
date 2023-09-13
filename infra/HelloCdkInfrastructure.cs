using Cdk.Core;
using Cdk.KeyVault;
using Cdk.ResourceManager;
using Cdk.Sql;
using Azure.Core;

namespace Hello.Cdk
{
    public class HelloCdkInfrastructure : Infrastructure
    {
        public HelloCdkInfrastructure(AzureLocation? location = default)
        {            
            ResourceGroup resourceGroup = new ResourceGroup(Resource.SubscriptionScope);
            Resources.Add(resourceGroup);

            var keyVault = new KeyVault(resourceGroup, "kv");

            // KeyVaultSecret sqlAdminSecret = new KeyVaultSecret(keyVault, "sqlAdminPassword");
            // sqlAdminSecret.AssignParameter(nameof(sqlAdminSecret.Properties.Properties.Value), sqlAdminPasswordParam);

            // KeyVaultSecret appUserSecret = new KeyVaultSecret(keyVault, "appUserPassword");
            // appUserSecret.AssignParameter(nameof(appUserSecret.Properties.Properties.Value), appUserPasswordParam);

            // SqlServer sqlServer = new SqlServer(resourceGroup, "hello-adk-sqlserver", location: location);
            // sqlServer.AssignParameter(nameof(sqlServer.Properties.AdministratorLoginPassword), sqlAdminPasswordParam);
        }
    }
}
