using Cdk.Core;
using Azure.Core;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.KeyVault.Models;

namespace Cdk.KeyVault
{
    public class KeyVault : Resource<KeyVaultData>
    {
        public override string Name { get; } = $"kv{Infrastructure.Seed.Replace("-","")}";

        public KeyVault(Resource scope, string name, string version = "2023-02-01", AzureLocation? location = default)
            : base(scope, version, ArmKeyVaultModelFactory.KeyVaultData(
                name: name is null ? $"kv-{Infrastructure.Seed}" : $"{name}-{Infrastructure.Seed}",
                resourceType: "Microsoft.KeyVault/vaults",
                location: location ?? Environment.GetEnvironmentVariable("AZURE_LOCATION") ?? AzureLocation.WestUS,
                properties: ArmKeyVaultModelFactory.KeyVaultProperties(
                    tenantId: Guid.Parse(Environment.GetEnvironmentVariable("AZURE_TENANT_ID")!),
                    sku: new KeyVaultSku(KeyVaultSkuFamily.A, KeyVaultSkuName.Standard),
                    accessPolicies: Environment.GetEnvironmentVariable("AZURE_PRINCIPAL_ID") is not null ? new List<KeyVaultAccessPolicy>()
                    {
                        new KeyVaultAccessPolicy(Guid.Parse(Environment.GetEnvironmentVariable("AZURE_TENANT_ID")!), Environment.GetEnvironmentVariable("AZURE_PRINCIPAL_ID"), new IdentityAccessPermissions()
                        {
                            Secrets =
                            {
                                IdentityAccessSecretPermission.Get,
                                IdentityAccessSecretPermission.List
                            }
                        })
                    } : default)))
        {
        }

        public void ParameterizeAccessPolicyObjectId(string instance, Parameter parameter)
        {
            ParameterOverrides.Add("objectId", parameter.Name);
            Parameters.Add(parameter);
        }
    }
}
